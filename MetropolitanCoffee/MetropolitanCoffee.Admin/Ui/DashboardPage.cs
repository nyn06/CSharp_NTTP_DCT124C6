using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

internal sealed class DashboardPage : UserControl
{
    private readonly AdminReadService _reader;
    private readonly Label _users = new() { Text = "—" };
    private readonly Label _products = new() { Text = "—" };
    private readonly Label _orders = new() { Text = "—" };
    private readonly Label _status = AdminTheme.Label("", 9, color: AdminTheme.Muted);

    internal DashboardPage(AdminReadService reader, Action<string> navigate)
    {
        _reader = reader;
        Dock = DockStyle.Fill;
        BackColor = AdminTheme.Canvas;
        AutoScroll = true;

        var layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 420, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 308));
        Controls.Add(layout);
        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Margin = Padding.Empty };
        for (var i = 0; i < 3; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        cards.Controls.Add(CreateCard("Users", "\uE77B", Color.FromArgb(13, 110, 253), _users, () => navigate("users")), 0, 0);
        cards.Controls.Add(CreateCard("Products", "\uE719", Color.FromArgb(25, 135, 84), _products, () => navigate("products")), 1, 0);
        cards.Controls.Add(CreateCard("Orders", "\uE7BF", Color.FromArgb(220, 53, 69), _orders, () => navigate("orders")), 2, 0);
        cards.Controls.Add(CreateCard("Purchase Orders", "\uE896", Color.FromArgb(255, 193, 7), "Manage purchase orders", () => navigate("purchases")), 0, 1);
        cards.Controls.Add(CreateCard("Pricing", "\uE8EC", Color.FromArgb(13, 202, 240), "Manage pricing rules", () => navigate("pricing")), 1, 1);
        cards.Controls.Add(CreateCard("Inventory", "\uE8B7", Color.FromArgb(108, 117, 125), "Manage stock & inventory", () => navigate("stock")), 2, 1);
        layout.Controls.Add(cards, 0, 2);

        var welcome = new Panel
        {
            Dock = DockStyle.Fill, BackColor = Color.White,
            Padding = new Padding(20, 14, 20, 8), Margin = Padding.Empty
        };
        var subtitle = AdminTheme.Label("Manage your coffee empire: products, users, orders & reports.", 10, color: AdminTheme.Muted);
        subtitle.Dock = DockStyle.Bottom;
        subtitle.Height = 28;
        subtitle.AutoSize = false;
        var title = AdminTheme.Label("Metropolitan Coffee Admin", 14, FontStyle.Bold);
        title.Dock = DockStyle.Top;
        title.Height = 30;
        title.AutoSize = false;
        welcome.Controls.Add(subtitle);
        welcome.Controls.Add(title);
        layout.Controls.Add(welcome, 0, 0);
        _status.Dock = DockStyle.Fill;
        _status.AutoSize = false;
        _status.Visible = false;
        layout.Controls.Add(_status, 0, 1);

        Load += async (_, _) => await LoadCountsAsync();
    }

    private static Panel CreateCard(string title, string iconGlyph, Color iconColor, object value, Action navigate)
    {
        var card = new Panel
        {
            BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill, Margin = new Padding(8), Padding = new Padding(17),
            Cursor = Cursors.Hand
        };
        var heading = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.White };
        var icon = new Label
        {
            Text = iconGlyph, Font = new Font("Segoe MDL2 Assets", 21, GraphicsUnit.Pixel),
            ForeColor = iconColor, Width = 34, Dock = DockStyle.Left,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var label = AdminTheme.Label(title, 11, FontStyle.Bold);
        label.Dock = DockStyle.Fill;
        label.TextAlign = ContentAlignment.MiddleLeft;
        heading.Controls.Add(label);
        heading.Controls.Add(icon);
        card.Controls.Add(heading);

        var valueLabel = value as Label ?? AdminTheme.Label((string)value, 10, color: AdminTheme.Muted);
        valueLabel.AutoSize = false;
        valueLabel.Dock = DockStyle.Fill;
        valueLabel.Font = value is Label ? AdminTheme.Font(25, FontStyle.Bold) : AdminTheme.Font(10);
        valueLabel.ForeColor = value is Label ? AdminTheme.Text : AdminTheme.Muted;
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;
        card.Controls.Add(valueLabel);
        card.Controls.SetChildIndex(heading, 0);

        void Attach(Control control)
        {
            control.Cursor = Cursors.Hand;
            control.Click += (_, _) => navigate();
            foreach (Control child in control.Controls) Attach(child);
        }
        Attach(card);
        return card;
    }

    private async Task LoadCountsAsync()
    {
        try
        {
            var counts = await _reader.GetDashboardCountsAsync();
            if (IsDisposed) return;
            _users.Text = counts.Users.ToString("N0");
            _products.Text = counts.Products.ToString("N0");
            _orders.Text = counts.Orders.ToString("N0");
            _status.Text = "";
            _status.Visible = false;
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            _status.Text = "Unable to load statistics from SQL Server.";
            _status.Visible = true;
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
