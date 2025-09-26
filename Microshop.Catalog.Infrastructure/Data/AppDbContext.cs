using Microshop.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Microshop.Catalog.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
}