using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace eVoucherAPI.Models
{
    [Table("tbl_evouchers")]
    [Index(nameof(UserId), Name = "user_id")]
    public partial class Evoucher
    {
        [Key]
        [Column("id", TypeName = "int(11)")]
        public int Id { get; set; }
        [Column("title")]
        [StringLength(50)]
        public string Title { get; set; }
        [Column("description")]
        [StringLength(100)]
        public string Description { get; set; }
        [Column("user_id", TypeName = "int(11)")]
        public int? UserId { get; set; }
        [Column("expiry_date", TypeName = "datetime")]
        public DateTime? ExpiryDate { get; set; }
        [Column("created_date", TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }
        [Column("amount")]
        public double? Amount { get; set; }
        [Column("promo_code")]
        [StringLength(20)]
        public string PromoCode { get; set; }
        [Column("qr_code")]
        [StringLength(100)]
        public string QrCode { get; set; }
        [Column("isactive")]
        public bool Isactive { get; set; }
        [Column("isused")]
        public bool Isused { get; set; }

        [Column("purchase_id")]
        public int? PurchaseId { get; set; }
    }
}
