using System.ComponentModel.DataAnnotations;

namespace Micrsoshop.Catalog.Application.Dtos.Category;

public class CategoryCreateDto
{
    [Required]
    public string? Name { get; set; }
}