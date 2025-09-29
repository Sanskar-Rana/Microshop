using FluentValidation;
using Microshop.Catalog.Domain.Entities;

namespace Microshop.Catalog.Domain.Validators;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(p=>p.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(p => p.Price).NotEmpty().WithMessage("Price is required.");
        RuleFor(p=>p.CategoryId).NotEmpty().WithMessage("Category is required.");
        RuleFor(p=> p.ImageUrl).NotEmpty().WithMessage("Image is required.");
        RuleFor(p => p.CreatedAt).NotEmpty();
        RuleFor(p => p.UpdatedAt).NotEmpty();
    }

}