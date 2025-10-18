using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers;

/// <summary>
/// Controller for Order operations
/// </summary>
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all orders or filters by status
    /// </summary>
    /// <param name="status">Optional status filter (Pending, Processing, Completed, Cancelled)</param>
    /// <returns>List of orders</returns>
    /// <response code="200">Returns list of orders</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] string? status = null)
    {
        _logger.LogInformation("Getting orders with status filter: {Status}", status ?? "all");
        
        var orders = string.IsNullOrEmpty(status)
            ? await _orderService.GetAllOrdersAsync()
            : await _orderService.GetOrdersByStatusAsync(status);

        return Ok(orders);
    }

    /// <summary>
    /// Gets a specific order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    /// <response code="200">Returns the order</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        
        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", id);
            return NotFound(new { message = $"Order with ID {id} not found" });
        }

        return Ok(order);
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="orderDto">Order data</param>
    /// <returns>Created order</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid order data</response>
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] OrderDto orderDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var order = await _orderService.CreateOrderAsync(orderDto);
        
        _logger.LogInformation("Order created with ID: {OrderId}", order.Id);
        
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    /// <summary>
    /// Updates an existing order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="orderDto">Updated order data</param>
    /// <returns>Updated order</returns>
    /// <response code="200">Order updated successfully</response>
    /// <response code="400">Invalid order data</response>
    /// <response code="404">Order not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> UpdateOrder(int id, [FromBody] OrderDto orderDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var order = await _orderService.UpdateOrderAsync(id, orderDto);
        
        if (order == null)
        {
            return NotFound(new { message = $"Order with ID {id} not found" });
        }

        return Ok(order);
    }

    /// <summary>
    /// Deletes an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Order deleted successfully</response>
    /// <response code="404">Order not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var result = await _orderService.DeleteOrderAsync(id);
        
        if (!result)
        {
            return NotFound(new { message = $"Order with ID {id} not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Gets order statistics
    /// </summary>
    /// <returns>Order statistics</returns>
    /// <response code="200">Returns order statistics</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetStats()
    {
        var totalCount = await _orderService.GetOrderCountAsync();
        
        return Ok(new
        {
            TotalOrders = totalCount,
            Timestamp = DateTime.UtcNow
        });
    }
}

