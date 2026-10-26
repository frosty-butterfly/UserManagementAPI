using System.Collections.Concurrent;
using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

// In-memory store. Thread-safe, so parallel requests do not crash the API.
public class UserService : IUserService
{
    private readonly ConcurrentDictionary<int, User> _users = new();
    private int _nextId = 0;

    public UserService()
    {
        // Seed data
        Create(new UserRequest { FirstName = "Aisyah", LastName = "Rahman", Email = "aisyah.rahman@techhive.com", Department = "HR", Role = "HR Manager" });
        Create(new UserRequest { FirstName = "Daniel", LastName = "Lee", Email = "daniel.lee@techhive.com", Department = "IT", Role = "System Admin" });
        Create(new UserRequest { FirstName = "Priya", LastName = "Nair", Email = "priya.nair@techhive.com", Department = "IT", Role = "Developer" });
    }

    public (IReadOnlyList<User> Items, int Total) GetAll(int page, int pageSize, string? department, string? search)
    {
        IEnumerable<User> query = _users.Values;

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(u => u.Department.Equals(department, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));

        var filtered = query.OrderBy(u => u.Id).ToList();

        // Pagination: return only one page, not the whole list
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return (items, filtered.Count);
    }

    public User? GetById(int id) => _users.TryGetValue(id, out var user) ? user : null;

    public bool EmailExists(string email, int? excludeUserId = null) =>
        _users.Values.Any(u =>
            u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase) &&
            (excludeUserId is null || u.Id != excludeUserId));

    public User Create(UserRequest request)
    {
        var id = Interlocked.Increment(ref _nextId);
        var user = new User
        {
            Id = id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Department = request.Department.Trim(),
            Role = request.Role.Trim()
        };
        _users[id] = user;
        return user;
    }

    public User? Update(int id, UserRequest request)
    {
        if (!_users.TryGetValue(id, out var existing))
            return null;

        existing.FirstName = request.FirstName.Trim();
        existing.LastName = request.LastName.Trim();
        existing.Email = request.Email.Trim();
        existing.Department = request.Department.Trim();
        existing.Role = request.Role.Trim();
        return existing;
    }

    public bool Delete(int id) => _users.TryRemove(id, out _);
}
