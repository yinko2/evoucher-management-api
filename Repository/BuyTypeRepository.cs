using System.Collections.Generic;
using System.Threading.Tasks;
using eVoucherAPI.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace eVoucherAPI.Repository
{
    public class BuyTypeRepository: RepositoryBase<Buytype>, IBuyTypeRepository
    {
        public BuyTypeRepository(eVoucherContext repositoryContext)
            :base(repositoryContext)
        {
        }

        public async Task<dynamic> GetBuyTypeInfoList()
        {
            var qry = ( from main in RepositoryContext.BuyTypes
                        select new {
                            main.Id,
                            main.Name
                        });
            return await qry.ToListAsync();
        }

    }
}