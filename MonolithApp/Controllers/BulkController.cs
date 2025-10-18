using Microsoft.AspNetCore.Mvc;
using MonolithApp.Models;
using MonolithApp.Services;

namespace MonolithApp.Controllers;

/// <summary>
/// Bulk data processing endpoints for load testing
/// </summary>
[ApiController]
[Route("[controller]")]
public class BulkController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<BulkController> _logger;

    public BulkController(IOrderService orderService, ILogger<BulkController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Process bulk data for load testing
    /// </summary>
    /// <param name="count">Number of items to process (default: 100)</param>
    /// <returns>Bulk processing result with timing information</returns>
    /// <response code="200">Bulk processing completed successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ProcessBulk([FromQuery] int count = 100)
    {
        try
        {
            _logger.LogInformation("Starting bulk processing with {Count} items", count);
            
            var startTime = DateTime.UtcNow;
            
            // Generate sample orders
            var orders = GenerateSampleOrders(count);
            
            // Process bulk orders
            await _orderService.CreateBulkOrdersAsync(orders);
            
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            _logger.LogInformation("Bulk processing completed in {Duration}ms", duration.TotalMilliseconds);

            return Ok(new
            {
                processedCount = count,
                durationMs = duration.TotalMilliseconds,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk processing");
            return StatusCode(500, "Internal server error");
        }
    }

    private List<Order> GenerateSampleOrders(int count)
    {
        var random = new Random();
        var products = new[] { "Laptop", "Mouse", "Keyboard", "Monitor", "Headphones", "Webcam", "Tablet", "Phone" };
        
        var orders = new List<Order>();
        for (int i = 0; i < count; i++)
        {
            orders.Add(new Order
            {
                ProductName = products[random.Next(products.Length)],
                Quantity = random.Next(1, 10),
                Price = (decimal)(random.NextDouble() * 1000 + 10)
            });
        }
        
        return orders;
    }
}
