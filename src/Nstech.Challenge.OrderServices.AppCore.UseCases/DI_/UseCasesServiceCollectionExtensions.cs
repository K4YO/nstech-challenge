using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Auth.GenereteToken;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Auth.GenereteToken.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Auth.GenereteToken.Validators_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Validators_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Validators_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Validators_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Validators_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Validators_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.DI_;

/// <summary>
/// Extension methods for registering use cases and validators in the dependency injection container.
/// </summary>
public static class UseCasesServiceCollectionExtensions
{
    /// <summary>
    /// Adds all use cases and their validators to the service collection.
    /// </summary>
    /// <param name="services">The service collection to extend</param>
    /// <returns>The updated service collection</returns>
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        // Register Order Use Cases
        services.AddScoped<IUseCase<CreateOrderDto, CreateOrderResultDto>, CreateOrderUseCase>();
        services.AddScoped<IUseCase<ConfirmOrderDto, ConfirmOrderResultDto>, ConfirmOrderUseCase>();
        services.AddScoped<IUseCase<CancelOrderDto, CancelOrderResultDto>, CancelOrderUseCase>();
        services.AddScoped<IUseCase<GetOrderDto, OrderDto>, GetOrderUseCase>();
        services.AddScoped<IUseCase<GetOrdersDto, GetOrdersResultDto>, GetOrdersUseCase>();
        services.AddScoped<IUseCase<CreateTokenDto, TokenDto>, GenereteTokenUseCase>();

        // Register Validators
        services.AddScoped<IValidator<CreateOrderDto>, CreateOrderDtoValidator>();
        services.AddScoped<IValidator<ConfirmOrderDto>, ConfirmOrderDtoValidator>();
        services.AddScoped<IValidator<CancelOrderDto>, CancelOrderDtoValidator>();
        services.AddScoped<IValidator<GetOrderDto>, GetOrderDtoValidator>();
        services.AddScoped<IValidator<GetOrdersDto>, GetOrdersDtoValidator>();
        services.AddScoped<IValidator<CreateTokenDto>, CreateTokenDtoValidator>();

        return services;
    }
}

