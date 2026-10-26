using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service) => _service = service;

    // GET api/users?page=1&pageSize=20&department=IT&search=daniel
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? department = null,
        [FromQuery] string? search = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { error = "page must be >= 1 and pageSize must be between 1 and 100." });

        var (items, total) = _service.GetAll(page, pageSize, department, search);
        Response.Headers["X-Total-Count"] = total.ToString();
        return Ok(items);
    }

    // GET api/users/5
    [HttpGet("{id:int}")]
    public ActionResult<User> GetUser(int id)
    {
        var user = _service.GetById(id);
        return user is null
            ? NotFound(new { error = $"User with id {id} was not found." })
            : Ok(user);
    }

    // POST api/users
    [HttpPost]
    public ActionResult<User> CreateUser([FromBody] UserRequest request)
    {
        if (_service.EmailExists(request.Email))
            return Conflict(new { error = "A user with this email already exists." });

        var created = _service.Create(request);
        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }

    // PUT api/users/5
    [HttpPut("{id:int}")]
    public ActionResult<User> UpdateUser(int id, [FromBody] UserRequest request)
    {
        if (_service.GetById(id) is null)
            return NotFound(new { error = $"User with id {id} was not found." });

        if (_service.EmailExists(request.Email, excludeUserId: id))
            return Conflict(new { error = "Another user already uses this email." });

        var updated = _service.Update(id, request);
        return Ok(updated);
    }

    // DELETE api/users/5
    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        return _service.Delete(id)
            ? NoContent()
            : NotFound(new { error = $"User with id {id} was not found." });
    }
}
