using FluentValidation;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Dtos_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Validators_;

public class CancelOrderDtoValidator : AbstractValidator<CancelOrderDto>
{
    public CancelOrderDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId cannot be empty");
    }
}

