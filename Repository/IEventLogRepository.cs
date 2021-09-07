using System.Threading.Tasks;
using eVoucherAPI.Models;

namespace eVoucherAPI.Repository
{
    public interface IEventLogRepository: IRepositoryBase<Eventlogs>
    {
        Task AddEventLog(EventLogTypes LogTypeEnum, string LogMessage, string ErrMessage, string SourceName="");
        Task Insert(dynamic obj, string Source);
        Task Update(dynamic obj, string Source);
        Task Delete(dynamic obj, string Source);
        Task Info(string LogMessage, string Source);
        Task Error(string LogMessage, string ErrMessage, string Source);
        Task Warning(string LogMessage, string Source);
    }
}