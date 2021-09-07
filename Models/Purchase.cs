using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace eVoucherAPI.Models
{
    [Table("tbl_purchases")]
    public partial class Purchase
    {
        [Key]
        [Column("id", TypeName = "int(11)")]
        public int Id { get; set; }

        [Column("title")]
        [StringLength(50)]
        [Required]
        public string Title { get; set; }

        [Column("description")]
        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        [Column("purchased_date", TypeName = "datetime")]
        public DateTime PurchasedDate { get; set; }

        [Column("buy_type_id", TypeName = "int(11)")]
        [Required]
        public int? BuyTypeId { get; set; }

        [Column("user_id", TypeName = "int(11)")]
        public int? UserId { get; set; }

        [Column("gift_user_id", TypeName = "int(11)")]
        public int? GiftUserId { get; set; }

        [Column("quantity", TypeName = "int(11)")]
        [Required]
        public int Quantity { get; set; }

        [Column("payment_id", TypeName = "int(11)")]
        [Required]
        public int PaymentId { get; set; }

        [Column("amount")]
        [Required]
        public double Amount { get; set; }

        [Column("discount")]
        public double? Discount { get; set; }

        [Column("cost")]
        public double? Cost { get; set; }

        [Column("is_paid")]
        public bool IsPaid { get; set; }

        [NotMapped]
        public string GiftUserPhone { get; set;}

        [NotMapped]
        public string Image { get; set;}
    }
}
