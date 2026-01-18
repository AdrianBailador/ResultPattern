using ResultPattern.Extensions;
using ResultPattern.Models;
using ResultPattern.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<OrderService>();

// Configure ProblemDetails
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = 
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
    };
});

var app = builder.Build();

// ============================================================
// User Endpoints
// ============================================================

var users = app.MapGroup("/api/users")
    .WithTags("Users");

users.MapGet("/", (UserService service) =>
{
    return service.GetAll().ToApiResult();
})
.WithName("GetAllUsers")
.WithDescription("Get all users");

users.MapGet("/{id:int}", (int id, UserService service) =>
{
    return service.GetById(id).ToApiResult();
})
.WithName("GetUserById")
.WithDescription("Get a user by ID");

users.MapGet("/email/{email}", (string email, UserService service) =>
{
    return service.GetByEmail(email).ToApiResult();
})
.WithName("GetUserByEmail")
.WithDescription("Get a user by email");

users.MapPost("/", (CreateUserRequest request, UserService service) =>
{
    return service.Create(request)
        .ToCreatedResult(user => $"/api/users/{user.Id}");
})
.WithName("CreateUser")
.WithDescription("Create a new user");

users.MapPut("/{id:int}", (int id, UpdateUserRequest request, UserService service) =>
{
    return service.Update(id, request).ToApiResult();
})
.WithName("UpdateUser")
.WithDescription("Update an existing user");

users.MapDelete("/{id:int}", (int id, UserService service) =>
{
    return service.Delete(id).ToNoContentResult();
})
.WithName("DeleteUser")
.WithDescription("Delete a user");

// ============================================================
// Product Endpoints
// ============================================================

var products = app.MapGroup("/api/products")
    .WithTags("Products");

products.MapGet("/", (ProductService service) =>
{
    return service.GetAll().ToApiResult();
})
.WithName("GetAllProducts")
.WithDescription("Get all products");

products.MapGet("/{id:guid}", (Guid id, ProductService service) =>
{
    return service.GetById(id).ToApiResult();
})
.WithName("GetProductById")
.WithDescription("Get a product by ID");

products.MapGet("/sku/{sku}", (string sku, ProductService service) =>
{
    return service.GetBySku(sku).ToApiResult();
})
.WithName("GetProductBySku")
.WithDescription("Get a product by SKU");

products.MapPost("/", (CreateProductRequest request, ProductService service) =>
{
    return service.Create(request)
        .ToCreatedResult(product => $"/api/products/{product.Id}");
})
.WithName("CreateProduct")
.WithDescription("Create a new product");

products.MapPatch("/{id:guid}/stock", (Guid id, StockUpdateRequest request, ProductService service) =>
{
    return service.UpdateStock(id, request.Quantity).ToApiResult();
})
.WithName("UpdateProductStock")
.WithDescription("Update product stock");

// ============================================================
// Order Endpoints - Demonstrates chaining with Bind
// ============================================================

var orders = app.MapGroup("/api/orders")
    .WithTags("Orders");

orders.MapGet("/{id:guid}", (Guid id, OrderService service) =>
{
    return service.GetById(id).ToApiResult();
})
.WithName("GetOrderById")
.WithDescription("Get an order by ID");

orders.MapGet("/user/{userId:int}", (int userId, OrderService service) =>
{
    return service.GetByUserId(userId).ToApiResult();
})
.WithName("GetOrdersByUser")
.WithDescription("Get all orders for a user");

orders.MapPost("/", (CreateOrderRequest request, OrderService service) =>
{
    return service.Create(request)
        .ToCreatedResult(order => $"/api/orders/{order.Id}");
})
.WithName("CreateOrder")
.WithDescription("Create a new order (demonstrates Result chaining)");

orders.MapPost("/{id:guid}/cancel", (Guid id, OrderService service) =>
{
    return service.Cancel(id).ToApiResult();
})
.WithName("CancelOrder")
.WithDescription("Cancel an order");

orders.MapPost("/{id:guid}/ship", (Guid id, OrderService service) =>
{
    return service.Ship(id).ToApiResult();
})
.WithName("ShipOrder")
.WithDescription("Ship an order");

// ============================================================
// Demo Endpoint - Shows Match usage
// ============================================================

app.MapGet("/api/demo/match/{userId:int}", (int userId, UserService service) =>
{
    // Demonstrates using Match for different outcomes
    return service.GetById(userId).Match(
        onSuccess: user => Results.Ok(new
        {
            Message = $"Welcome back, {user.Name}!",
            User = new UserResponse(user.Id, user.Name, user.Email, user.CreatedAt)
        }),
        onFailure: error => Results.NotFound(new
        {
            Message = "User not found",
            Error = error.Description
        })
    );
})
.WithName("DemoMatch")
.WithTags("Demo")
.WithDescription("Demonstrates Match pattern for handling Results");

// ============================================================
// Health Check & Info
// ============================================================

app.MapGet("/", () => Results.Ok(new
{
    Title = "Result Pattern Demo API",
    Description = "Demonstrates the Result Pattern for error handling in C#",
    Version = "1.0.0",
    Endpoints = new
    {
        Users = new[] { "GET /api/users", "GET /api/users/{id}", "POST /api/users" },
        Products = new[] { "GET /api/products", "GET /api/products/{id}", "POST /api/products" },
        Orders = new[] { "GET /api/orders/{id}", "POST /api/orders", "POST /api/orders/{id}/cancel" },
        Demo = new[] { "GET /api/demo/match/{userId}" }
    },
    TestData = new
    {
        Users = "IDs 1, 2, 3 exist",
        Products = "IDs 11111111-1111-1111-1111-111111111111 through 44444444-4444-4444-4444-444444444444 exist"
    }
}));

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithTags("Health");

app.Run();

// Additional request types
public record StockUpdateRequest(int Quantity);