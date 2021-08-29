using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using eVoucherAPI.Models;
using eVoucherAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace eVoucherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UserController(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<dynamic> RegisterUser([FromBody] Newtonsoft.Json.Linq.JObject param)
        {
            try
            {
                dynamic objparam = param;
                string Password = objparam.Password.ToString();
                string Name = objparam.Name.ToString();
                string PhoneNumber = objparam.PhoneNumber.ToString();

                int eid = await _repositoryWrapper.User.GetUserIdByPhone(PhoneNumber);
                if (eid > 0)
                {
                     _repositoryWrapper.EventLog.Warning("User with PhNo: " + PhoneNumber + " already exists", "User >> Register");
                    return BadRequest(new { status = "fail", message = "User with PhNo: " + PhoneNumber + " already exists" });
                }
                User objUser = new User();
                objUser.Name = Name;
                objUser.PhoneNumber = PhoneNumber;
                objUser.Passwordsalt = Util.SaltedHash.GenerateSalt();
                objUser.Password = Util.SaltedHash.ComputeHash(objUser.Passwordsalt, Password);
                objUser.BuyCount = 0;
                objUser.GiftCount = 0;

                Validator.ValidateObject(objUser, new ValidationContext(objUser), true);
                await _repositoryWrapper.User.CreateAsync(objUser, true);
                 _repositoryWrapper.EventLog.Insert(objUser, "User >> Register");
                return new { status = "success", data = true };
            }
            catch (ValidationException vex)
            {
                return BadRequest(vex.Message);
            }
        }
    }
}
