namespace WebSaklaso.Models.Product
{
    public class ProductForCreatingDto
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Guid CategoryId { get; set; }
        public Guid SupplierId { get; set; }
    }
}
