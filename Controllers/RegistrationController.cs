using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using eVoucherAPI.Models;
using eVoucherAPI.Repository;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;

namespace eVoucherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : BaseController
    {
        public RegistrationController(IRepositoryWrapper repositoryWrapper, IConfiguration configuration) : base(repositoryWrapper, configuration)
        {
        }

        [HttpPost]
        public async Task<dynamic> RegisterUser([FromBody] Newtonsoft.Json.Linq.JObject param)
        {
            try
            {
                //get data from body
                dynamic objparam = param;
                string Password = objparam.Password;
                string Name = objparam.Name;
                string PhoneNumber = objparam.PhoneNumber;

                //search user for duplicate phone no
                int eid = await _repositoryWrapper.User.GetUserIdByPhone(PhoneNumber);
                if (eid > 0)
                {
                    await _repositoryWrapper.EventLog.Warning("User with PhoneNumber: " + PhoneNumber + " already exists", "Registration >> POST");
                    return StatusCode(StatusCodes.Status409Conflict, new { error = "User with PhoneNumber: " + PhoneNumber + " already exists" });
                }
                else if (string.IsNullOrEmpty(Password))
                {
                    await _repositoryWrapper.EventLog.Warning("Empty password", "Registration >> POST");
                    return StatusCode(StatusCodes.Status422UnprocessableEntity, new { error = "Password cannot be empty" });
                }

                //create new user
                User objUser = new User();
                objUser.Name = Name;
                objUser.PhoneNumber = PhoneNumber;
                objUser.Passwordsalt = Util.SaltedHash.GenerateSalt();
                objUser.Password = Util.SaltedHash.ComputeHash(objUser.Passwordsalt, Password);
                objUser.BuyCount = 0;
                objUser.GiftCount = 0;

                Validator.ValidateObject(objUser, new ValidationContext(objUser), true); //validate for database

                await _repositoryWrapper.User.CreateAsync(objUser, true); //add user to database
                await _repositoryWrapper.EventLog.Insert(objUser, "Registration >> POST");

                return StatusCode(StatusCodes.Status201Created, new { data = new { objUser.Id, objUser.Name, objUser.PhoneNumber }});
            }
            catch (ValidationException vex)
            {
                await _repositoryWrapper.EventLog.Error("Register Validation Error", vex.Message, "Registration >> POST");
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new { error = vex.ValidationResult.ErrorMessage });
            }
            catch (Exception ex)
            {
                await _repositoryWrapper.EventLog.Error("Registration Failed", ex.Message, "Registration >> POST");
                return StatusCode(StatusCodes.Status400BadRequest, new { error = "Something went wrong" });
            }
        }
    }
}
