using Microsoft.AspNetCore.Mvc;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Auth.GenereteToken.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;
using Nstech.Challenge.OrderServices.Http.Bff.Shared_;


namespace Nstech.Challenge.OrderServices.Http.Bff.Auth.V1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IUseCase<CreateTokenDto, TokenDto> _createTokenUseCase;

    public AuthController(IConfiguration configuration, IUseCase<CreateTokenDto, TokenDto> createTokenUseCase)
    {
        _createTokenUseCase = createTokenUseCase;
    }

    /// <summary>
    /// Generate JWT token for authentication
    /// </summary>
    /// <param name="request">Token request</param>
    /// <returns>JWT token</returns>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValueFailureDetail), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateToken([FromBody] CreateTokenDto request, CancellationToken cancellationToken)
    {
        var result = await _createTokenUseCase.ExecuteAsync(request, cancellationToken);
        return this.UseCaseToActionResult(result);
    }
}

