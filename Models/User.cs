using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models;

// Stored user record
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

// Input model for POST and PUT (no Id, so clients cannot set it)
public class UserRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "First name cannot be blank.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is required.")]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Last name cannot be blank.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email format is not valid.")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Department cannot be blank.")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Role cannot be blank.")]
    public string Role { get; set; } = string.Empty;
}
