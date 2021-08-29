using System.Threading.Tasks;
using eVoucherAPI.Models;

namespace eVoucherAPI.Repository
{
    public interface IPaymentRepository: IRepositoryBase<Payment>
    {
        Task<double> GetDiscountByPaymentId(int id);
    }
}