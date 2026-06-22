using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSaklaso.Entities
{
    public class OrderItem
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }

        [Required]
        [ForeignKey(nameof(Order))]
        public Guid OrderId { get; set; }

        [Required]
        [ForeignKey(nameof(Product))]
        public Guid ProductId { get; set; }

        //Navigation
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
