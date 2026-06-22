using Microsoft.EntityFrameworkCore;
using WebSaklaso.Data;
using WebSaklaso.Entities;
using WebSaklaso.Repositories.Contracts;
using WebSaklaso.Repositories.GenericRepo;

namespace WebSaklaso.Repositories
{
    public class SupplierRepository : GenericRepo<Supplier, AppDbContext>, ISupplierRepository
    {
        public SupplierRepository(AppDbContext db) : base(db)
        {
            
        }
    }
}
