using cw7.DTOs;
using FluentValidation;

namespace cw7.Validators;

public class RequestValidator : AbstractValidator<RequestDTO>
{
    public RequestValidator()
    {
        RuleFor(r => r.IdProduct).NotNull();
        RuleFor(r => r.IdWarehouse).NotNull();
        RuleFor(r => r.Amount).NotNull().Must(a => a > 0).WithMessage("amount should be more then zero");
        RuleFor(r => r.CreatedAt).NotNull().Must(d => d.Date <= DateTime.Now);
        
    }
}