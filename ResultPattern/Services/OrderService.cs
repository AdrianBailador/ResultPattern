using ResultPattern.Core;
using ResultPattern.Errors;
using ResultPattern.Extensions;
using ResultPattern.Models;

namespace ResultPattern.Services;

/// <summary>
/// Order service demonstrating Result Pattern chaining with Bind.
/// </summary>
public class OrderService
{
    private readonly UserService _userService;
    private readonly ProductService _productService;

    // In-memory storage for demo purposes
    private static readonly List<Order> _orders = new();

    public OrderService(UserService userService, ProductService productService)
    {
        _userService = userService;
        _productService = productService;
    }

    /// <summary>
    /// Gets an order by ID.
    /// </summary>
    public Result<Order> GetById(Guid id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);

        if (order is null)
            return DomainErrors.Order.NotFound(id);

        return order;
    }

    /// <summary>
    /// Gets all orders for a user.
    /// </summary>
    public Result<IEnumerable<Order>> GetByUserId(int userId)
    {
        // First verify user exists
        var userResult = _userService.GetById(userId);
        
        if (userResult.IsFailure)
            return Result<IEnumerable<Order>>.Failure(userResult.Error);

        var orders = _orders.Where(o => o.UserId == userId);
        return Result<IEnumerable<Order>>.Success(orders);
    }

    /// <summary>
    /// Creates an order using Result Pattern chaining.
    /// Demonstrates: ValidateRequest → ValidateUser → ValidateProducts → CreateOrder
    /// </summary>
    public Result<Order> Create(CreateOrderRequest request)
    {
        return ValidateRequest(request)
            .Bind(_ => ValidateUser(request.UserId))
            .Bind(_ => ValidateAndReserveProducts(request.Items))
            .Bind(items => CreateOrderInternal(request.UserId, items));
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    public Result<Order> Cancel(Guid orderId)
    {
        return GetById(orderId)
            .Bind(ValidateCanCancel)
            .Tap(order => order.Status = OrderStatus.Cancelled);
    }

    /// <summary>
    /// Ships an order - demonstrates Match usage.
    /// </summary>
    public Result<Order> Ship(Guid orderId)
    {
        return GetById(orderId)
            .Bind(ValidateCanShip)
            .Tap(order => order.Status = OrderStatus.Shipped);
    }

    // Private validation methods that return Results

    private Result<CreateOrderRequest> ValidateRequest(CreateOrderRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
            return DomainErrors.Order.EmptyCart;

        foreach (var item in request.Items)
        {
            if (item.Quantity < 1)
                return DomainErrors.Order.InvalidQuantity;
        }

        return request;
    }

    private Result<User> ValidateUser(int userId)
    {
        return _userService.GetById(userId);
    }

    private Result<List<OrderItem>> ValidateAndReserveProducts(List<OrderItemRequest> items)
    {
        var orderItems = new List<OrderItem>();

        foreach (var item in items)
        {
            var productResult = _productService.GetById(item.ProductId);

            if (productResult.IsFailure)
                return Result<List<OrderItem>>.Failure(productResult.Error);

            var product = productResult.Value;

            // Check stock
            if (product.Stock < item.Quantity)
                return DomainErrors.Product.InsufficientStock(
                    product.Id, item.Quantity, product.Stock);

            // Reserve stock (in real app, this would be transactional)
            product.Stock -= item.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        return orderItems;
    }

    private Result<Order> CreateOrderInternal(int userId, List<OrderItem> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Items = items,
            Status = OrderStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        // Check order total limit
        const decimal maxOrderTotal = 10_000m;
        if (order.Total > maxOrderTotal)
            return DomainErrors.Order.TotalExceedsLimit(order.Total, maxOrderTotal);

        _orders.Add(order);

        return order;
    }

    private Result<Order> ValidateCanCancel(Order order)
    {
        if (order.Status == OrderStatus.Cancelled)
            return DomainErrors.Order.AlreadyCancelled;

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
            return DomainErrors.Order.AlreadyShipped;

        return order;
    }

    private Result<Order> ValidateCanShip(Order order)
    {
        if (order.Status == OrderStatus.Cancelled)
            return DomainErrors.Order.AlreadyCancelled;

        if (order.Status == OrderStatus.Shipped)
            return DomainErrors.Order.AlreadyShipped;

        if (order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Processing)
            return Error.Validation("Order.InvalidStatus", 
                $"Cannot ship order with status {order.Status}");

        return order;
    }
}