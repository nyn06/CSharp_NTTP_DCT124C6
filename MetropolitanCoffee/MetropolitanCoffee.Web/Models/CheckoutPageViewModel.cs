using System.ComponentModel.DataAnnotations;
using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Web.Models;

public sealed class CheckoutPageViewModel
{
    public CheckoutPreview Preview { get; set; } = new();
    public CheckoutProfile Profile { get; set; } = new("", "", "", "", "");
    public bool BuyNow { get; set; }
    public string RequestId { get; set; } = "";
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    [Required] public string Address { get; set; } = "";
    [Required] public string Ward { get; set; } = "";
    public string? Note { get; set; }
    [Required] public string PaymentMethod { get; set; } = "";
}
