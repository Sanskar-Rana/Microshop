using FluentValidation;

namespace Microshop.Order.Domain.Validators;

public class OrderValidator : AbstractValidator<Entities.Order>
{
    public OrderValidator()
    {
        RuleFor(order => order.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .NotNull().WithMessage("User ID cannot be null");

        RuleFor(order => order.UserEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(order => order.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than 0");

        RuleFor(order => order.Status)
            .IsInEnum().WithMessage("Invalid order status");

        RuleFor(order => order.Items)
            .NotNull().WithMessage("Order items cannot be null")
            .Must(items => items != null && items.Any()).WithMessage("At least one order item is required");

        RuleFor(order => order.CreatedAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Created date cannot be in the future");

        RuleFor(order => order.UpdatedAt)
            .GreaterThanOrEqualTo(order => order.CreatedAt)
            .When(order => order.UpdatedAt != default && order.CreatedAt != default)
            .WithMessage("Updated date cannot be before created date");
    }
}