using ResultPattern.Core;
using ResultPattern.Errors;
using ResultPattern.Models;

namespace ResultPattern.Services;

/// <summary>
/// User service demonstrating the Result Pattern for error handling.
/// </summary>
public class UserService
{
    // In-memory storage for demo purposes
    private static readonly List<User> _users = new()
    {
        new User { Id = 1, Name = "Alice Johnson", Email = "alice@example.com" },
        new User { Id = 2, Name = "Bob Smith", Email = "bob@example.com" },
        new User { Id = 3, Name = "Charlie Brown", Email = "charlie@example.com" }
    };

    private static int _nextId = 4;

    /// <summary>
    /// Gets a user by ID. Returns a failure if not found.
    /// </summary>
    public Result<User> GetById(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);

        if (user is null)
            return DomainErrors.User.NotFound(id);

        return user; // Implicit conversion to Result<User>.Success
    }

    /// <summary>
    /// Gets a user by email. Returns a failure if not found.
    /// </summary>
    public Result<User> GetByEmail(string email)
    {
        var user = _users.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user is null)
            return DomainErrors.User.NotFoundByEmail(email);

        return user;
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    public Result<IEnumerable<User>> GetAll()
    {
        return Result<IEnumerable<User>>.Success(_users.AsEnumerable());
    }

    /// <summary>
    /// Creates a new user with validation.
    /// </summary>
    public Result<User> Create(CreateUserRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
            return DomainErrors.User.NameRequired;

        if (request.Name.Length < 2)
            return DomainErrors.User.NameTooShort;

        if (string.IsNullOrWhiteSpace(request.Email))
            return DomainErrors.User.EmailRequired;

        if (!IsValidEmail(request.Email))
            return DomainErrors.User.InvalidEmail;

        // Check for duplicate email
        if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
            return DomainErrors.User.EmailAlreadyExists(request.Email);

        // Create user
        var user = new User
        {
            Id = _nextId++,
            Name = request.Name.Trim(),
            Email = request.Email.ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(user);

        return user;
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    public Result<User> Update(int id, UpdateUserRequest request)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);

        if (user is null)
            return DomainErrors.User.NotFound(id);

        // Validate and update name if provided
        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return DomainErrors.User.NameRequired;

            if (request.Name.Length < 2)
                return DomainErrors.User.NameTooShort;

            user.Name = request.Name.Trim();
        }

        // Validate and update email if provided
        if (request.Email is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return DomainErrors.User.EmailRequired;

            if (!IsValidEmail(request.Email))
                return DomainErrors.User.InvalidEmail;

            // Check for duplicate email (excluding current user)
            if (_users.Any(u => u.Id != id && 
                u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                return DomainErrors.User.EmailAlreadyExists(request.Email);

            user.Email = request.Email.ToLowerInvariant();
        }

        return user;
    }

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    public Result Delete(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);

        if (user is null)
            return DomainErrors.User.NotFound(id);

        _users.Remove(user);

        return Result.Success();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}