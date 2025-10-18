using System.ComponentModel.DataAnnotations;

namespace OrderService.Models;

/// <summary>
/// Order entity
/// </summary>
public class Order
{
    /// <summary>
    /// Order ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity ordered
    /// </summary>
    [Range(1, 10000)]
    public int Quantity { get; set; }

    /// <summary>
    /// Price per unit
    /// </summary>
    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    /// <summary>
    /// Total price (calculated)
    /// </summary>
    public decimal TotalPrice => Quantity * Price;

    /// <summary>
    /// Order creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Order update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Order status
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";
}

/// <summary>
/// DTO for creating/updating orders
/// </summary>
public class OrderDto
{
    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, 10000)]
    public int Quantity { get; set; }

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Pending";
}

