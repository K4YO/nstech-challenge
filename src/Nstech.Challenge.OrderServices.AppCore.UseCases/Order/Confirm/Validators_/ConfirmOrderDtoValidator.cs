using FluentValidation;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Dtos_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Validators_;

public class ConfirmOrderDtoValidator : AbstractValidator<ConfirmOrderDto>
{
    public ConfirmOrderDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId cannot be empty");
    }
}
