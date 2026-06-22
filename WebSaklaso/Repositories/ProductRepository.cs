using Microsoft.EntityFrameworkCore;
using WebSaklaso.Data;
using WebSaklaso.Entities;
using WebSaklaso.Repositories.Contracts;
using WebSaklaso.Repositories.GenericRepo;

namespace WebSaklaso.Repositories
{
    public class ProductRepository : GenericRepo<Product, AppDbContext>, IProductRepository
    {
        public ProductRepository(AppDbContext db) : base(db)
        {
            
        }
    }
}
