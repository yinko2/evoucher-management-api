using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace eVoucherAPI.Models
{
    [Table("tbl_payment_types")]
    public partial class Payment
    {
        [Key]
        [Column("id", TypeName = "int(11)")]
        public int Id { get; set; }
        [Column("payment_name")]
        [StringLength(20)]
        public string PaymentName { get; set; }
        [Column("discount")]
        public double Discount { get; set; }
    }
}
