using System;
using System.Linq;
using System.Security.Claims;
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

        public void AddEventLog(EventLogTypes LogTypeEnum, string LogMessage, string ErrMessage, string SourceName="")
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
                    newobj.UserId = int.Parse(LoginUserID);

                    Create(newobj);
                    Save();

                }
                catch (Exception ex)
                {
                    Globalfunction.WriteSystemLog("SQL Exception :" + ex.Message);
                }
            }
        }
        
        public void Insert(dynamic obj, string Source)
        {
            string LogMessage = "";
            LogMessage += "Created :\r\n";
            LogMessage += this.SetOldObjectToString(obj);
            AddEventLog(EventLogTypes.Insert, LogMessage, "", Source);
        }

        public void Update(dynamic obj, string Source)
        {
            string LogMessage = "";
            LogMessage += "Updated :\r\n";
            LogMessage += this.SetOldObjectToString(obj);
            AddEventLog(EventLogTypes.Update, LogMessage, "", Source);
        }

        public void Delete(dynamic obj, string Source)
        {
            string LogMessage = "";
            LogMessage += "Deleted :\r\n";
            LogMessage += this.SetOldObjectToString(obj);
            AddEventLog(EventLogTypes.Delete, LogMessage, "", Source);
        }

        public void Error(string LogMessage, string ErrMessage, string Source)
        {
            AddEventLog(EventLogTypes.Error, LogMessage, ErrMessage, Source);
        }

        public void Info(string LogMessage, string Source)
        {
            AddEventLog(EventLogTypes.Info, LogMessage, "", Source);
        }

        public void Warning(string LogMessage, string Source)
        {
            AddEventLog(EventLogTypes.Warning, LogMessage, "", Source);
        }
    }
}