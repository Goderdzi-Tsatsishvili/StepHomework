using WebSaklaso.Models.Category;

namespace WebSaklaso.Models.Product
{
    public class ProductForGettingDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public CategoryForGettingDto Category { get; set; }
    }
}
