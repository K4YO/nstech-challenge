using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Dtos_;

namespace Nstech.Challenge.OrderServices.Http.Bff.OrderServices.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly CreateOrderUseCase _createOrderUseCase;
    private readonly ConfirmOrderUseCase _confirmOrderUseCase;
    private readonly CancelOrderUseCase _cancelOrderUseCase;
    private readonly GetOrderUseCase _getOrderUseCase;
    private readonly GetOrdersUseCase _getOrdersUseCase;

    public OrderController(
        CreateOrderUseCase createOrderUseCase,
        ConfirmOrderUseCase confirmOrderUseCase,
        CancelOrderUseCase cancelOrderUseCase,
        GetOrderUseCase getOrderUseCase,
        GetOrdersUseCase getOrdersUseCase)
    {
        _createOrderUseCase = createOrderUseCase;
        _confirmOrderUseCase = confirmOrderUseCase;
        _cancelOrderUseCase = cancelOrderUseCase;
        _getOrderUseCase = getOrderUseCase;
        _getOrdersUseCase = getOrdersUseCase;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="dto">Order creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _createOrderUseCase.ExecuteAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ErrorResponse { Message = result.Message, Errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetOrder), new { id = result.Data }, new OrderCreatedResponse(result.Data!));
    }

    /// <summary>
    /// Confirm an order (idempotent)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var dto = new ConfirmOrderDto(id);
        var result = await _confirmOrderUseCase.ExecuteAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ErrorResponse { Message = result.Message, Errors = result.Errors });
        }

        return Ok(new SuccessResponse { Message = result.Message });
    }

    /// <summary>
    /// Cancel an order (idempotent)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var dto = new CancelOrderDto(id);
        var result = await _cancelOrderUseCase.ExecuteAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ErrorResponse { Message = result.Message, Errors = result.Errors });
        }

        return Ok(new SuccessResponse { Message = result.Message });
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var dto = new GetOrderDto(id);
        var result = await _getOrderUseCase.ExecuteAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ErrorResponse { Message = result.Message, Errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// List orders with pagination and filtering
    /// </summary>
    /// <param name="customerId">Filter by customer ID (optional)</param>
    /// <param name="status">Filter by status (optional)</param>
    /// <param name="fromDate">Filter by creation date from (optional)</param>
    /// <param name="toDate">Filter by creation date to (optional)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetOrdersResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] Guid? customerId,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var dto = new GetOrdersDto(customerId, status, fromDate, toDate, page, pageSize);
        var result = await _getOrdersUseCase.ExecuteAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ErrorResponse { Message = result.Message, Errors = result.Errors });
        }

        return Ok(result.Data);
    }
}

