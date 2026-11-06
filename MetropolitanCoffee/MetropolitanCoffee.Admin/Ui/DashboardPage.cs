using MetropolitanCoffee.Services;
using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Admin.Ui;

internal sealed class DashboardPage : UserControl
{
    private readonly AdminReadService _reader;
    private readonly Label _users = new() { Text = "—" };
    private readonly Label _products = new() { Text = "—" };
    private readonly Label _orders = new() { Text = "—" };
    private readonly Label _status = AdminTheme.Label("", 9, color: AdminTheme.Muted);
    private readonly Button _refresh = AdminTheme.Button("Refresh");
    private readonly System.Windows.Forms.Timer _syncTimer = new() { Interval = 5000 };
    private readonly CancellationTokenSource _lifetime = new();
    private AdminDashboardOrders? _lastOrders;
    private bool _loading;
    private bool _loaded;

    internal DashboardPage(AdminReadService reader, Action<string> navigate)
    {
        _reader = reader;
        Dock = DockStyle.Fill;
        BackColor = AdminTheme.Canvas;
        AutoScroll = true;

        var layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 444, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
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
        var toolbar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        _status.Dock = DockStyle.Fill;
        _status.AutoSize = false;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        _refresh.Click += async (_, _) => await LoadCountsAsync();
        toolbar.Controls.Add(_status, 0, 0);
        toolbar.Controls.Add(_refresh, 1, 0);
        layout.Controls.Add(toolbar, 0, 1);

        _syncTimer.Tick += async (_, _) => await PollOrdersAsync();
        Load += async (_, _) =>
        {
            await LoadCountsAsync();
            if (!IsDisposed) _syncTimer.Start();
        };
        VisibleChanged += async (_, _) =>
        {
            if (Visible && _loaded) await PollOrdersAsync();
        };
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

    private Task LoadCountsAsync() => RefreshAsync(full: true);

    private Task PollOrdersAsync()
    {
        if (!Visible || FindForm()?.WindowState == FormWindowState.Minimized) return Task.CompletedTask;
        return RefreshAsync(full: !_loaded);
    }

    private async Task RefreshAsync(bool full)
    {
        if (_loading || IsDisposed || _lifetime.IsCancellationRequested) return;
        _loading = true;
        _refresh.Enabled = false;
        if (full) SetStatus("Loading statistics...", false);
        try
        {
            AdminDashboardOrders orders;
            if (full)
            {
                var snapshot = await _reader.GetDashboardAsync(_lifetime.Token);
                if (IsDisposed || _lifetime.IsCancellationRequested) return;
                _users.Text = snapshot.Users.ToString("N0");
                _products.Text = snapshot.Products.ToString("N0");
                orders = snapshot.Orders;
            }
            else orders = await _reader.GetDashboardOrdersAsync(_lifetime.Token);
            if (IsDisposed || _lifetime.IsCancellationRequested) return;
            _orders.Text = orders.Count.ToString("N0");
            var message = _lastOrders is not null && orders.LatestId > _lastOrders.LatestId
                ? "A new order just arrived. Dashboard count updated automatically."
                : _lastOrders is not null && orders != _lastOrders
                    ? "Order changes were synced automatically."
                    : "Statistics up to date. Orders sync every 5 seconds.";
            _lastOrders = orders;
            _loaded = true;
            SetStatus(message, false);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (IsDisposed || _lifetime.IsCancellationRequested) return;
            SetStatus(_loaded
                ? "Unable to refresh statistics. Showing previous values. Please try Refresh."
                : "Unable to load statistics from SQL Server. Please try Refresh.", true);
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            _loading = false;
            if (!IsDisposed) _refresh.Enabled = true;
        }
    }

    private void SetStatus(string message, bool error)
    {
        _status.Text = message;
        _status.ForeColor = error ? Color.Firebrick : AdminTheme.Muted;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_lifetime.IsCancellationRequested)
        {
            _syncTimer.Stop();
            _syncTimer.Dispose();
            _lifetime.Cancel();
            _lifetime.Dispose();
        }
        base.Dispose(disposing);
    }
}
