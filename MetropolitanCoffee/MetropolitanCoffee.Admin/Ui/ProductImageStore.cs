namespace MetropolitanCoffee.Admin.Ui;

internal sealed class ProductImageStore
{
    private readonly string _directory;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxUploadBytes = 10 * 1024 * 1024;

    internal ProductImageStore()
    {
        var webRoot = Environment.GetEnvironmentVariable("METROPOLITANCOFFEE_WEBROOT");
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, "MetropolitanCoffee.Web", "wwwroot");
                if (Directory.Exists(candidate)) { webRoot = candidate; break; }
                directory = directory.Parent;
            }
        }
        webRoot ??= Path.Combine(AppContext.BaseDirectory, "wwwroot");
        _directory = Path.Combine(Path.GetFullPath(webRoot), "assets", "img");
    }

    internal Bitmap Load(string? imageName, int? productId = null)
    {
        var name = Path.GetFileName((imageName ?? "").Replace('\\', '/'));
        if (productId == 7 && name.Equals("1775676167_Mocha.jpg", StringComparison.Ordinal) &&
            File.Exists(Path.Combine(_directory, "1775678316_mocha.jpg")))
            name = "1775678316_mocha.jpg";
        var path = Path.Combine(_directory, name);
        if (string.IsNullOrWhiteSpace(name) || !File.Exists(path))
            path = Path.Combine(_directory, "MetropolitanCafeLogo.png");
        try
        {
            using var stream = File.OpenRead(path);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }
        catch
        {
            var placeholder = new Bitmap(80, 60);
            using var graphics = Graphics.FromImage(placeholder);
            graphics.Clear(Color.FromArgb(244, 246, 249));
            using var brush = new SolidBrush(Color.FromArgb(108, 117, 125));
            graphics.DrawString("No image", SystemFonts.DefaultFont, brush, new RectangleF(4, 20, 72, 20));
            return placeholder;
        }
    }

    internal Bitmap LoadLocalPreview(string path)
    {
        using var stream = File.OpenRead(path);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }

    internal async Task<string> SaveUploadAsync(string sourcePath, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension)) throw new InvalidOperationException("Select a JPG, PNG, WebP, or GIF image.");
        var info = new FileInfo(sourcePath);
        if (!info.Exists || info.Length == 0 || info.Length > MaxUploadBytes)
            throw new InvalidOperationException("Image must be between 1 byte and 10 MB.");
        using (var source = File.OpenRead(sourcePath))
        using (var image = Image.FromStream(source))
        {
            if (image.Width < 1 || image.Height < 1) throw new InvalidOperationException("The selected image is invalid.");
        }
        if (!Directory.Exists(_directory)) throw new InvalidOperationException("The shared Web image directory is unavailable.");
        var name = "admin_" + Guid.NewGuid().ToString("N") + extension;
        var destination = Path.Combine(_directory, name);
        await using var input = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        try { await input.CopyToAsync(output, ct); }
        catch
        {
            output.Close();
            File.Delete(destination);
            throw;
        }
        return name;
    }

    internal void DeleteFailedUpload(string imageName)
    {
        if (!imageName.StartsWith("admin_", StringComparison.Ordinal) || Path.GetFileName(imageName) != imageName) return;
        var path = Path.Combine(_directory, imageName);
        if (File.Exists(path)) File.Delete(path);
    }
}
