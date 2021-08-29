using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace eVoucherAPI.Models
{
    [Table("tbl_buy_types")]
    public partial class Buytype
    {
        [Key]
        [Column("id", TypeName = "int(11)")]
        public int Id { get; set; }
        
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }

    }
}
