using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Auth.GenereteToken.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Auth.GenereteToken
{
    internal class GenereteTokenUseCase : UseCase<CreateTokenDto, TokenDto>
    {
        private readonly IConfiguration _configuration;
        public GenereteTokenUseCase(
            IConfiguration configuration,
            IValidator<CreateTokenDto> validator,
            ILogger<GenereteTokenUseCase> logger) : base(logger, validator)
        {
            _configuration = configuration;
        }
        protected override async Task<UseCaseResult<TokenDto>> ExecuteValidatedAsync(CreateTokenDto input, CancellationToken cancellationToken)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var expirationMinutes = int.TryParse(jwtSettings["ExpirationMinutes"], out var minutes) ? minutes : 60;

            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, input.Username),
                    new Claim("sub", input.Username)
                ]),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            Logger.LogInformation("Token generated for user {Username}, expires in {ExpirationMinutes} minutes",
                input.Username, expirationMinutes);

            var tokenDto = new TokenDto(tokenString, expirationMinutes * 60, "Bearer");

            return UseCaseResult<TokenDto>.Success(tokenDto);
        }
    }
}
