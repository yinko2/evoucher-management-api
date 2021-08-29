using eVoucherAPI.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace eVoucherAPI.Repository
{
    public class UserRepository: RepositoryBase<User>, IUserRepository
    {
        public UserRepository(eVoucherContext repositoryContext)
            :base(repositoryContext)
        {
        }

        public async Task<int> GetUserIdByPhone(string phonenumber)
        {
            var result = await (from usr in RepositoryContext.Users
                            where usr.PhoneNumber == (string)phonenumber
                            select usr.Id).FirstOrDefaultAsync();
            return result;
        }

        public async Task<User> GetUserByPhone(string phonenumber)
        {
            var result = await (from usr in RepositoryContext.Users
                            where usr.PhoneNumber == (string)phonenumber
                            select usr).FirstOrDefaultAsync();
            return result;
        }

    }
}