using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using eVoucherAPI.Models;
using eVoucherAPI.Repository;
using Microsoft.Extensions.Configuration;

namespace eVoucherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstoreController : BaseController
    {
        public EstoreController(IRepositoryWrapper repositoryWrapper, IConfiguration configuration) : base(repositoryWrapper, configuration)
        {
        }

        [HttpGet("evoucherlist")]
        public async Task<dynamic> getEvouchers()
        {
            try
            {
                int userId = int.Parse(_tokenData.UserID);
                var elist = await _repositoryWrapper.Evoucher.FindVoucherListByUserId(userId);
                return Ok(new { status = "success", data = elist });
            }
            catch (Exception ex) {
                await _repositoryWrapper.EventLog.Error("get evouchers fail", ex.Message, "Estore >> getEvouchers");
                return BadRequest(new { status = "fail", message = "Something went wrong." });
            }
        }

        [HttpGet("evoucher/{id}")]
        public async Task<dynamic> GetEvoucherbyId(int id)
        {
            try
            {
                int userId = int.Parse(_tokenData.UserID);
                var obj = await _repositoryWrapper.Evoucher.FindVoucherById(id, userId);
                if (obj == null)
                {
                    return NotFound(new { status = "fail", message = "Voucher Not Found." });
                }
                return Ok(new { status = "success", data = obj });
            }
            catch (Exception ex) {
                await _repositoryWrapper.EventLog.Error("get evoucher by id fail", ex.Message, "Estore >> GetEvoucherbyId");
                return BadRequest(new { status = "fail", message = "Something went wrong." });
            }
        }

        [HttpPost("verifypromo")]
        public async Task<dynamic> VerifyPromoCode([FromBody] Newtonsoft.Json.Linq.JObject param)
        {
            try
            {
                dynamic objparam = param;
                string promocode = objparam.PromoCode.ToString();

                int userId = int.Parse(_tokenData.UserID);

                Evoucher obj = await _repositoryWrapper.Evoucher.FindVoucherByPromoCode(promocode);
                if (obj == null || obj.UserId != userId)
                {
                    return BadRequest(new { status = "fail", message = "Voucher Not Found" });
                }
                else if (obj.Isused)
                {
                    return BadRequest(new { status = "fail", message = "Voucher is already used" });
                }
                else if (!obj.Isactive)
                {
                    return BadRequest(new { status = "fail", message = "Voucher do not exist" });
                }
                else
                {
                    return Ok(new { status = "success", data = true });
                }
            }
            catch (Exception ex) {
                await _repositoryWrapper.EventLog.Error("Verify PromoCode fail", ex.Message, "CMS >> VerifyPromoCode");
                return BadRequest(new { status = "fail", message = "Something went wrong." });
            }
        }

        [HttpGet("purchasehistory")]
        public async Task<dynamic> GetPurchaseHistory()
        {
            try
            {
                int userId = int.Parse(_tokenData.UserID);

                var obj = await _repositoryWrapper.Purchase.GetPurchaseHistoryByUserId(userId);
                return Ok(new { status = "success", data = obj });
            }
            catch (Exception ex) {
                await _repositoryWrapper.EventLog.Error("Get PurchaseHistory fail", ex.Message, "Estore >> GetPurchaseHistory");
                return BadRequest(new { status = "fail", message = "Something went wrong." });
            }
        }

        [HttpGet("voucherlistbypurchase/{id}")]
        public async Task<dynamic> GetVoucherListByPurchase(int id)
        {
            try
            {
                int userId = int.Parse(_tokenData.UserID);

                IEnumerable<dynamic> obj = await _repositoryWrapper.Purchase.GetVoucherListByPurchaseId(id, userId);
                if (obj.Count() == 0)
                {
                    return BadRequest(new { status = "fail", message = "data not found" });
                }
                var Unused = obj.Where(u => u.Isused == false);
                var Used = obj.Where(u => u.Isused == true);
                return Ok(new { status = "success", data = new { UnusedVouchers = Unused, UsedVouchers = Used }});
            }
            catch (Exception ex) {
                await _repositoryWrapper.EventLog.Error("Get VoucherList By Purchase fail", ex.Message, "Estore >> GetVoucherListByPurchase");
                return BadRequest(new { status = "fail", message = "Something went wrong." });
            }
        }
    }
}
