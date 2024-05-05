using cw7.DTOs;
using cw7.Models;
using FluentValidation;

namespace cw7.Validators;

public class WarehouseValidator : AbstractValidator<WarehouseDTO>
{
    public WarehouseValidator()
    {
        RuleFor(w => w.Name).NotEmpty().NotNull().MaximumLength(200);
        RuleFor(w => w.Address).NotEmpty().NotNull().MaximumLength(200);
    }
}