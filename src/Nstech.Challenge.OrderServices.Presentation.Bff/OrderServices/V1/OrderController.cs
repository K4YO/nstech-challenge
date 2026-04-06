using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Cancel.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Confirm.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.Create.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrder.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Order.GetOrders.Dtos_;
using Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_;
using Nstech.Challenge.OrderServices.Http.Bff.Shared_;

namespace Nstech.Challenge.OrderServices.Http.Bff.OrderServices.V1;

/// <summary>
/// Controller for managing orders.
/// Provides endpoints for creating, confirming, canceling, and retrieving orders.
/// All endpoints require JWT authorization.
/// </summary>
[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    // Inject via IUseCase<TInput, TOutput> interface, not concrete classes
    private readonly IUseCase<CreateOrderDto, CreateOrderResultDto> _createOrderUseCase;
    private readonly IUseCase<ConfirmOrderDto, ConfirmOrderResultDto> _confirmOrderUseCase;
    private readonly IUseCase<CancelOrderDto, CancelOrderResultDto> _cancelOrderUseCase;
    private readonly IUseCase<GetOrderDto, OrderDto> _getOrderUseCase;
    private readonly IUseCase<GetOrdersDto, GetOrdersResultDto> _getOrdersUseCase;

    /// <summary>
    /// Initializes a new instance of the OrderController.
    /// All dependencies are injected via interfaces for loose coupling.
    /// </summary>
    public OrderController(
        IUseCase<CreateOrderDto, CreateOrderResultDto> createOrderUseCase,
        IUseCase<ConfirmOrderDto, ConfirmOrderResultDto> confirmOrderUseCase,
        IUseCase<CancelOrderDto, CancelOrderResultDto> cancelOrderUseCase,
        IUseCase<GetOrderDto, OrderDto> getOrderUseCase,
        IUseCase<GetOrdersDto, GetOrdersResultDto> getOrdersUseCase)
    {
        _createOrderUseCase = createOrderUseCase ?? throw new ArgumentNullException(nameof(createOrderUseCase));
        _confirmOrderUseCase = confirmOrderUseCase ?? throw new ArgumentNullException(nameof(confirmOrderUseCase));
        _cancelOrderUseCase = cancelOrderUseCase ?? throw new ArgumentNullException(nameof(cancelOrderUseCase));
        _getOrderUseCase = getOrderUseCase ?? throw new ArgumentNullException(nameof(getOrderUseCase));
        _getOrdersUseCase = getOrdersUseCase ?? throw new ArgumentNullException(nameof(getOrdersUseCase));
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="createOrderDto">Order creation request containing customer ID, currency, and items</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Created order with its ID</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid request or business rule violation</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValueFailureDetail), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderDto createOrderDto,
        CancellationToken cancellationToken)
    {
        var result = await _createOrderUseCase.ExecuteAsync(createOrderDto, cancellationToken);
        return this.UseCaseToActionResult(result);
    }

    /// <summary>
    /// Confirms an order (idempotent operation).
    /// Transitions order from Placed to Confirmed status and reserves stock.
    /// </summary>
    /// <param name="id">Order ID to confirm</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Confirmation details including order status</returns>
    /// <response code="200">Order confirmed successfully</response>
    /// <response code="400">Invalid order state or business rule violation</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid</response>
    /// <response code="404">Order not found</response>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(typeof(ConfirmOrderResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValueFailureDetail), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var dto = new ConfirmOrderDto(id);
        var result = await _confirmOrderUseCase.ExecuteAsync(dto, cancellationToken);
        return this.UseCaseToActionResult(result);
    }

    /// <summary>
    /// Cancels an order (idempotent operation).
    /// Transitions order to Canceled status and releases reserved stock if applicable.
    /// </summary>
    /// <param name="id">Order ID to cancel</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Cancellation details including order status</returns>
    /// <response code="200">Order canceled successfully</response>
    /// <response code="400">Invalid order state or business rule violation</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid</response>
    /// <response code="404">Order not found</response>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(CancelOrderResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValueFailureDetail), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var dto = new CancelOrderDto(id);
        var result = await _cancelOrderUseCase.ExecuteAsync(dto, cancellationToken);
        return this.UseCaseToActionResult(result);
    }

    /// <summary>
    /// Retrieves a single order by ID.
    /// </summary>
    /// <param name="id">Order ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Complete order details including items</returns>
    /// <response code="200">Order found and returned</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValueFailureDetail), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var dto = new GetOrderDto(id);
        var result = await _getOrderUseCase.ExecuteAsync(dto, cancellationToken);
        return this.UseCaseToActionResult(result);
    }

    /// <summary>
    /// Retrieves orders with pagination and filtering.
    /// </summary>
    /// <param name="customerId">Filter by customer ID (optional)</param>
    /// <param name="status">Filter by order status: Draft, Placed, Confirmed, Canceled (optional)</param>
    /// <param name="fromDate">Filter orders created from this date (optional)</param>
    /// <param name="toDate">Filter orders created until this date (optional)</param>
    /// <param name="page">Page number (default: 1, minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, minimum: 1, maximum: 100)</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Paginated list of orders with summary information</returns>
    /// <response code="200">Orders found and returned with pagination info</response>
    /// <response code="400">Invalid pagination or filter parameters</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetOrdersResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValueFailureDetail), StatusCodes.Status400BadRequest)]
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
        return this.UseCaseToActionResult(result);
    }
}
