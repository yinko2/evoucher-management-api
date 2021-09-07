using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors.Infrastructure;
using eVoucherAPI.Models;
using eVoucherAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace eVoucherAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            var corsBuilder = new CorsPolicyBuilder();
            corsBuilder.AllowAnyHeader();
            corsBuilder.AllowAnyMethod();
            corsBuilder.AllowAnyOrigin(); // For anyone access.
            // corsBuilder.WithOrigins("http://localhost:4200","http://localhost"); // for a specific url. Don't add a forward slash on the end!
            // corsBuilder.AllowCredentials();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsAllowAllPolicy", corsBuilder.Build());
            });
        }

        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options => 
            {
            });
        }

        public static void ConfigureMySqlContext(this IServiceCollection services, IConfiguration config)
        {
            
            var connectionString = config["ConnectionStrings:DefaultConnection"];
            
            services.AddDbContext<eVoucherContext>(opt => opt.UseMySql(connectionString, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.11-mysql")));
        }

        public static void ConfigureRepositoryWrapper(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
        }

        //it is for error handler of model validation exception when direct bind request parameter to model in controller function
        public static void ConfigureModelBindingExceptionHandling(this IServiceCollection services) 
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    ValidationProblemDetails error = actionContext.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .Select(e => new ValidationProblemDetails(actionContext.ModelState)).FirstOrDefault();
            
                    string ErrorMessage = "";
                    foreach (KeyValuePair<string, string[]>  errobj in error.Errors) {
                        foreach(string s in errobj.Value) {
                            ErrorMessage = ErrorMessage + s + "\r\n";
                        }
                    }
                    return new BadRequestObjectResult(new { data = 0, error = ErrorMessage});
                };
            });
        }

    }
}
