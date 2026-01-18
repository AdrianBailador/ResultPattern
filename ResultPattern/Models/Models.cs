namespace ResultPattern.Models;

/// <summary>
/// User entity.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request to create a new user.
/// </summary>
public record CreateUserRequest(string Name, string Email);

/// <summary>
/// Request to update an existing user.
/// </summary>
public record UpdateUserRequest(string? Name, string? Email);

/// <summary>
/// User response DTO.
/// </summary>
public record UserResponse(int Id, string Name, string Email, DateTime CreatedAt);

/// <summary>
/// Product entity.
/// </summary>
public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Sku { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request to create a new product.
/// </summary>
public record CreateProductRequest(string Name, string? Description, decimal Price, int Stock, string Sku);

/// <summary>
/// Product response DTO.
/// </summary>
public record ProductResponse(Guid Id, string Name, string? Description, decimal Price, int Stock, string Sku);

/// <summary>
/// Order entity.
/// </summary>
public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Order item entity.
/// </summary>
public class OrderItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

/// <summary>
/// Order status enumeration.
/// </summary>
public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// Request to create a new order.
/// </summary>
public record CreateOrderRequest(int UserId, List<OrderItemRequest> Items);

/// <summary>
/// Order item request.
/// </summary>
public record OrderItemRequest(Guid ProductId, int Quantity);

/// <summary>
/// Order response DTO.
/// </summary>
public record OrderResponse(
    Guid Id,
    int UserId,
    List<OrderItemResponse> Items,
    decimal Total,
    string Status,
    DateTime CreatedAt);

/// <summary>
/// Order item response DTO.
/// </summary>
public record OrderItemResponse(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal Subtotal);