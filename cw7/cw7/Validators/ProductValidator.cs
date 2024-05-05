using cw7.DTOs;
using FluentValidation;

namespace cw7.Validators;

public class ProductValidator : AbstractValidator<ProductDTO>
{
    public ProductValidator()
    {
        RuleFor(p => p.Description).NotEmpty().NotNull().MaximumLength(200);
        RuleFor(p => p.Name).NotEmpty().NotNull().MaximumLength(200);
        RuleFor(p => p.Price).NotNull();
    }
}