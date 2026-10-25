using System.ComponentModel.DataAnnotations;

namespace MetropolitanCoffee.Web.Models;

public sealed class RegisterViewModel
{
    [Required] public string Username { get; set; } = "";
    [Required] public string Email { get; set; } = "";
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    [Required] public string Phone { get; set; } = "";
    [Required] public string Ward { get; set; } = "";
    [Required] public string Address { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    [Required] public string ConfirmPassword { get; set; } = "";
}
