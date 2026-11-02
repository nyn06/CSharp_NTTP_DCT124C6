using System.ComponentModel.DataAnnotations;

namespace MetropolitanCoffee.Web.Models;

public sealed class ProfileEditViewModel
{
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    [Required] public string Phone { get; set; } = "";
    [Required] public string Ward { get; set; } = "";
    [Required] public string Address { get; set; } = "";
}
