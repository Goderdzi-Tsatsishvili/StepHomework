using Microsoft.EntityFrameworkCore;
using WebSaklaso.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WebSaklaso.Models.Auth;

namespace WebSaklaso.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {  
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.DataSeeder();
            modelBuilder.NormalizeIdentityTableNames();
            modelBuilder.EnsureRefreshTokenIsUnique();
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<AppUser> ApplicationUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
