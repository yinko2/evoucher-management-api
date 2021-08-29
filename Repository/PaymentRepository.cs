using System.Threading.Tasks;
using eVoucherAPI.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace eVoucherAPI.Repository
{
    public class PaymentRepository: RepositoryBase<Payment>, IPaymentRepository
    {
        public PaymentRepository(eVoucherContext repositoryContext)
            :base(repositoryContext)
        {
        }

        public async Task<double> GetDiscountByPaymentId(int id)
        {
            var result = await (from main in RepositoryContext.Payments
                            where main.Id.Equals(id)
                            select main.Discount).FirstOrDefaultAsync();
            return result;
        }

    }
}