using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using eVoucherAPI.Models;
using eVoucherAPI.Util;

namespace eVoucherAPI.Repository
{
    public class EventLogRepository: RepositoryBase<Eventlogs>, IEventLogRepository
    {
        public EventLogRepository(eVoucherContext repositoryContext)
            :base(repositoryContext)
        {
        }

        public async Task AddEventLog(EventLogTypes LogTypeEnum, string LogMessage, string ErrMessage, string SourceName)
        {
            EventLogTypes LogType = LogTypeEnum;
             
            string LoginUserID = "0";
            
            ClaimsIdentity objclaim = RepositoryContext._httpContextAccessor.HttpContext.User.Identities.Last();
            if(objclaim != null)
            {
                if(objclaim.FindFirst("UserID") != null) LoginUserID = objclaim.FindFirst("UserID").Value;
            } 
        
            if (LogMessage != "" || ErrMessage != "")
            {
                try
                {
                    var newobj = new Eventlogs();
                    newobj.LogType = LogType;
                    newobj.LogDateTime = DateTime.UtcNow;
                    newobj.LogMessage = LogMessage;
                    newobj.ErrorMessage = ErrMessage;
                    newobj.Source = SourceName;

                    if (LoginUserID != "0")
                    {
                        newobj.UserId = int.Parse(LoginUserID);
                    }

                    await CreateAsync(newobj);
                    await SaveAsync();

                }
                catch (Exception ex)
                {
                    Globalfunction.WriteSystemLog("SQL Exception :" + ex.Message);
                }
            }
        }
        
        public async Task Insert(dynamic obj, string Source)
        {
            string LogMessage = "";
            LogMessage += "Created :\r\n";
            LogMessage += this.SetOldObjectToString(obj);
            await AddEventLog(EventLogTypes.Insert, LogMessage, "", Source);
        }

        public async Task Update(dynamic obj, string Source)
        {
            string LogMessage = "";
            LogMessage += "Updated :\r\n";
            LogMessage += this.SetOldObjectToString(obj);
            await AddEventLog(EventLogTypes.Update, LogMessage, "", Source);
        }

        public async Task Delete(dynamic obj, string Source)
        {
            string LogMessage = "";
            LogMessage += "Deleted :\r\n";
            LogMessage += this.SetOldObjectToString(obj);
            await AddEventLog(EventLogTypes.Delete, LogMessage, "", Source);
        }

        public async Task Error(string LogMessage, string ErrMessage, string Source)
        {
            await AddEventLog(EventLogTypes.Error, LogMessage, ErrMessage, Source);
        }

        public async Task Info(string LogMessage, string Source)
        {
            await AddEventLog(EventLogTypes.Info, LogMessage, "", Source);
        }

        public async Task Warning(string LogMessage, string Source)
        {
            await AddEventLog(EventLogTypes.Warning, LogMessage, "", Source);
        }
    }
}