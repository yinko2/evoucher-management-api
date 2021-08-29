using System.Threading.Tasks;
using eVoucherAPI.Models;

namespace eVoucherAPI.Repository
{
    public interface IUserRepository: IRepositoryBase<User>
    {
        Task<int> GetUserIdByPhone(string phonenumber);
        Task<User> GetUserByPhone(string phonenumber);
    }
}