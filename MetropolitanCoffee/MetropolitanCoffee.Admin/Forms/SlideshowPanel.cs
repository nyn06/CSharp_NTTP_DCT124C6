using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace MetropolitanCoffee.Admin.Forms;

internal sealed class SlideshowPanel : Panel
{
    private readonly Image[] _images;
    private const int HoldMilliseconds = 3600;
    private const int FadeMilliseconds = 750;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 33 };
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private int _current;

    internal SlideshowPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.Black;

        var imageDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "LoginSlideshow");
        _images = Enumerable.Range(1, 3)
            .Select(number => LoadImage(Path.Combine(imageDirectory, $"bg{number}.jpg")))
            .Where(image => image is not null)
            .Cast<Image>()
            .ToArray();

        _timer.Tick += (_, _) =>
        {
            if (_images.Length < 2) return;
            if (_clock.ElapsedMilliseconds >= HoldMilliseconds + FadeMilliseconds)
            {
                _current = (_current + 1) % _images.Length;
                _clock.Restart();
            }
            Invalidate();
        };
        if (_images.Length > 1) _timer.Start();
    }

    private static Image? LoadImage(string path)
    {
        if (!File.Exists(path))
        {
            System.Diagnostics.Debug.WriteLine($"Login slideshow image missing: {path}");
            return null;
        }

        using var stream = File.OpenRead(path);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_images.Length == 0 || ClientSize.Width == 0 || ClientSize.Height == 0) return;

        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        DrawCover(e.Graphics, _images[_current], 1);

        if (_images.Length < 2 || _clock.ElapsedMilliseconds < HoldMilliseconds) return;
        var progress = Math.Clamp((float)(_clock.ElapsedMilliseconds - HoldMilliseconds) / FadeMilliseconds, 0, 1);
        var eased = progress * progress * (3 - 2 * progress);
        DrawCover(e.Graphics, _images[(_current + 1) % _images.Length], eased);
    }

    private void DrawCover(Graphics graphics, Image image, float opacity)
    {
        var scale = Math.Max((float)ClientSize.Width / image.Width, (float)ClientSize.Height / image.Height);
        var width = image.Width * scale;
        var height = image.Height * scale;
        var destination = new RectangleF((ClientSize.Width - width) / 2, (ClientSize.Height - height) / 2, width, height);
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = opacity });
        graphics.DrawImage(image, Rectangle.Round(destination), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
            foreach (var image in _images) image.Dispose();
        }
        base.Dispose(disposing);
    }
}
