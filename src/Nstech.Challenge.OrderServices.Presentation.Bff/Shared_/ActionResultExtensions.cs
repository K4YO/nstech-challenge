using Microsoft.AspNetCore.Mvc;
using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;

namespace Nstech.Challenge.OrderServices.Http.Bff.Shared_;

public static class ActionResultExtensions
{
    /// <summary>
    /// Maps a UseCaseResult to an ASP.NET Core IActionResult.
    /// For Created results, a factory can be provided to produce a CreatedAtAction result.
    /// </summary>
    public static IActionResult UseCaseToActionResult<T>(this ControllerBase controller, UseCaseResult<T> result)
        where T : Dto
    {
        return result.Type switch
        {
            UseCaseResultType.Created => result.Data is null
                ? controller.StatusCode(StatusCodes.Status201Created)        
                : controller.StatusCode(StatusCodes.Status201Created, result.Data),
            UseCaseResultType.Success => controller.Ok(result.Data),
            UseCaseResultType.NoContent => controller.NoContent(),
            UseCaseResultType.Accepted => controller.Accepted(result.Data),
            UseCaseResultType.Unprocessable => controller.UnprocessableEntity(new ValueFailureDetail(result.Failures)),
            UseCaseResultType.NotFound => controller.NotFound(new ValueFailureDetail(result.Failures)),
            UseCaseResultType.Failure => controller.BadRequest(new ValueFailureDetail(result.Failures)),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError, new ValueFailureDetail(new List<FailureDetail> { new("Unexpected result type") }))
        };
    }
}
