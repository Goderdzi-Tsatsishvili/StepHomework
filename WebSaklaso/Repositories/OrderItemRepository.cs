using Microsoft.EntityFrameworkCore;
using WebSaklaso.Data;
using WebSaklaso.Entities;
using WebSaklaso.Repositories.Contracts;
using WebSaklaso.Repositories.GenericRepo;

namespace WebSaklaso.Repositories
{
    public class OrderItemRepository : GenericRepo<OrderItem, AppDbContext>, IOrderItemRepository
    {
        public OrderItemRepository(AppDbContext db) : base(db)
        {
            
        }
    }
}
