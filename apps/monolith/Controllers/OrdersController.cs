using Microsoft.AspNetCore.Mvc;
using MonolithApp.Models;
using MonolithApp.Services;

namespace MonolithApp.Controllers;

/// <summary>
/// Orders management endpoints for CRUD operations
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
    /// Get all orders
    /// </summary>
    /// <returns>List of all orders</returns>
    /// <response code="200">Returns the list of orders</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="order">Order data</param>
    /// <returns>Created order</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid order data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
    {
        try
        {
            if (order == null || string.IsNullOrEmpty(order.ProductName))
            {
                return BadRequest("Invalid order data");
            }

            var createdOrder = await _orderService.CreateOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Order>> UpdateOrder(int id, [FromBody] Order order)
    {
        try
        {
            if (order == null || string.IsNullOrEmpty(order.ProductName))
            {
                return BadRequest("Invalid order data");
            }

            var updatedOrder = await _orderService.UpdateOrderAsync(id, order);
            if (updatedOrder == null)
            {
                return NotFound();
            }
            return Ok(updatedOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteOrder(int id)
    {
        try
        {
            var result = await _orderService.DeleteOrderAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order with ID: {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
