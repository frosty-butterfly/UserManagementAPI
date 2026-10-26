using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

public interface IUserService
{
    (IReadOnlyList<User> Items, int Total) GetAll(int page, int pageSize, string? department, string? search);
    User? GetById(int id);
    bool EmailExists(string email, int? excludeUserId = null);
    User Create(UserRequest request);
    User? Update(int id, UserRequest request);
    bool Delete(int id);
}
