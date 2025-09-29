using FluentValidation;
using Microshop.Catalog.Domain.Entities;

namespace Microshop.Catalog.Domain.Validators;

public class CategoryValidator : AbstractValidator<Category>
{

    public CategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }


}