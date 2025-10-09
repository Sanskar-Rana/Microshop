using Microshop.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Microshop.Order.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Domain.Entities.Order>(entity =>
        {
            entity.Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(p => p.ProductPrice).HasColumnType("decimal(18,2)");
            entity.Property(p => p.TotalPrice).HasColumnType("decimal(18,2)");
        });
    }

    public DbSet<Domain.Entities.Order>  Orders { get; set; }
    public DbSet<OrderItem>  OrderItems { get; set; }
    
}