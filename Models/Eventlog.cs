using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace eVoucherAPI.Models
{
    [Table("tbl_eventlog")]
    public partial class Eventlogs
    {
        [Key]
        [Column("id", TypeName = "int(11)")]
        public int Id { get; set; }
        [Column("log_type", TypeName = "int(11)")]
        public EventLogTypes LogType { get; set; }
        [Column("log_date_time", TypeName = "datetime")]
        public DateTime? LogDateTime { get; set; }
        [Column("source")]
        [StringLength(50)]
        public string Source { get; set; }
        [Column("log_message", TypeName = "text")]
        public string LogMessage { get; set; }
        [Column("error_message", TypeName = "text")]
        public string ErrorMessage { get; set; }
        [Column("user_id", TypeName = "int(11)")]
        public int? UserId { get; set; }
    }

    public enum EventLogTypes
    {
        Info = 1,
        Error = 2,
        Warning = 3,
        Insert = 4,
        Update = 5,
        Delete = 6
    }
}
