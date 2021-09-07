using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using eVoucherAPI.Util;
using eVoucherAPI.Models;
using eVoucherAPI.Repository;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace eVoucherAPI.Controllers
{

    [Route("api/[controller]")]
    public class BaseController : Controller
    {
        public TokenData _tokenData = new TokenData();
        public readonly IRepositoryWrapper _repositoryWrapper;
        public IConfiguration _configuration;

        public BaseController(IRepositoryWrapper repositoryWrapper, IConfiguration configuration)
        {
            _repositoryWrapper = repositoryWrapper;
            _configuration = configuration;            
        }
        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            await setDefaultDataFromToken();
        }

        public async Task setDefaultDataFromToken()
        {
            try
            {
                string access_token = "";
                var hdtoken = Request.Headers["Authorization"];
                if (hdtoken.Count > 0)
                {
                    access_token = hdtoken[0];
                    access_token = access_token.Replace("Bearer ", "");
                    var handler = new JwtSecurityTokenHandler();
                    var tokenS = handler.ReadToken(access_token) as JwtSecurityToken;
                    _tokenData = Globalfunction.GetTokenData(tokenS);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _repositoryWrapper.EventLog.Error("Read token error", ex.Message, "Base >> setDefaultDataFromToken");
            }
        }   
    }
}