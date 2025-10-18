using Microsoft.EntityFrameworkCore;
using MonolithApp.Data;
using MonolithApp.Models;

namespace MonolithApp.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        _logger.LogInformation("Retrieving all orders");
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving order with ID: {OrderId}", id);
        return await _context.Orders.FindAsync(id);
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        _logger.LogInformation("Creating new order for product: {ProductName}", order.ProductName);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> UpdateOrderAsync(int id, Order order)
    {
        _logger.LogInformation("Updating order with ID: {OrderId}", id);
        var existingOrder = await _context.Orders.FindAsync(id);
        if (existingOrder == null)
        {
            return null;
        }

        existingOrder.ProductName = order.ProductName;
        existingOrder.Quantity = order.Quantity;
        existingOrder.Price = order.Price;

        await _context.SaveChangesAsync();
        return existingOrder;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        _logger.LogInformation("Deleting order with ID: {OrderId}", id);
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return false;
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Order>> CreateBulkOrdersAsync(IEnumerable<Order> orders)
    {
        _logger.LogInformation("Creating bulk orders. Count: {OrderCount}", orders.Count());
        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();
        return orders;
    }
}
