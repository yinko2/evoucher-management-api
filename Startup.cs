using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using eVoucherAPI.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using eVoucherAPI.CustomTokenAuthProvider;

namespace eVoucherAPI
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            StaticConfig = Configuration;
        }

        public IConfiguration Configuration { get; }
        public static IConfiguration StaticConfig { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureCors();

            services.ConfigureMySqlContext(Configuration);
            
            services.ConfigureRepositoryWrapper();
            services.ConfigureIISIntegration();

            services.AddTransient<TokenProviderMiddleware>();
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddMvc(option => option.EnableEndpointRouting = false)
             .AddNewtonsoftJson(o => {
                o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
                o.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;		//it must be Include, otherwise default value (boolean=false, int=0, int?=null, object=null) will be missing in response json			
                o.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddControllers().AddJsonOptions(options => {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            services.AddDistributedMemoryCache();
            services.ConfigureModelBindingExceptionHandling();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(jwt =>
            {
                var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("TokenAuthentication:SecretKey").Value));

                jwt.TokenValidationParameters = new TokenValidationParameters
                {

                    // The signing key must match!
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    RequireSignedTokens = true,
                    // Validate the JWT Issuer (iss) claim
                    ValidateIssuer = true,
                    ValidIssuer = Configuration.GetSection("TokenAuthentication:Issuer").Value,
                    // Validate the JWT Audience (aud) claim
                    ValidateAudience = true,
                    ValidAudience = Configuration.GetSection("TokenAuthentication:Audience").Value,
                    // Validate the token expiry
                    ValidateLifetime = true,
                    // If you want to allow a certain amount of clock drift, set that here:
                    ClockSkew = TimeSpan.Zero
                };

                // double expiretimespan = Convert.ToDouble(Configuration.GetSection("TokenAuthentication:TokenExpire").Value);
            });
            
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("CorsAllowAllPolicy");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthentication();

            app.UseTokenProviderMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
