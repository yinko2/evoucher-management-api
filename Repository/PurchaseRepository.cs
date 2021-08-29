using System.Collections.Generic;
using eVoucherAPI.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace eVoucherAPI.Repository
{
    public class PurchaseRepository: RepositoryBase<Purchase>, IPurchaseRepository
    {
        public PurchaseRepository(eVoucherContext repositoryContext)
            :base(repositoryContext)
        {
        }

        public async Task<dynamic> GetPurchaseHistoryByUserId(int userid)
        {
            var qry = ( from main in RepositoryContext.Purchases.Where(m  => m.UserId.Equals(userid))
                        join bt  in RepositoryContext.BuyTypes on main.BuyTypeId equals bt.Id
                        join pm in RepositoryContext.Payments on main.PaymentId equals pm.Id
                        join user in RepositoryContext.Users on main.GiftUserId equals user.Id into tmp
                        from a in tmp.DefaultIfEmpty()
                        select new { PurchaseId = main.Id, main.Title, main.Description, main.PurchasedDate, BuyType = bt.Name, GiftTo = a.PhoneNumber, main.Quantity, Payment = pm.PaymentName, main.Amount, main.Discount, main.Cost, main.IsPaid});
            return await qry.OrderByDescending(q => q.PurchasedDate).ToListAsync();
        }

        public async Task<dynamic> GetVoucherListByPurchaseId(int id, int userid)
        {
            var qry = from main in RepositoryContext.Evouchers.Where(m  => m.PurchaseId.Equals(id) && m.Isactive == true)
                        join p in RepositoryContext.Purchases.Where(a => a.UserId.Equals(userid)) on main.PurchaseId equals p.Id 
                        join u in RepositoryContext.Users on main.UserId equals u.Id
                        select new { VoucherId = main.Id, main.Title, main.Description, EligiblePhone = u.PhoneNumber, main.CreatedDate, main.ExpiryDate, main.Amount, main.PromoCode, main.QrCode, main.Isused };
            return await qry.ToListAsync();
        }
    }
}