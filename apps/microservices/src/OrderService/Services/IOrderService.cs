using OrderService.Models;

namespace OrderService.Services;

/// <summary>
/// Interface for Order service operations
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Gets all orders
    /// </summary>
    Task<IEnumerable<Order>> GetAllOrdersAsync();

    /// <summary>
    /// Gets orders by status
    /// </summary>
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);

    /// <summary>
    /// Gets an order by ID
    /// </summary>
    Task<Order?> GetOrderByIdAsync(int id);

    /// <summary>
    /// Creates a new order
    /// </summary>
    Task<Order> CreateOrderAsync(OrderDto orderDto);

    /// <summary>
    /// Updates an existing order
    /// </summary>
    Task<Order?> UpdateOrderAsync(int id, OrderDto orderDto);

    /// <summary>
    /// Deletes an order
    /// </summary>
    Task<bool> DeleteOrderAsync(int id);

    /// <summary>
    /// Gets total order count
    /// </summary>
    Task<int> GetOrderCountAsync();
}

