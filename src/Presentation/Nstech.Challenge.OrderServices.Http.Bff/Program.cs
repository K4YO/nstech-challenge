using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
using Nstech.Challenge.OrderServices.Infrastructure.DI_;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var key = Encoding.ASCII.GetBytes(secretKey);

// Add services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Services API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});

// Add Infrastructure
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddInfrastructure(connectionString);

// Add Use Cases
builder.Services.AddScoped<CreateOrderUseCase>();
builder.Services.AddScoped<ConfirmOrderUseCase>();
builder.Services.AddScoped<CancelOrderUseCase>();
builder.Services.AddScoped<GetOrderUseCase>();
builder.Services.AddScoped<GetOrdersUseCase>();

// Add Validators
builder.Services.AddScoped<IValidator<CreateOrderDto>, CreateOrderDtoValidator>();
builder.Services.AddScoped<IValidator<ConfirmOrderDto>, ConfirmOrderDtoValidator>();
builder.Services.AddScoped<IValidator<CancelOrderDto>, CancelOrderDtoValidator>();
builder.Services.AddScoped<IValidator<GetOrderDto>, GetOrderDtoValidator>();
builder.Services.AddScoped<IValidator<GetOrdersDto>, GetOrdersDtoValidator>();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Services API v1");
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

