using FluentValidation;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Dtos_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Validators_;

public class GetOrdersDtoValidator : AbstractValidator<GetOrdersDto>
{
    public GetOrdersDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100");

        RuleFor(x => x.Status)
            .Must(status => status == null || IsValidStatus(status))
            .WithMessage("Status must be one of: Draft, Placed, Confirmed, Canceled");

        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
            .WithMessage("FromDate must be less than or equal to ToDate");
    }

    private static bool IsValidStatus(string status)
    {
        return status switch
        {
            "Draft" or "Placed" or "Confirmed" or "Canceled" => true,
            _ => false
        };
    }
}
