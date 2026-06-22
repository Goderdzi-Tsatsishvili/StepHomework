using Microsoft.EntityFrameworkCore;
using WebSaklaso.Data;
using WebSaklaso.Entities;
using WebSaklaso.Repositories.Contracts;
using WebSaklaso.Repositories.GenericRepo;

namespace WebSaklaso.Repositories
{
    public class CustomerRepository : GenericRepo<Customer, AppDbContext>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext db) : base(db)
        {
        }
    }
}
