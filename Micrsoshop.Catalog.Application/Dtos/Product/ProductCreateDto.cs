namespace Micrsoshop.Catalog.Application.Dtos.Product;

public class ProductCreateDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public string ImageUrl { get; set; }
}