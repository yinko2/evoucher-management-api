using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using eVoucherAPI.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using eVoucherAPI.Models;
using eVoucherAPI.Util;
using System.ComponentModel.DataAnnotations;

namespace eVoucherAPI.CustomTokenAuthProvider
{
    public class TokenProviderMiddleware : IMiddleware
    {
        private IRepositoryWrapper _repository;
        private readonly TokenProviderOptions _options;
        private readonly JsonSerializerSettings _serializerSettings;
        private IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;

        public TokenProviderMiddleware(IHttpContextAccessor httpContextAccessor, ILoggerFactory DepLoggerFactory, IRepositoryWrapper repository, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = repository;
            _configuration = configuration;
            var requestPath = _httpContextAccessor.HttpContext.Request.Path.ToString();
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            double expiretimespan = Convert.ToDouble(_configuration.GetSection("TokenAuthentication:TokenExpire").Value);
            TimeSpan expiration = TimeSpan.FromMinutes(expiretimespan);
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("TokenAuthentication:SecretKey").Value));
            _options = new TokenProviderOptions
            {
                Path = _configuration.GetSection("TokenAuthentication:TokenPath").Value,
                Audience = _configuration.GetSection("TokenAuthentication:Audience").Value,
                Issuer = _configuration.GetSection("TokenAuthentication:Issuer").Value,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                Expiration = expiration
            };
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            TokenData _tokenData = null;
            var access_token = string.Empty;
            var hdtoken = context.Request.Headers["Authorization"];
            if (hdtoken.Count > 0)
            {
                access_token = hdtoken[0];
                access_token = access_token.Replace("Bearer ", string.Empty);
                
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var tokenS = handler.ReadToken(access_token) as JwtSecurityToken;
                    _tokenData = Globalfunction.GetTokenData(tokenS);
                }
                catch (Exception ex)
                {
                    _repository.EventLog.AddEventLog(EventLogTypes.Error, "Invalid Token", ex.Message, "Authentication >> CheckToken");
                    await ResponseMessage(new { status = "fail", message = "Invalid Token" }, context, 401);
                    return;
                }
            }
        
            if (context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                await GenerateToken(context, next);
            }
            else if(context.Request.Path.Equals("/", StringComparison.Ordinal))
            {
                await next(context);  ///skip default landing page
            }
            else
            {
                string strPath = context.Request.Path.ToString();
                // if(context.Request.Path.ToString().Split("/").Length > 3) {
                //     methodName = context.Request.Path.ToString().Split("/")[3];
                // }
                
                // you can add with || multiple public available functions to skip login
                if (strPath == "/api/user/register")
                {
                    await next(context);
                }

                if (access_token == "")
                {
                    await ResponseMessage(new { status = "fail", message = "Token not found" }, context, 401);
                }
                else
                {
                    //Regenerate newtoken for not timeout at running
                    string newToken = "";
                    try
                    {
                        var pathstr = context.Request.Path.ToString();
                        string[] patharr = pathstr.Split('/');

                        // check token expired   
                        double expireTime = Convert.ToDouble(_options.Expiration.TotalMinutes);
                        DateTime issueDate = _tokenData.TicketExpireDate.AddMinutes(-expireTime);
                        DateTime NowDate = DateTime.UtcNow;
                        if (issueDate > NowDate || _tokenData.TicketExpireDate < NowDate)
                        {
                            newToken = "-2";
                            throw new Exception("Invalid Token Expire, Issue Date: " + issueDate.ToString());
                        }
                        
                        // end of token expired check

                        var now = DateTime.UtcNow;
                        _tokenData.TicketExpireDate = now.Add(_options.Expiration);
                        var claims = Globalfunction.GetClaims(_tokenData);

                        // Create the JWT and write it to a string
                        var jwt = new JwtSecurityToken(
                            issuer: _options.Issuer,
                            audience: _options.Audience,
                            claims: claims,
                            notBefore: now,
                            expires: now.Add(_options.Expiration),
                            signingCredentials: _options.SigningCredentials);
                        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                        newToken = encodedJwt;
                    }
                    catch (Exception ex)
                    {
                        Globalfunction.WriteSystemLog("InvokeAsync: " + ex.Message);
                    }

                    if (newToken == "-2")
                    {
                        await ResponseMessage(new { status = "fail", message = "The Token has expired" }, context, 401);
                    }
                    else if (newToken != "")
                    {
                        context.Response.Headers.Add("Access-Control-Expose-Headers", "NewToken");
                        context.Response.Headers.Add("NewToken", newToken);
                        await next(context);
                    }
                }
            }            
        }

        private async Task GenerateToken(HttpContext context, RequestDelegate next)
        {
            User userData = new User();
            string phoneno = "";
            string password = "";

            try
            {
                using (var reader = new System.IO.StreamReader(context.Request.Body))
                {
                    var request_body = reader.ReadToEnd();
                    userData = JsonConvert.DeserializeObject<User>(request_body, _serializerSettings);
                    if (string.IsNullOrEmpty(userData.PhoneNumber) || string.IsNullOrEmpty(userData.Password))
                    {
                        _repository.EventLog.AddEventLog(EventLogTypes.Error, "Invalid login credentials", "", "Authentication >> GenerateToken");
                        await ResponseMessage(new { status = "fail", message = "Invalid login credentials" }, context, 400);
                        return;
                    }
                    // phoneno = Encryption.DecryptClient_String(userData.PhoneNumber);
                    // password = Encryption.DecryptClient_String(userData.Password);
                    phoneno = userData.PhoneNumber;
                    password = userData.Password;
                }
            }
            catch (Exception ex)
            {
                _repository.EventLog.AddEventLog(EventLogTypes.Error, "Failed to read login credentials", ex.Message, "Authentication >> GenerateToken");
                Globalfunction.WriteSystemLog("GenerateToken: " + ex.Message);
                await ResponseMessage(new { status = "fail", message = "Invalid login credentials" }, context, 400);
                return;
            }

            try 
            {
                dynamic loginresult = null;
                int UserID;
                string UserName;
                string PhoneNumber;

                loginresult = await dologinValidation(phoneno, password);
                if(loginresult.error == 0) 
                {
                    loginresult = loginresult.data;
                    UserID = loginresult.Id;
                    UserName = loginresult.Name;
                    PhoneNumber = loginresult.PhoneNumber;
                }
                else 
                {
                    string error_msg = loginresult.message.ToString();
                    await ResponseMessage(new { status = "fail", message = error_msg }, context, 400);
                    return;
                }

                string userID = UserID.ToString();
                var now = DateTime.UtcNow;
                var _tokenData = new TokenData();
                _tokenData.Sub = UserName;
                _tokenData.Jti = await _options.NonceGenerator();
                _tokenData.Iat = new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString();
                _tokenData.UserID = userID;
                _tokenData.UserName = UserName;
                _tokenData.TicketExpireDate = now.Add(_options.Expiration);
                var claims = Globalfunction.GetClaims(_tokenData);

                var appIdentity = new ClaimsIdentity(claims);
                context.User.AddIdentity(appIdentity);

                // Create the JWT and write it to a string
                var jwt = new JwtSecurityToken(
                    issuer: _options.Issuer,
                    audience: _options.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now.Add(_options.Expiration),
                    signingCredentials: _options.SigningCredentials);

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                var tokeninfo = new
                {
                    access_token = encodedJwt,
                    expires_in_seconds = (int)_options.Expiration.TotalSeconds,
                    name = UserName
                };

                var response = new
                {
                    status = "success",
                    data = tokeninfo
                };
                
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
                 
                _repository.EventLog.AddEventLog(EventLogTypes.Info, "Successful login for this account Ph No: " + phoneno, "", "GenerateToken");
            }
            catch(Exception ex) 
            {
                Globalfunction.WriteSystemLog("Generate Token Fail: " + phoneno + ", Error: " + ex.Message); 
                _repository.EventLog.AddEventLog(EventLogTypes.Error, "Generate Token Fail for" + phoneno, ex.Message, "GenerateToken");; 
                await ResponseMessage(new { status = "fail", message = "Generate Token Fail" }, context, 401);
                return;
            }
        }

        async Task<dynamic> dologinValidation(string phonenumber, string password)
        {
            try {
                User result = await _repository.User.GetUserByPhone(phonenumber);
                if (result == null)
                {
                    _repository.EventLog.AddEventLog(EventLogTypes.Error, "User not found with " + phonenumber, "", "doLoginValidation");
                    return new { error = 1, message = "User not found"};
                }
    
                string oldhash = result.Password; 
                string oldsalt = result.Passwordsalt; 
                bool flag = SaltedHash.Verify(oldsalt, oldhash, password);
                if (flag)
                {
                    return new {error = 0, data = result};
                }
                _repository.EventLog.AddEventLog(EventLogTypes.Error, "Password Validation Failed with " + phonenumber, "", "doLoginValidation");
                return new { error = 1, message = "Password Validation Failed"};
            }
            catch (ValidationException vex)
            {
                Globalfunction.WriteSystemLog("dologinValidation: " + vex.Message);
                _repository.EventLog.AddEventLog(EventLogTypes.Error, "Login Validation Failed with " + phonenumber, vex.Message, "doLoginValidation");
                return new { error = 1, message = "Login Validation Failed"};
            }
            catch(Exception ex) {
                Globalfunction.WriteSystemLog("dologinValidation: " + ex.Message);
                _repository.EventLog.AddEventLog(EventLogTypes.Error, "Login Fail : " + phonenumber, ex.Message, "Login");
                return new { error = 1, message = "Login Fail"};
            }
        }

        public async Task ResponseMessage(dynamic data, HttpContext context, int code = 400)
        {
            var response = new
            {
                status = data.status,
                message = data.message
            };
            context.Response.StatusCode = code;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
        }
    }
}