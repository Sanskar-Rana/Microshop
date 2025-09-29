using System.ComponentModel.DataAnnotations;

namespace Microshop.Catalog.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
   
    public string? Description { get; set; }
    public decimal Price { get; set; }
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
    

    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}