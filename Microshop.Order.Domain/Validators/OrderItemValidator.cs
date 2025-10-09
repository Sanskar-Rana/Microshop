using FluentValidation;
using Microshop.Order.Domain.Entities;

namespace Microshop.Order.Domain.Validators;

public class OrderItemValidator : AbstractValidator<OrderItem>
{
    public OrderItemValidator()
    {
       RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product Id is required.");
       RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity is required.");
      
    }
}