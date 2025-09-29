namespace Microshop.Catalog.Domain.Entities;

public class Category
{

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; 
}