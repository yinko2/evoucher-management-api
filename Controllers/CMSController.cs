using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using eVoucherAPI.Models;
using eVoucherAPI.Repository;
using System.ComponentModel.DataAnnotations;
using eVoucherAPI.Util;
using Microsoft.Extensions.Configuration;

namespace eVoucherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CMSController : BaseController
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private IConfiguration _configuration;

        public CMSController(IRepositoryWrapper repositoryWrapper, IConfiguration configuration)
        {
            _repositoryWrapper = repositoryWrapper;
            _configuration = configuration;
        }

        // [HttpGet("checkPurchaseCount")]
        // public async Task<dynamic> checkPurchaseCount()
        // {
        //     try
        //     {
        //         int userId = int.Parse(_tokenData.UserID);
        //         User obj = await _repositoryWrapper.User.FindByIDAsync(userId);
        //         string purchaseLimit = _configuration.GetSection("appSettings:purchaseLimit").Value;
        //         if (obj.BuyCount > Convert.ToInt32(purchaseLimit))
        //             return Ok(new { status = "success", data = true});
        //         return Ok(new { status = "success", data = false} );
        //     }
        //     catch (Exception ex) {
        //         _repositoryWrapper.EventLog.Error("Add purchase fail", ex.Message, "CMS >> checkPurchaseCount");
        //         return BadRequest(new { status = "fail", message = "Something went wrong." });
        //     }
        // }

        // [HttpGet("checkGiftCount")]
        // public async Task<dynamic> checkGiftCount()
        // {
        //     try
        //     {
        //         int userId = int.Parse(_tokenData.UserID);
        //         User obj = await _repositoryWrapper.User.FindByIDAsync(userId);
        //         string giftLimit = _configuration.GetSection("appSettings:giftLimit").Value;
        //         if (obj.GiftCount > Convert.ToInt32(giftLimit))
        //             return Ok(new { status = "success", data = true});
        //         return Ok(new { status = "success", data = false} );
        //     }
        //     catch (Exception ex) {
        //         _repositoryWrapper.EventLog.Error("Add purchase fail", ex.Message, "CMS >> checkGiftCount");
        //         return BadRequest(new { status = "fail", message = "Something went wrong." });
        //     }
        // }

        [HttpPost("verifyphone")]
        public async Task<dynamic> VerifyPhoneForGift([FromBody] Newtonsoft.Json.Linq.JObject param)
        {
            try
            {
                dynamic objparam = param;
                string phoneno = objparam.PhoneNumber.ToString();
                User userobj = await _repositoryWrapper.User.GetUserByPhone(phoneno);
                if (userobj != null)
                    return Ok(new { status = "success", data = new { UserName = userobj.Name}});
                return BadRequest(new { status = "fail", message = "User With PhoneNo: "+ phoneno + " Not Found"} );
            }
            catch (Exception ex) {
                _repositoryWrapper.EventLog.Error("Add purchase fail", ex.Message, "CMS >> VerifyPhoneForGift");
                return BadRequest(new { status = "fail", message = "Something went wrong." });
            }
        }

        [HttpPost("purchasevoucher")]
        public async Task<dynamic> PurchaseEVoucher(Purchase obj)
        {
            try
            {
                int userId = int.Parse(_tokenData.UserID);
                obj.UserId = userId;
                obj.IsPaid = false;
                User userobj = await _repositoryWrapper.User.FindByIDAsync(userId);
                string purchaseLimit = _configuration.GetSection("appSettings:purchaseLimit").Value;
                string giftLimit = _configuration.GetSection("appSettings:giftLimit").Value;
                
                if (obj.BuyTypeId == 2)
                {
                    if (string.IsNullOrEmpty(obj.GiftUserPhone))
                    {
                        return BadRequest(new { status = "fail", message = "Need gift user phonenumber"});
                    }

                    int gid = await _repositoryWrapper.User.GetUserIdByPhone(obj.GiftUserPhone);
                    if (gid == 0)
                    {
                        _repositoryWrapper.EventLog.Error("Gift User with PhoneNo: "+ obj.GiftUserPhone +" Not Found", "", "CMS >> PurchaseEVoucher");
                        return BadRequest(new { status = "fail", message = "Gift User Not Found." });
                    }
                    else if (gid == userId)
                    {
                        return BadRequest(new { status = "fail", message = "Cannot gift yourself" });
                    }
                    else if(userobj.GiftCount >= Convert.ToInt32(giftLimit) || userobj.GiftCount + obj.Quantity > Convert.ToInt32(giftLimit))
                    {
                        _repositoryWrapper.EventLog.Error("Gift Limit Reached with PhoneNo: "+ obj.GiftUserPhone, "", "CMS >> PurchaseEVoucher");
                        return BadRequest(new { status = "fail", message = "Gift Limit Reached" });
                    }
                    obj.GiftUserId = gid;
                }
                else
                {
                    if (userobj.BuyCount >= Convert.ToInt32(purchaseLimit) || userobj.BuyCount + obj.Quantity > Convert.ToInt32(purchaseLimit))
                    {
                        _repositoryWrapper.EventLog.Error("Purchase Limit Reached with PhoneNo: "+ userobj.PhoneNumber, "", "CMS >> PurchaseEVoucher");
                        return BadRequest(new { status = "fail", message = "Purchase Limit Reached" });
                    }
                }

                obj.PurchasedDate = DateTime.UtcNow;
                double dc = await _repositoryWrapper.Payment.GetDiscountByPaymentId(obj.PaymentId);
                obj.Discount = dc;
                double total = obj.Quantity * obj.Amount;
                obj.Cost = total - (total * (dc/100));
                Validator.ValidateObject(obj, new ValidationContext(obj), true);
                await _repositoryWrapper.Purchase.CreateAsync(obj, true);
                _repositoryWrapper.EventLog.Insert(obj, "CMS >> PurchaseEVoucher");

                if (!string.IsNullOrEmpty(obj.Image))
                {
                    FileService.MoveTempFile("eVoucherPhoto", obj.Id.ToString(), obj.Image, _configuration);
                    _repositoryWrapper.EventLog.Info("Save eVoucher Photo: " + obj.Id.ToString(), "CMS >> PurchaseEVoucher");
                }

                return Created("", new { status = "success", data = new { PurchaseId = obj.Id } });
            }
            catch (ValidationException vex)
            {
                _repositoryWrapper.EventLog.Error("Add purchase validation fail", vex.Message, "CMS >> PurchaseEVoucher");
                return BadRequest(new { status = "fail", message = vex.ValidationResult.ErrorMessage });
            }
            catch (Exception ex) {
                _repositoryWrapper.EventLog.Error("Add purchase fail", ex.Message, "CMS >> PurchaseEVoucher");
                return BadRequest(new { status = "fail", message = "Something went wrong." });
            }
        }

        [HttpPut("editpurchase")]
        public async Task<dynamic> EditPurchase(Purchase obj)
        {
            try
            {
                Validator.ValidateObject(obj, new ValidationContext(obj), true);
                int userId = int.Parse(_tokenData.UserID);

                Purchase existingpur = await _repositoryWrapper.Purchase.FindByIDAsync(obj.Id);

                if (existingpur == null)
                {
                    throw new Exception("Purchase not found");
                }
                if (existingpur.IsPaid)
                {
                    throw new Exception("Cannot edit payment completed purchase");
                }
                else if (existingpur.UserId != userId)
                {
                    throw new Exception("Invalid Purchase ID");
                }
                else if (obj.BuyTypeId == 2 && string.IsNullOrEmpty(obj.GiftUserPhone))
                {
                    throw new Exception("Need gift user phonenumber");
                }

                existingpur.BuyTypeId = obj.BuyTypeId;
                existingpur.Title = obj.Title;
                existingpur.Description = obj.Description;
                existingpur.Quantity = obj.Quantity;
                existingpur.PaymentId = obj.PaymentId;
                existingpur.Amount = obj.Amount;
                double dc = await _repositoryWrapper.Payment.GetDiscountByPaymentId(obj.PaymentId);
                existingpur.Discount = dc;
                double total = obj.Quantity * obj.Amount;
                existingpur.Cost = total - (total * (dc/100));

                User userobj = await _repositoryWrapper.User.FindByIDAsync(userId);
                string purchaseLimit = _configuration.GetSection("appSettings:purchaseLimit").Value;
                string giftLimit = _configuration.GetSection("appSettings:giftLimit").Value;
                
                if (obj.BuyTypeId == 2)
                {                    
                    int gid = await _repositoryWrapper.User.GetUserIdByPhone(obj.GiftUserPhone);
                    if (gid == 0)
                    {
                        _repositoryWrapper.EventLog.Error("Gift User with PhoneNo: "+ obj.GiftUserPhone +" Not Found", "", "CMS >> PurchaseEVoucher");
                        return BadRequest(new { status = "fail", message = "Gift User Not Found." });
                    }
                    else if (gid == userId)
                    {
                        return BadRequest(new { status = "fail", message = "Cannot gift yourself" });
                    }
                    else if(userobj.GiftCount >= Convert.ToInt32(giftLimit) || userobj.GiftCount + obj.Quantity > Convert.ToInt32(giftLimit))
                    {
                        _repositoryWrapper.EventLog.Error("Gift Limit Reached with PhoneNo: "+ obj.GiftUserPhone, "", "CMS >> PurchaseEVoucher");
                        return BadRequest(new { status = "fail", message = "Gift Limit Reached" });
                    }
                    existingpur.GiftUserId = gid;
                    existingpur.GiftUserPhone = obj.GiftUserPhone;
                }
                else
                {
                    if (userobj.BuyCount >= Convert.ToInt32(purchaseLimit) || userobj.BuyCount + obj.Quantity > Convert.ToInt32(purchaseLimit))
                    {
                        _repositoryWrapper.EventLog.Error("Purchase Limit Reached with PhoneNo: "+ userobj.PhoneNumber, "", "CMS >> PurchaseEVoucher");
                        return BadRequest(new { status = "fail", message = "Purchase Limit Reached" });
                    }
                    existingpur.GiftUserId = null;
                }
                await _repositoryWrapper.Purchase.UpdateAsync(existingpur, true);
                _repositoryWrapper.EventLog.Update(obj, "CMS >> PurchaseEVoucher");

                if (!string.IsNullOrEmpty(obj.Image))
                {
                    FileService.DeleteFileNameOnly("eVoucherPhoto", existingpur.Id.ToString(), _configuration);
                    FileService.MoveTempFile("eVoucherPhoto", existingpur.Id.ToString(), obj.Image.ToString(), _configuration);
                    _repositoryWrapper.EventLog.Info("Save evoucher Photo: " + obj.Id.ToString(), "CMS >> EditPurchase");
                }

                return Ok(new { status = "success", data = new { PurchaseId = obj.Id } });
            }
            catch (ValidationException vex)
            {
                _repositoryWrapper.EventLog.Error("Edit purchase validation fail", vex.Message, "CMS >> EditPurchase");
                return BadRequest(new { status = "fail", message = vex.ValidationResult.ErrorMessage });
            }
            catch (Exception ex) 
            {
                _repositoryWrapper.EventLog.Error("Edit purchase fail", ex.Message, "CMS >> EditVoucher");
                return BadRequest(new { status = "fail", message = ex.Message });
            }
        }

        [HttpPost("makepayment")]
        public async Task<dynamic> MakePayment([FromBody] Newtonsoft.Json.Linq.JObject param)
        {
            try
            {
                dynamic objparam = param;
                string purchaseid = objparam.PurchaseId.ToString().Trim();
                string cardno = objparam.CardNumber.ToString().Trim();
                string cvv = objparam.CVV.ToString().Trim();
                string eDate = objparam.ExpiryDate.ToString().Trim();

                int userId = int.Parse(_tokenData.UserID);

                if (!int.TryParse(purchaseid, out int pid) || string.IsNullOrEmpty(purchaseid) || string.IsNullOrEmpty(cardno) || string.IsNullOrEmpty(cvv) || string.IsNullOrEmpty(eDate))
                {
                    return BadRequest(new { status = "fail", message = "Invalid Parameters" });
                }

                Purchase purobj = await _repositoryWrapper.Purchase.FindByIDAsync(pid);
                if (purobj == null)
                {
                    return BadRequest(new { status = "fail", message = "Purchase Not Found" });
                }

                if (userId != purobj.UserId)
                {
                    return BadRequest(new { status = "fail", message = "Purchase User Not Match" });
                }

                if (purobj.IsPaid)
                {
                    return BadRequest(new { status = "fail", message = "Already Purchased" });
                }

                bool check = Globalfunction.IsCreditCardInfoValid(cardno, eDate, cvv);
                if (!check)
                {
                    return BadRequest(new { status = "fail", message = "Invalid Payment Credentials" });
                }
                purobj.IsPaid = true;
                await _repositoryWrapper.Purchase.UpdateAsync(purobj, true);
                _repositoryWrapper.EventLog.Update(purobj, "Estore >> MakePayment");

                //Create eVouchers
                for(int i = 0; i < purobj.Quantity; i++)
                {
                    string promocode = Globalfunction.RandomAlphnumericString(6, 5);
                    while (_repositoryWrapper.Evoucher.CheckPromoExists(promocode))
                    {
                        promocode = Globalfunction.RandomAlphnumericString(6, 5);
                    }
                    var uid = purobj.GiftUserId == null ? purobj.UserId: purobj.GiftUserId;
                    Evoucher newobj = new Evoucher()
                    {
                        Title = purobj.Title,
                        Description = purobj.Description,
                        UserId = uid,
                        CreatedDate = DateTime.UtcNow,
                        ExpiryDate = DateTime.UtcNow.AddMonths(1),               
                        Amount = purobj.Amount,
                        PromoCode = promocode,
                        QrCode = promocode,
                        Isactive = true,
                        Isused = false,
                        PurchaseId = purobj.Id
                    };
                    await _repositoryWrapper.Evoucher.CreateAsync(newobj, true);
                    _repositoryWrapper.EventLog.Insert(newobj, "Evoucher >> MakePayment >> CreateEVoucher");
                }

                //update user purchase and gift limit
                User userobj = await _repositoryWrapper.User.FindByIDAsync(userId);
                if (purobj.GiftUserId != null)
                {
                    userobj.GiftCount += purobj.Quantity;
                }
                else
                {
                    userobj.BuyCount += purobj.Quantity;
                }
                await _repositoryWrapper.User.UpdateAsync(userobj, true);
                _repositoryWrapper.EventLog.Update(userobj, "Evoucher >> MakePayment >> UpdateUser");
                return Ok(new { status = "success", data = true });
            }
            catch (Exception ex) {
                _repositoryWrapper.EventLog.Error("MakePayment Failed", ex.Message, "Estore >> MakePayment");
                return BadRequest(new { status = "fail", message = "Something went wrong." });
            }
        }

        [HttpPut("editvoucher")]
        public async Task<dynamic> EditVoucher([FromBody] Newtonsoft.Json.Linq.JObject param)
        {
            try
            {
                dynamic objparam = param;
                string voucherid = objparam.VoucherId.ToString().Trim();
                string title = objparam.Title.ToString().Trim();
                string description = objparam.Description.ToString().Trim();
                string image = objparam.Image.ToString().Trim();
                bool isactive = objparam.Isactive;

                if (!int.TryParse(voucherid, out int pid) || string.IsNullOrEmpty(voucherid) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
                {
                    return BadRequest(new { status = "fail", message = "Invalid Parameters" });
                }
                int userId = int.Parse(_tokenData.UserID);
                var obj = await _repositoryWrapper.Evoucher.FindVoucherById(pid, userId);
                if (obj == null)
                {
                    return BadRequest(new { status = "fail", message = "Voucher Not Found." });
                }
                else if (obj.Isused == true)
                {
                    return BadRequest(new { status = "fail", message = "Used Voucher cannot be edited" });
                }
                
                obj.Title = title;
                obj.Description = description;
                obj.Isactive = isactive;

                await _repositoryWrapper.Evoucher.UpdateAsync(obj, true);
                _repositoryWrapper.EventLog.Update(obj, "CMS >> EditVoucher");

                if (!string.IsNullOrEmpty(image))
                {
                    FileService.DeleteFileNameOnly("eVoucherPhoto", obj.PurchaseId.ToString(), _configuration);
                    FileService.MoveTempFile("eVoucherPhoto", obj.PurchaseId.ToString(), image, _configuration);
                    _repositoryWrapper.EventLog.Info("Save evoucher Photo: " + obj.Id.ToString(), "CMS >> EditVoucher");
                }

                return Ok(new { status = "success", data = true });
            }
            catch (Exception ex) {
                _repositoryWrapper.EventLog.Error("get evoucher by id fail", ex.Message, "CMS >> EditVoucher");
                return BadRequest(new { status = "fail", message = "Something went wrong." });
            }
        }
    }
}
