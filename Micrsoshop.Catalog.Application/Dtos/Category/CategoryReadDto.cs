namespace Micrsoshop.Catalog.Application.Dtos.Category;

public class CategoryReadDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<Microshop.Catalog.Domain.Entities.Product> Products { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}