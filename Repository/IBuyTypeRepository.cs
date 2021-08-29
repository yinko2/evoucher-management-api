using System.Threading.Tasks;
using eVoucherAPI.Models;

namespace eVoucherAPI.Repository
{
    public interface IBuyTypeRepository: IRepositoryBase<Buytype>
    {
        Task<dynamic> GetBuyTypeInfoList();
    }
}