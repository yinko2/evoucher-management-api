using System.Collections.Generic;
using System.Threading.Tasks;
using eVoucherAPI.Models;

namespace eVoucherAPI.Repository
{
    public interface IEvoucherRepository: IRepositoryBase<Evoucher>
    {
        bool CheckPromoExists(string promo);
        Task<IEnumerable<Evoucher>> FindVoucherListByUserId(int id);
        Task<Evoucher> FindVoucherById(int id, int userid);
        Task<Evoucher> FindVoucherByPromoCode(string promo);
    }
}