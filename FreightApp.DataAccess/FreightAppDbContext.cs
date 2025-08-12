using Microsoft.EntityFrameworkCore;
using FreightApp.Domain.Models;

namespace FreightApp.DataAccess
{
    public class FreightAppDbContext : DbContext
    {
        public FreightAppDbContext(DbContextOptions<FreightAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Userlist { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<OrderList> OrderList { get; set; }
        public DbSet<OrderHead> OrderHead { get; set; }
        public DbSet<OrderStatus> Status_Order { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Userlist");
            modelBuilder.Entity<UserRole>().ToTable("UserRole");
            modelBuilder.Entity<OrderList>().ToTable("OrderList");
            modelBuilder.Entity<OrderHead>().ToTable("OrderHead");
            modelBuilder.Entity<OrderStatus>().ToTable("Status_Order");
        }
    }
}
