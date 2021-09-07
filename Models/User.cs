using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace eVoucherAPI.Models
{
    [Table("tbl_users")]
    public partial class User
    {
        [Key]
        [Column("id", TypeName = "int(11)")]
        public int Id { get; set; }

        [Column("phone_number")]
        [StringLength(20)]
        [Required]
        public string PhoneNumber { get; set; }

        [Column("name")]
        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [Column("password")]
        [StringLength(255)]
        [Required]
        public string Password { get; set; }

        [Column("passwordsalt")]
        [Required]
        [StringLength(255)]
        public string Passwordsalt { get; set; }

        [Column("buy_count", TypeName = "int(11)")]
        public int? BuyCount { get; set; }
        
        [Column("gift_count", TypeName = "int(11)")]
        public int? GiftCount { get; set; }
    }
}
