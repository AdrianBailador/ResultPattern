using ResultPattern.Core;
using ResultPattern.Errors;
using ResultPattern.Models;

namespace ResultPattern.Services;

/// <summary>
/// Product service with Result Pattern.
/// </summary>
public class ProductService
{
    // In-memory storage for demo purposes
    private static readonly List<Product> _products = new()
    {
        new Product
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Laptop Pro",
            Description = "High-performance laptop",
            Price = 1299.99m,
            Stock = 50,
            Sku = "LAPTOP-001"
        },
        new Product
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Wireless Mouse",
            Description = "Ergonomic wireless mouse",
            Price = 49.99m,
            Stock = 200,
            Sku = "MOUSE-001"
        },
        new Product
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "USB-C Cable",
            Description = "2m USB-C to USB-C cable",
            Price = 19.99m,
            Stock = 500,
            Sku = "CABLE-001"
        },
        new Product
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "Monitor 27\"",
            Description = "4K IPS Monitor",
            Price = 599.99m,
            Stock = 30,
            Sku = "MONITOR-001"
        }
    };

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    public Result<Product> GetById(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);

        if (product is null)
            return DomainErrors.Product.NotFound(id);

        return product;
    }

    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    public Result<Product> GetBySku(string sku)
    {
        var product = _products.FirstOrDefault(p => 
            p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));

        if (product is null)
            return DomainErrors.Product.NotFoundBySku(sku);

        return product;
    }

    /// <summary>
    /// Gets all products.
    /// </summary>
    public Result<IEnumerable<Product>> GetAll()
    {
        return Result<IEnumerable<Product>>.Success(_products.AsEnumerable());
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    public Result<Product> Create(CreateProductRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
            return DomainErrors.Product.NameRequired;

        if (request.Price <= 0)
            return DomainErrors.Product.InvalidPrice;

        if (request.Stock < 0)
            return DomainErrors.Product.InvalidStock;

        // Check for duplicate SKU
        if (_products.Any(p => p.Sku.Equals(request.Sku, StringComparison.OrdinalIgnoreCase)))
            return DomainErrors.Product.SkuAlreadyExists(request.Sku);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            Sku = request.Sku.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow
        };

        _products.Add(product);

        return product;
    }

    /// <summary>
    /// Updates product stock.
    /// </summary>
    public Result<Product> UpdateStock(Guid id, int quantity)
    {
        var productResult = GetById(id);

        if (productResult.IsFailure)
            return productResult;

        var product = productResult.Value;
        var newStock = product.Stock + quantity;

        if (newStock < 0)
            return DomainErrors.Product.InvalidStock;

        product.Stock = newStock;

        return product;
    }
}