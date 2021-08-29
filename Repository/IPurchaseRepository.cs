using System.Threading.Tasks;
using eVoucherAPI.Models;

namespace eVoucherAPI.Repository
{
    public interface IPurchaseRepository: IRepositoryBase<Purchase>
    {
        Task<dynamic> GetPurchaseHistoryByUserId(int userid);
        Task<dynamic> GetVoucherListByPurchaseId(int id, int userid);
    }
}