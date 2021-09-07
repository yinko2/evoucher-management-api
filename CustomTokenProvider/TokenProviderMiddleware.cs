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

        public TokenProviderMiddleware(IHttpContextAccessor httpContextAccessor, IRepositoryWrapper repository, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = repository;
            _configuration = configuration;
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

            //Check Token
            var hdtoken = context.Request.Headers["Authorization"];
            if (hdtoken.Count > 0)
            {
                access_token = hdtoken[0];
                access_token = access_token.Replace("Bearer ", string.Empty); //Get Token
                
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var tokenS = handler.ReadToken(access_token) as JwtSecurityToken;
                    _tokenData = Globalfunction.GetTokenData(tokenS);
                }
                catch (Exception ex)
                {
                    //Read Token Error
                    await _repository.EventLog.Error("Invalid Token", ex.Message, "TokenProviderMiddleware >> InvokeAsync");
                    await ResponseMessage(new { error = "Invalid Token" }, context, StatusCodes.Status401Unauthorized);
                    return;
                }
            }
        
            if (context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                await GenerateToken(context); // login with api/token
            }
            else if(context.Request.Path.Equals("/", StringComparison.Ordinal))
            {
                //default landing page
                await ResponseMessage(new { healthy = true }, context, StatusCodes.Status200OK);
            }
            else
            {
                string strPath = context.Request.Path.ToString();
                
                // you can add with || multiple publically available functions to skip login
                if (strPath.Equals("/api/registration", StringComparison.OrdinalIgnoreCase))
                {
                    await next(context);
                }
                else if (string.IsNullOrEmpty(access_token))
                {
                    await _repository.EventLog.Warning("Token not found", "TokenProviderMiddleware >> InvokeAsync");
                    await ResponseMessage(new { error = "Token not found" }, context, StatusCodes.Status401Unauthorized);
                }
                else
                {
                    //Regenerate newtoken for not timeout at running
                    string newToken = "";
                    try
                    {
                        // check token expired   
                        double expireTime = Convert.ToDouble(_options.Expiration.TotalMinutes);
                        DateTime issueDate = _tokenData.TicketExpireDate.AddMinutes(-expireTime);
                        DateTime NowDate = DateTime.UtcNow;
                        if (issueDate > NowDate || _tokenData.TicketExpireDate < NowDate)
                        {
                            // throw new Exception("Invalid Token Expire, Issue Date: " + issueDate.ToString());
                            await _repository.EventLog.Warning("The Token has expired", "TokenProviderMiddleware >> InvokeAsync");
                            await ResponseMessage(new { error = "The Token has expired" }, context, StatusCodes.Status401Unauthorized);
                            return;
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
                        if (!string.IsNullOrEmpty(newToken))
                        {
                            context.Response.Headers.Add("Access-Control-Expose-Headers", "NewToken");
                            context.Response.Headers.Add("NewToken", newToken);
                            await next(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        Globalfunction.WriteSystemLog("InvokeAsync: " + ex.Message);
                        await _repository.EventLog.Error("New Token Generation Failed", ex.Message, "TokenProviderMiddleware >> InvokeAsync");
                        await ResponseMessage(new { error = "Something went wrong" }, context, StatusCodes.Status401Unauthorized);
                        return;
                    }
                }
            }            
        }

        private async Task GenerateToken(HttpContext context)
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
                        await _repository.EventLog.Error("Invalid login credentials", "PhNo:" + userData.PhoneNumber + ", Password: " + userData.Password, "Authentication >> GenerateToken");
                        await ResponseMessage(new { error = "Invalid login credentials" }, context, StatusCodes.Status422UnprocessableEntity);
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
                await _repository.EventLog.Error("Failed to read login credentials", ex.Message, "Authentication >> GenerateToken");
                Globalfunction.WriteSystemLog("GenerateToken: " + ex.Message);
                await ResponseMessage(new { error = "Invalid login credentials" }, context, StatusCodes.Status400BadRequest);
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
                    await ResponseMessage(new { error = error_msg }, context, StatusCodes.Status401Unauthorized);
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
                    user_id = UserID,
                    name = UserName
                };

                var response = new
                {
                    data = tokeninfo
                };
                
                await ResponseMessage(response, context, StatusCodes.Status200OK);
                 
                await _repository.EventLog.Info("Successful login for this account Ph No: " + phoneno, "GenerateToken");
            }
            catch(Exception ex) 
            {
                Globalfunction.WriteSystemLog("Generate Token Fail: " + phoneno + ", Error: " + ex.Message); 
                await _repository.EventLog.Error("Generate Token Fail for" + phoneno, ex.Message, "GenerateToken");; 
                await ResponseMessage(new { error = "Generate Token Fail" }, context, StatusCodes.Status401Unauthorized);
                return;
            }
        }

        private async Task<dynamic> dologinValidation(string phonenumber, string password)
        {
            try 
            {
                User result = await _repository.User.GetUserByPhone(phonenumber);
                if (result == null)
                {
                    await _repository.EventLog.Error("User not found with " + phonenumber, "", "TokenProviderMiddleware >> doLoginValidation");
                    return new { error = 1, message = "User not found with " + phonenumber };
                }
    
                string oldhash = result.Password; 
                string oldsalt = result.Passwordsalt; 
                bool flag = SaltedHash.Verify(oldsalt, oldhash, password);
                if (flag)
                {
                    return new { error = 0, data = result };
                }
                await _repository.EventLog.Error("Password Validation Failed with " + phonenumber, "", "TokenProviderMiddleware >> doLoginValidation");
                return new { error = 1, message = "Password Validation Failed"};
            }
            // catch (ValidationException vex)
            // {
            //     Globalfunction.WriteSystemLog("dologinValidation: " + vex.Message);
            //     await _repository.EventLog.Error("Login Validation Failed with " + phonenumber, vex.Message, "doLoginValidation");
            //     return new { error = 1, message = "Login Validation Failed"};
            // }
            catch(Exception ex) 
            {
                Globalfunction.WriteSystemLog("dologinValidation: " + ex.Message);
                await _repository.EventLog.Error("Login Fail with PhNo: " + phonenumber, ex.Message, "TokenProviderMiddleware >> doLoginValidation");
                return new { error = 1, message = "Login Failed"};
            }
        }

        public async Task ResponseMessage(object data, HttpContext context, int code = StatusCodes.Status400BadRequest)
        {
            context.Response.StatusCode = code;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(data, _serializerSettings));
        }
    }
}