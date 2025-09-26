using Microshop.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Microshop.Catalog.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property( p => p.Price ).HasColumnType("decimal(18,2)");
        });
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
}