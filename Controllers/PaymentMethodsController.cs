using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using eVoucherAPI.Models;
using eVoucherAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;

namespace eVoucherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodsController : BaseController
    {
        public PaymentMethodsController(IRepositoryWrapper repositoryWrapper, IConfiguration configuration) : base(repositoryWrapper, configuration)
        {
        }

        [HttpGet]
        public async Task<dynamic> GetPaymentMethods()
        {
            try
            {
                var result = await _repositoryWrapper.Payment.FindAllAsync();
                if (result.Any())
                    return Ok(new { data = result });
                return NotFound();
            }
            catch (Exception ex) {
                await _repositoryWrapper.EventLog.Error("get payments fail", ex.Message, "Get paymentmethods");
                return BadRequest(new { error = "Something went wrong" });
            }
        }
    }
}
