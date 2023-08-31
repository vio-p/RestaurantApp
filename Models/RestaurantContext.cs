using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RestaurantApp.Models;

public class RestaurantContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["DbRestaurantConnectionString"].ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .UseTpcMappingStrategy();

        modelBuilder.Entity<User>()
            .Ignore(u => u.FullName);

        modelBuilder.Entity<Administrator>()
            .ToTable("Administrators");

        modelBuilder.Entity<Waiter>()
            .ToTable("Waiters");

        modelBuilder.Entity<Order>()
            .Property(o => o.State)
            .HasConversion<int>();
        modelBuilder.Entity<Order>()
            .Property(o => o.Total)
            .HasPrecision(19, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(19, 2);
        modelBuilder.Entity<Product>()
            .Ignore(p => p.DisplayName);

        modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new { op.OrderId, op.ProductId });
        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Order)
            .WithMany(o => o.OrderProducts)
            .HasForeignKey(op => op.OrderId);
        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Product)
            .WithMany(p => p.OrderProducts)
            .HasForeignKey(op => op.ProductId);
        modelBuilder.Entity<OrderProduct>()
            .Property(op => op.ProductPrice)
            .HasPrecision(19, 2);
        modelBuilder.Entity<OrderProduct>()
            .Ignore(op => op.TotalPrice);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
}
