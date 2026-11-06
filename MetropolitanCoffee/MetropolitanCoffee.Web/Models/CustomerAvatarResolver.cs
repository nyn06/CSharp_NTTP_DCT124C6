using System.Text.RegularExpressions;

namespace MetropolitanCoffee.Web.Models;

public static class CustomerAvatarResolver
{
    private const string Default = "~/assets/img/MetropolitanCafeLogo.png";
    private static readonly Regex SafePath = new(@"\Aassets/img/avatars/[A-Za-z0-9_-]+\.(?:jpg|png|gif|webp)\z",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string Resolve(string? storedPath, string webRootPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath) || !SafePath.IsMatch(storedPath)) return Default;
        var fileName = storedPath[("assets/img/avatars/".Length)..];
        return File.Exists(Path.Combine(webRootPath, "assets", "img", "avatars", fileName))
            ? "~/assets/img/avatars/" + Uri.EscapeDataString(fileName)
            : Default;
    }
}
