using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using Prometheus;

namespace OrderService.Services;

/// <summary>
/// Implementation of Order service operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;
    private readonly ILogger<OrderService> _logger;

    private static readonly Counter OrdersCreatedCounter = Metrics.CreateCounter(
        "orders_created_total",
        "Total number of orders created");

    private static readonly Counter OrdersRetrievedCounter = Metrics.CreateCounter(
        "orders_retrieved_total",
        "Total number of orders retrieved");

    private static readonly Counter OrdersUpdatedCounter = Metrics.CreateCounter(
        "orders_updated_total",
        "Total number of orders updated");

    private static readonly Counter OrdersDeletedCounter = Metrics.CreateCounter(
        "orders_deleted_total",
        "Total number of orders deleted");

    private static readonly Histogram DatabaseQueryDuration = Metrics.CreateHistogram(
        "orders_database_query_duration_seconds",
        "Duration of database queries in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "operation" },
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
        });

    private static readonly Gauge OrdersTotalCount = Metrics.CreateGauge(
        "orders_total_count",
        "Total number of orders in database");

    public OrderService(OrderDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        using (DatabaseQueryDuration.WithLabels("get_all").NewTimer())
        {
            _logger.LogInformation("Retrieving all orders");
            var orders = await _context.Orders.ToListAsync();
            OrdersRetrievedCounter.Inc(orders.Count);
            return orders;
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
    {
        using (DatabaseQueryDuration.WithLabels("get_by_status").NewTimer())
        {
            _logger.LogInformation("Retrieving orders with status: {Status}", status);
            var orders = await _context.Orders
                .Where(o => o.Status == status)
                .ToListAsync();
            OrdersRetrievedCounter.Inc(orders.Count);
            return orders;
        }
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        using (DatabaseQueryDuration.WithLabels("get_by_id").NewTimer())
        {
            _logger.LogInformation("Retrieving order with ID: {OrderId}", id);
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                OrdersRetrievedCounter.Inc();
            }
            return order;
        }
    }

    public async Task<Order> CreateOrderAsync(OrderDto orderDto)
    {
        using (DatabaseQueryDuration.WithLabels("create").NewTimer())
        {
            var order = new Order
            {
                ProductName = orderDto.ProductName,
                Quantity = orderDto.Quantity,
                Price = orderDto.Price,
                Status = orderDto.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            OrdersCreatedCounter.Inc();
            OrdersTotalCount.Inc();

            _logger.LogInformation("Order created with ID: {OrderId}", order.Id);
            return order;
        }
    }

    public async Task<Order?> UpdateOrderAsync(int id, OrderDto orderDto)
    {
        using (DatabaseQueryDuration.WithLabels("update").NewTimer())
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found for update", id);
                return null;
            }

            order.ProductName = orderDto.ProductName;
            order.Quantity = orderDto.Quantity;
            order.Price = orderDto.Price;
            order.Status = orderDto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            OrdersUpdatedCounter.Inc();

            _logger.LogInformation("Order with ID {OrderId} updated", id);
            return order;
        }
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        using (DatabaseQueryDuration.WithLabels("delete").NewTimer())
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found for deletion", id);
                return false;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            OrdersDeletedCounter.Inc();
            OrdersTotalCount.Dec();

            _logger.LogInformation("Order with ID {OrderId} deleted", id);
            return true;
        }
    }

    public async Task<int> GetOrderCountAsync()
    {
        using (DatabaseQueryDuration.WithLabels("count").NewTimer())
        {
            var count = await _context.Orders.CountAsync();
            OrdersTotalCount.Set(count);
            return count;
        }
    }
}

