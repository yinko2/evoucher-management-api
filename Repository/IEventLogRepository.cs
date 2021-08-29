using eVoucherAPI.Models;

namespace eVoucherAPI.Repository
{
    public interface IEventLogRepository: IRepositoryBase<Eventlogs>
    {
        void AddEventLog(EventLogTypes LogTypeEnum, string LogMessage, string ErrMessage, string SourceName="");
        void Insert(dynamic obj, string Source);
        void Update(dynamic obj, string Source);
        void Delete(dynamic obj, string Source);
        void Info(string LogMessage, string Source);
        void Error(string LogMessage, string ErrMessage, string Source);
        void Warning(string LogMessage, string Source);
    }
}