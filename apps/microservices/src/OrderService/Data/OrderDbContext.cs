using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

/// <summary>
/// Database context for Order service
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Seed initial data
        modelBuilder.Entity<Order>().HasData(
            new Order 
            { 
                Id = 1, 
                ProductName = "Laptop", 
                Quantity = 2, 
                Price = 999.99m, 
                Status = "Completed",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Order 
            { 
                Id = 2, 
                ProductName = "Mouse", 
                Quantity = 10, 
                Price = 29.99m, 
                Status = "Processing",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Order 
            { 
                Id = 3, 
                ProductName = "Keyboard", 
                Quantity = 5, 
                Price = 79.99m, 
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Order 
            { 
                Id = 4, 
                ProductName = "Monitor", 
                Quantity = 3, 
                Price = 299.99m, 
                Status = "Completed",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Order 
            { 
                Id = 5, 
                ProductName = "Headphones", 
                Quantity = 15, 
                Price = 49.99m, 
                Status = "Processing",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            }
        );
    }
}

