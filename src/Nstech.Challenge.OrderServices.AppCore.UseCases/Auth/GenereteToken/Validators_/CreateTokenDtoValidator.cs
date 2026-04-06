using FluentValidation;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Auth.GenereteToken.Dtos_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Auth.GenereteToken.Validators_;

public class CreateTokenDtoValidator : AbstractValidator<CreateTokenDto>
{
    public CreateTokenDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
