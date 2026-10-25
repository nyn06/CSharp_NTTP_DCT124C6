using System.ComponentModel.DataAnnotations;

namespace MetropolitanCoffee.Web.Models;

public sealed class LoginViewModel
{
    [Required] public string Username { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
