using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Web.Models;

public static class ProductImageResolver
{
    public static string Resolve(Product product, string webRootPath)
    {
        var imageName = (product.Image ?? "").Replace('\\', '/').Split('/').LastOrDefault() ?? "";
        // The B04 export refers to an older missing Mocha upload; this later Mocha image exists in B04.
        if (product.Id == 7 && imageName.Equals("1775676167_Mocha.jpg", StringComparison.Ordinal) &&
            File.Exists(Path.Combine(webRootPath, "assets", "img", "1775678316_mocha.jpg")))
        {
            imageName = "1775678316_mocha.jpg";
        }

        return !string.IsNullOrWhiteSpace(imageName) &&
               File.Exists(Path.Combine(webRootPath, "assets", "img", imageName))
            ? "~/assets/img/" + Uri.EscapeDataString(imageName)
            : "~/assets/img/MetropolitanCafeLogo.png";
    }
}
