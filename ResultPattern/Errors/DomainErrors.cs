using ResultPattern.Core;

namespace ResultPattern.Errors;

/// <summary>
/// Centralised domain error definitions.
/// Keeping all errors in one place ensures consistency and makes documentation easier.
/// </summary>
public static class DomainErrors
{
    public static class User
    {
        public static Error NotFound(int id) =>
            Error.NotFound("User.NotFound", $"User with ID {id} was not found");

        public static Error NotFoundByEmail(string email) =>
            Error.NotFound("User.NotFoundByEmail", $"User with email '{email}' was not found");

        public static Error EmailAlreadyExists(string email) =>
            Error.Conflict("User.EmailExists", $"A user with email '{email}' already exists");

        public static readonly Error InvalidEmail =
            Error.Validation("User.InvalidEmail", "The email format is invalid");

        public static readonly Error EmailRequired =
            Error.Validation("User.EmailRequired", "Email is required");

        public static readonly Error NameRequired =
            Error.Validation("User.NameRequired", "Name is required");

        public static readonly Error NameTooShort =
            Error.Validation("User.NameTooShort", "Name must be at least 2 characters");

        public static readonly Error PasswordTooWeak =
            Error.Validation("User.PasswordTooWeak",
                "Password must be at least 8 characters with uppercase, lowercase, and digits");

        public static readonly Error InvalidCredentials =
            Error.Unauthorized("User.InvalidCredentials", "Invalid email or password");

        public static readonly Error AccountLocked =
            Error.Forbidden("User.AccountLocked", "Account is locked due to too many failed attempts");
    }

    public static class Product
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound("Product.NotFound", $"Product with ID {id} was not found");

        public static Error NotFoundBySku(string sku) =>
            Error.NotFound("Product.NotFoundBySku", $"Product with SKU '{sku}' was not found");

        public static readonly Error NameRequired =
            Error.Validation("Product.NameRequired", "Product name is required");

        public static readonly Error InvalidPrice =
            Error.Validation("Product.InvalidPrice", "Price must be greater than zero");

        public static readonly Error InvalidStock =
            Error.Validation("Product.InvalidStock", "Stock cannot be negative");

        public static Error SkuAlreadyExists(string sku) =>
            Error.Conflict("Product.SkuExists", $"A product with SKU '{sku}' already exists");

        public static Error InsufficientStock(Guid productId, int requested, int available) =>
            Error.Conflict("Product.InsufficientStock",
                $"Insufficient stock for product {productId}. Requested: {requested}, Available: {available}");
    }

    public static class Order
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound("Order.NotFound", $"Order with ID {id} was not found");

        public static readonly Error EmptyCart =
            Error.Validation("Order.EmptyCart", "Cannot create order with empty cart");

        public static readonly Error InvalidQuantity =
            Error.Validation("Order.InvalidQuantity", "Quantity must be at least 1");

        public static Error TotalExceedsLimit(decimal total, decimal limit) =>
            Error.Validation("Order.TotalExceedsLimit",
                $"Order total ({total:C}) exceeds maximum allowed ({limit:C})");

        public static Error PaymentFailed(string reason) =>
            Error.Failure("Order.PaymentFailed", $"Payment failed: {reason}");

        public static readonly Error AlreadyShipped =
            Error.Conflict("Order.AlreadyShipped", "Cannot modify an order that has already shipped");

        public static readonly Error AlreadyCancelled =
            Error.Conflict("Order.AlreadyCancelled", "Order has already been cancelled");
    }

    public static class Authentication
    {
        public static readonly Error InvalidToken =
            Error.Unauthorized("Auth.InvalidToken", "The authentication token is invalid or expired");

        public static readonly Error MissingToken =
            Error.Unauthorized("Auth.MissingToken", "Authentication token is required");

        public static readonly Error InsufficientPermissions =
            Error.Forbidden("Auth.InsufficientPermissions", "You do not have permission to perform this action");
    }
}