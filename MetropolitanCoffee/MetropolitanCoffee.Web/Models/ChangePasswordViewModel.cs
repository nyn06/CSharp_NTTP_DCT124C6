using System.ComponentModel.DataAnnotations;

namespace MetropolitanCoffee.Web.Models;

public sealed class ChangePasswordViewModel
{
    [Required] public string CurrentPassword { get; set; } = "";
    [Required] public string NewPassword { get; set; } = "";
    [Required] public string ConfirmNewPassword { get; set; } = "";
}
