using Microsoft.EntityFrameworkCore;
using WebSaklaso.Data;
using WebSaklaso.Entities;
using WebSaklaso.Repositories.Contracts;
using WebSaklaso.Repositories.GenericRepo;

namespace WebSaklaso.Repositories
{
    public class CategoryRepository : GenericRepo<Category, AppDbContext>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext db) : base(db)
        {
        }
    }
}
