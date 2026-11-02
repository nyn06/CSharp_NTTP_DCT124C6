using MetropolitanCoffee.Admin.Ui;
using MetropolitanCoffee.Services;
using System.Drawing.Text;

namespace MetropolitanCoffee.Admin.Forms;

public sealed class LoginForm : Form
{
    private readonly AdminAuthService _auth;
    private readonly AdminReadService _reader;
    private readonly TextBox _username = new() { Dock = DockStyle.Fill, Font = Theme.Font(11) };
    private readonly TextBox _password = new() { Dock = DockStyle.Fill, Font = Theme.Font(11), UseSystemPasswordChar = true };
    private readonly Label _error = Theme.Label("", 9, color: Color.Firebrick);
    private readonly Button _submit = Theme.Button("Đăng nhập", true);

    public LoginForm(AdminAuthService auth, AdminReadService reader)
    {
        _auth = auth;
        _reader = reader;
        Text = "Đăng nhập quản trị · Metropolitan Coffee";
        MinimumSize = new Size(850, 560);
        Size = new Size(1000, 640);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Cream;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Theme.Font();

        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Theme.Cream };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        Controls.Add(split);
        split.Controls.Add(new SlideshowPanel { Dock = DockStyle.Fill, Margin = Padding.Empty }, 0, 0);

        var center = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Cream, Margin = Padding.Empty };
        split.Controls.Add(center, 1, 0);
        const int fieldsHeight = 496;
        var fields = new TableLayoutPanel { ColumnCount = 1, RowCount = 10, Margin = Padding.Empty };
        foreach (var height in new[] { 150, 34, 54, 30, 42, 30, 42, 16, 48, 50 })
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        using var installedFonts = new InstalledFontCollection();
        var brandFamily = installedFonts.Families.Any(family =>
            family.Name.Equals("Playfair Display", StringComparison.OrdinalIgnoreCase)
            && family.IsStyleAvailable(FontStyle.Bold)) ? "Playfair Display" : "Georgia";
        var brandName = new Label
        {
            Text = "Metropolitan\nCoffee",
            Font = new Font(brandFamily, 40, FontStyle.Bold),
            ForeColor = Theme.Coffee
        };
        brandName.AutoSize = false;
        brandName.Dock = DockStyle.Fill;
        brandName.Margin = Padding.Empty;
        brandName.TextAlign = ContentAlignment.MiddleLeft;
        fields.Controls.Add(brandName);
        void FitBrandFont()
        {
            if (brandName.ClientSize.Width < 1 || brandName.ClientSize.Height < 1) return;
            for (var size = 40; size >= 24; size--)
            {
                using var candidate = new Font(brandFamily, size, FontStyle.Bold);
                var line = TextRenderer.MeasureText("Metropolitan", candidate, Size.Empty, TextFormatFlags.NoPadding);
                if (line.Width > brandName.ClientSize.Width - 8 || line.Height * 2 > brandName.ClientSize.Height - 12)
                    continue;
                if (Math.Abs(brandName.Font.SizeInPoints - size) > 0.1f)
                {
                    var previous = brandName.Font;
                    brandName.Font = (Font)candidate.Clone();
                    previous.Dispose();
                }
                break;
            }
        }
        brandName.Resize += (_, _) => FitBrandFont();
        fields.Controls.Add(Theme.Label("CHÀO MỪNG", 11, FontStyle.Bold, Theme.Accent));
        fields.Controls.Add(Theme.Label("Đăng nhập quản trị", 21, FontStyle.Bold));
        fields.Controls.Add(Theme.Label("Tên đăng nhập", 9, FontStyle.Bold));
        _username.AutoSize = false;
        _username.Margin = new Padding(0, 0, 0, 8);
        fields.Controls.Add(_username);
        fields.Controls.Add(Theme.Label("Mật khẩu", 9, FontStyle.Bold));
        _password.AutoSize = false;
        _password.Margin = new Padding(0, 0, 0, 8);
        fields.Controls.Add(_password);
        _submit.AutoSize = false;
        _submit.Margin = Padding.Empty;
        _submit.Dock = DockStyle.Fill;
        _submit.Click += async (_, _) => await SignInAsync();
        fields.Controls.Add(_submit, 0, 8);
        _error.AutoSize = false;
        _error.Margin = new Padding(0, 8, 0, 0);
        _error.Dock = DockStyle.Fill;
        fields.Controls.Add(_error, 0, 9);
        center.Controls.Add(fields);
        void CenterFields()
        {
            var width = Math.Max(1, Math.Min(440, center.ClientSize.Width - 64));
            fields.SetBounds((center.ClientSize.Width - width) / 2,
                (center.ClientSize.Height - fieldsHeight) / 2, width, fieldsHeight);
            FitBrandFont();
        }
        center.Resize += (_, _) => CenterFields();
        CenterFields();
        AcceptButton = _submit;
    }

    private async Task SignInAsync()
    {
        _submit.Enabled = false;
        _error.Text = "Đang xác thực...";
        try
        {
            var (user, error) = await _auth.LoginAsync(_username.Text, _password.Text);
            if (user is null) { _error.Text = error; return; }
            _error.Text = "";
            _password.Clear();
            Hide();
            using var main = new MainForm(user.Username, _reader);
            main.ShowDialog(this);
            Show();
            _username.Focus();
        }
        catch (Exception ex)
        {
            _error.Text = "Không thể kết nối SQL Server. Kiểm tra cấu hình và thử lại.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally { _submit.Enabled = true; }
    }
}
