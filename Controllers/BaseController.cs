using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using eVoucherAPI.Util;
using eVoucherAPI.Models;

namespace eVoucherAPI.Controllers
{

    [Route("api/[controller]")]
    public class BaseController : Controller
    {
        public TokenData _tokenData = new TokenData();
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            setDefaultDataFromToken();
        }

        public void setDefaultDataFromToken()
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
            }
        }

    
    }
}