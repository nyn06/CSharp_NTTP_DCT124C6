using MetropolitanCoffee.Admin.Customers;
using MetropolitanCoffee.Admin.Ui;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Forms;

public sealed class MainForm : Form
{
    private readonly AdminReadService _reader;
    private readonly Panel _pageHost = new() { Dock = DockStyle.Fill, BackColor = AdminTheme.Canvas, Padding = new Padding(30) };
    private readonly Label _heading = AdminTheme.Label("Dashboard", 17, FontStyle.Bold);
    private readonly Dictionary<string, AdminNavButton> _nav = [];
    private readonly TableLayoutPanel _sidebar = new() { Dock = DockStyle.Left, Width = 250, BackColor = AdminTheme.Chrome };

    public MainForm(string username, AdminReadService reader)
    {
        _reader = reader;
        Text = "Metropolitan Coffee Admin";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1040, 670);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = AdminTheme.Font();
        BackColor = AdminTheme.Canvas;

        var contentFrame = new Panel { Dock = DockStyle.Fill, BackColor = AdminTheme.Canvas };
        Controls.Add(contentFrame);
        Controls.Add(_sidebar);
        contentFrame.Controls.Add(_pageHost);
        var pageTopbar = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.White, Padding = new Padding(25, 14, 20, 10) };
        pageTopbar.Paint += (_, e) =>
        {
            using var pen = new Pen(AdminTheme.Border);
            e.Graphics.DrawLine(pen, 0, pageTopbar.Height - 1, pageTopbar.Width, pageTopbar.Height - 1);
        };
        contentFrame.Controls.Add(pageTopbar);
        _heading.Dock = DockStyle.Fill;
        _heading.TextAlign = ContentAlignment.MiddleLeft;
        _heading.Margin = Padding.Empty;
        pageTopbar.Controls.Add(_heading);

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top, Height = 64, BackColor = AdminTheme.Chrome,
            ColumnCount = 4, RowCount = 1, Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 235));
        Controls.Add(header);

        var toggle = new Button
        {
            Text = "☰", Dock = DockStyle.Fill, Margin = Padding.Empty,
            Font = AdminTheme.Font(18), ForeColor = Color.White,
            BackColor = AdminTheme.Chrome, FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand, AccessibleName = "Toggle sidebar"
        };
        toggle.FlatAppearance.BorderSize = 0;
        toggle.Click += (_, _) => _sidebar.Visible = !_sidebar.Visible;
        header.Controls.Add(toggle, 0, 0);

        var logo = new PictureBox { Dock = DockStyle.Fill, Margin = new Padding(4, 8, 4, 8), SizeMode = PictureBoxSizeMode.Zoom };
        var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Admin", "MetropolitanCafeLogo.png");
        if (File.Exists(logoPath))
        {
            using var image = Image.FromFile(logoPath);
            logo.Image = new Bitmap(image);
            Disposed += (_, _) => logo.Image?.Dispose();
        }
        header.Controls.Add(logo, 1, 0);

        var brand = new Label
        {
            Text = "Metropolitan Coffee Admin", Dock = DockStyle.Fill, AutoSize = false,
            Margin = Padding.Empty, TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Georgia", 16, FontStyle.Bold | FontStyle.Italic),
            ForeColor = Color.White, AutoEllipsis = true
        };
        header.Controls.Add(brand, 2, 0);

        void ConfirmLogout()
        {
            if (MessageBox.Show(this, "Are you sure you want to log out?", "Log out",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Close();
        }
        var account = new Button
        {
            Text = "Welcome, " + username + "  ▾", Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 10, 0), TextAlign = ContentAlignment.MiddleRight,
            Font = AdminTheme.Font(10, FontStyle.Bold), ForeColor = Color.White,
            BackColor = AdminTheme.Chrome, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            AutoEllipsis = true
        };
        account.FlatAppearance.BorderSize = 0;
        var accountMenu = new ContextMenuStrip();
        accountMenu.Items.Add(new ToolStripMenuItem("Profile") { Enabled = false });
        accountMenu.Items.Add(new ToolStripSeparator());
        accountMenu.Items.Add("Logout", null, (_, _) => ConfirmLogout());
        account.Click += (_, _) => accountMenu.Show(account, new Point(0, account.Height));
        Disposed += (_, _) => accountMenu.Dispose();
        header.Controls.Add(account, 3, 0);

        _sidebar.ColumnCount = 1;
        _sidebar.RowCount = 2;
        _sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        var navPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, AutoScroll = true, Margin = Padding.Empty,
            BackColor = AdminTheme.Chrome
        };
        _sidebar.Controls.Add(navPanel, 0, 0);
        foreach (var (key, title, icon) in new[]
        {
            ("dashboard", "Dashboard", "\uE80F"), ("users", "Users", "\uE77B"),
            ("products", "Products", "\uE719"), ("categories", "Categories", "\uE8EF"),
            ("purchases", "Purchase Orders", "\uE896"), ("pricing", "Pricing", "\uE8EC"),
            ("orders", "Orders", "\uE7BF"), ("stock", "Inventory", "\uE8B7")
        })
        {
            var button = new AdminNavButton { Text = title, IconGlyph = icon };
            button.Click += (_, _) => Navigate(key);
            button.MouseEnter += (_, _) => { if (!button.Selected) button.BackColor = AdminTheme.ChromeHover; };
            button.MouseLeave += (_, _) => { if (!button.Selected) button.BackColor = AdminTheme.Chrome; };
            navPanel.Controls.Add(button);
            _nav.Add(key, button);
        }
        void FitNavigationButtons()
        {
            var width = navPanel.ClientSize.Width - (navPanel.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0);
            foreach (var button in _nav.Values) button.Width = Math.Max(0, width);
        }
        navPanel.Resize += (_, _) => FitNavigationButtons();
        FitNavigationButtons();

        var logout = new AdminNavButton { Text = "Logout", IconGlyph = "\uE8AC", Dock = DockStyle.Fill };
        logout.Click += (_, _) => ConfirmLogout();
        _sidebar.Controls.Add(logout, 0, 1);
        Navigate("dashboard");
    }

    private void Navigate(string key)
    {
        _heading.Text = key == "stock"
            ? "Inventory Management"
            : key == "orders" ? "Administration"
            : _nav[key].Text;
        foreach (var (name, button) in _nav)
        {
            button.Selected = name == key;
            button.BackColor = name == key ? AdminTheme.ChromeHover : AdminTheme.Chrome;
            button.Invalidate();
        }
        var previous = _pageHost.Controls.Cast<Control>().ToArray();
        _pageHost.Controls.Clear();
        foreach (var control in previous) control.Dispose();
        Control page = key == "dashboard"
            ? new DashboardPage(_reader, Navigate)
            : key == "users" ? new CustomersPage(Program.ConnectionString)
            : key == "stock" ? new B04InventoryPage(Program.Inventory)
            : key == "purchases" ? new PurchasesPage(Program.Purchases)
            : key == "products" ? new ProductsPage(Program.Catalog)
            : key == "categories" ? new CategoriesPage(Program.Catalog)
            : key == "pricing" ? new PricingPage(Program.Catalog)
            : key == "orders" ? new OrdersPage(_reader)
            : new AdminPage(key, _reader);
        page.Dock = DockStyle.Fill;
        _pageHost.Controls.Add(page);
    }
}
