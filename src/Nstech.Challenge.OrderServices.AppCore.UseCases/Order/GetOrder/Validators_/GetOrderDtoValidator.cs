using FluentValidation;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Dtos_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Validators_;

public class GetOrderDtoValidator : AbstractValidator<GetOrderDto>
{
    public GetOrderDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId cannot be empty");
    }
}
