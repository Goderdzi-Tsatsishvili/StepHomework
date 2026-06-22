using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSaklaso.Entities
{
    public class Supplier
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string SupplierName { get; set; }

        //Navigation
        public ICollection<Product> Products { get; set; }
    }
}
