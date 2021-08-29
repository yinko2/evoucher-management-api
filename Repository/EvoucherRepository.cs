using System.Collections.Generic;
using eVoucherAPI.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace eVoucherAPI.Repository
{
    public class EvoucherRepository: RepositoryBase<Evoucher>, IEvoucherRepository
    {
        public EvoucherRepository(eVoucherContext repositoryContext)
            :base(repositoryContext)
        {
        }

        public bool CheckPromoExists(string promo)
        {
            var qry = ( from main in RepositoryContext.Evouchers
                        where main.PromoCode.Equals(promo)
                        select main.Id).Count();
            if (qry == 0)
                return false;
            return true;
        }

        public async Task<IEnumerable<Evoucher>> FindVoucherListByUserId(int id)
        {
            return await RepositoryContext.Evouchers
                        .Where(e => e.UserId.Equals(id))
                        .OrderBy(s => s.Id).ToListAsync();
        }

        public async Task<Evoucher> FindVoucherById(int id, int userid)
        {
            return await RepositoryContext.Evouchers
                        .Where(e => e.UserId.Equals(userid) && e.Id.Equals(id))
                        .FirstOrDefaultAsync();
        }

        public async Task<Evoucher> FindVoucherByPromoCode(string promo)
        {
            return await RepositoryContext.Evouchers
                        .Where(e => e.PromoCode.Equals(promo))
                        .FirstOrDefaultAsync();
        }

    }
}