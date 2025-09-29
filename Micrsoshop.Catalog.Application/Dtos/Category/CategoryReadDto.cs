using Micrsoshop.Catalog.Application.Dtos.Product;

namespace Micrsoshop.Catalog.Application.Dtos.Category;

public class CategoryReadDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<ProductReadDto> Products { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}