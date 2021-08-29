using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;


namespace eVoucherAPI.Repository
{
    public interface IRepositoryBase<T>
    {
        EntityEntry EntryObject(T objT);
        IEnumerable<T> FindAll();
        Task<IEnumerable<T>> FindAllAsync();
        T FindByCompositeID(long ID1, long ID2);
        T FindByCompositeID(int ID1, int ID2);
        Task<T> FindByCompositeIDAsync(long ID1, long ID2);
        IEnumerable<T> FindByCondition(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> expression);
        bool AnyByCondition(Expression<Func<T, bool>> expression);
        Task<bool> AnyByConditionAsync(Expression<Func<T, bool>> expression);
        T FindByID(long ID);
        T FindByID(int ID);
        Task<T> FindByIDAsync(int ID);
        void Create(dynamic entity, bool flush = true);
        void Update(dynamic entity, bool flush = true);
        void Delete(dynamic entity, bool flush = true);
        void Save();
        Task CreateAsync(dynamic entity, bool flush = true);
        Task UpdateAsync(dynamic entity, bool flush = true);
        Task DeleteAsync(dynamic entity, bool flush = true);

        Task<bool> IsExist(int ID);
        Task SaveAsync();
    }
}
