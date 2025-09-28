using FluentValidation;
using Micrsoshop.Catalog.Application.Dtos.Category;

namespace Microshop.Catalog.Domain.Validators;


public class CategoryCreateDtoValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}