using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

/// <summary>
/// C# WinForms counterpart of B04/admin/inventory.php.
/// Only the four original B04 sections; there is no StockMovements, reconciliation or stock editing here.
/// </summary>
internal sealed class B04InventoryPage : UserControl
{
    private readonly B04InventoryService _service;
    private readonly ProductImageStore _images = new();
    private readonly Dictionary<string, Bitmap> _imageCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly FlowLayoutPanel _sections = new()
    {
        Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown,
        WrapContents = false, Padding = new Padding(0, 0, 0, 8), BackColor = AdminTheme.Canvas
    };
    private readonly Label _available = MetricValue();
    private readonly Label _inStock = MetricValue();
    private readonly Label _runningLow = MetricValue();
    private readonly Label _outOfStock = MetricValue();
    private readonly Label _runningLowLabel = AdminTheme.Label("Running Low (<= 10)", 9, color: AdminTheme.Muted);
    private readonly DateTimePicker _snapshotDate = DateInput();
    private readonly ComboBox _snapshotCategory = new() { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _snapshotGrid = CreateGrid("Product", "Category", "Stock Quantity");
    private readonly Label _snapshotInfo = AdminTheme.Label("", 9, color: AdminTheme.Muted);
    private readonly DateTimePicker _from = DateInput();
    private readonly DateTimePicker _to = DateInput();
    private readonly TextBox _productSearch = new() { Width = 180, PlaceholderText = "Search product..." };
    private readonly ToolStripDropDown _productSuggestionsPopup = new() { AutoClose = true, Padding = Padding.Empty };
    private readonly ListBox _productSuggestions = new() { Width = 220, Height = 190, BorderStyle = BorderStyle.None };
    private IReadOnlyList<string> _productNames = [];
    private readonly DataGridView _reportGrid = CreateGrid("Product", "Category", "Total Imported", "Total Exported");
    private readonly Label _reportInfo = AdminTheme.Label("", 9, color: AdminTheme.Muted);
    private readonly NumericUpDown _threshold = new() { Width = 75, Minimum = 0, Maximum = 999, Value = 10 };
    private readonly DataGridView _alertGrid = CreateGrid("Product", "Category", "Current Stock", "Status");
    private readonly Label _alertInfo = AdminTheme.Label("", 9, color: AdminTheme.Muted);
    private readonly Button _set = AdminTheme.Button("Set");
    private bool _busy;

    internal B04InventoryPage(B04InventoryService service)
    {
        _service = service;
        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();
        Controls.Add(_sections);
        BuildSections();
        _sections.Resize += (_, _) => SizeSections();
        Load += async (_, _) => await InitializeAsync();
        Disposed += (_, _) =>
        {
            _productSuggestionsPopup.Dispose();
            foreach (var image in _imageCache.Values) image.Dispose();
            _imageCache.Clear();
        };
    }

    private static DateTimePicker DateInput() => new()
    {
        Format = DateTimePickerFormat.Custom, CustomFormat = "MM/dd/yyyy", Width = 135,
        ShowCheckBox = true, MaxDate = DateTime.Today, Value = DateTime.Today, Checked = false
    };

    private static Label MetricValue() => new()
    {
        Text = "–", AutoSize = false, Dock = DockStyle.Top, Height = 42,
        TextAlign = ContentAlignment.MiddleCenter, Font = AdminTheme.Font(21, FontStyle.Bold)
    };

    private static DataGridView CreateGrid(params string[] headers)
    {
        var grid = AdminTheme.Grid();
        grid.AutoGenerateColumns = false;
        grid.RowTemplate.Height = 49;
        grid.ColumnHeadersHeight = 42;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        foreach (var heading in headers)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = heading, HeaderText = heading, SortMode = DataGridViewColumnSortMode.NotSortable,
                FillWeight = heading == "Product" ? 175 : 100
            };
            grid.Columns.Add(col);
        }
        return grid;
    }

    private void HookImagePainting(DataGridView grid)
    {
        grid.CellPainting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0) return;
            var graphics = e.Graphics;
            if (graphics is null) return;
            var row = grid.Rows[e.RowIndex];
            if (row.Tag is not ImageKey key) return;
            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
            var x = e.CellBounds.Left + 7;
            var y = e.CellBounds.Top + (e.CellBounds.Height - 34) / 2;
            var hasImage = !string.IsNullOrWhiteSpace(key.File);
            if (hasImage)
            {
                var image = GetImage(key);
                if (image is not null) graphics.DrawImage(image, new Rectangle(x, y, 34, 34));
            }
            var textLeft = x + (hasImage ? 43 : 0);
            var textRect = new Rectangle(textLeft, e.CellBounds.Top + 1,
                Math.Max(8, e.CellBounds.Right - textLeft - 5), e.CellBounds.Height - 2);
            TextRenderer.DrawText(graphics, Convert.ToString(row.Cells[0].Value) ?? "", grid.Font,
                textRect, row.DefaultCellStyle.ForeColor.IsEmpty ? AdminTheme.Text : row.DefaultCellStyle.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            e.Handled = true;
        };
    }

    private Bitmap? GetImage(ImageKey key)
    {
        if (string.IsNullOrWhiteSpace(key.File)) return null;
        var cacheKey = $"{key.ProductId}:{key.File}";
        if (_imageCache.TryGetValue(cacheKey, out var existing)) return existing;
        var bitmap = _images.Load(key.File, key.ProductId); // read-only access to existing Web images
        _imageCache[cacheKey] = bitmap;
        return bitmap;
    }

    private void BuildSections()
    {
        var overview = WhiteSection(132);
        var metrics = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Margin = Padding.Empty };
        for (var i = 0; i < 4; i++) metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        metrics.Controls.Add(Metric(_available, "Available Products", Color.FromArgb(248, 249, 250), AdminTheme.Text), 0, 0);
        metrics.Controls.Add(Metric(_inStock, "Items in Stock", Color.FromArgb(232, 245, 233), Color.FromArgb(25, 135, 84)), 1, 0);
        metrics.Controls.Add(Metric(_runningLow, _runningLowLabel, Color.FromArgb(255, 243, 224), Color.FromArgb(153, 102, 0)), 2, 0);
        metrics.Controls.Add(Metric(_outOfStock, "Out of Stock", Color.FromArgb(252, 228, 236), Color.FromArgb(220, 53, 69)), 3, 0);
        overview.Controls.Add(metrics);
        _sections.Controls.Add(overview);

        var snapshot = SectionWithGrid("Stock on a Selected Date", _snapshotGrid, _snapshotInfo, 370, out var snapshotFilters);
        snapshotFilters.Controls.Add(Captioned("Select Date *", _snapshotDate));
        _snapshotCategory.Items.Add("All Categories");
        _snapshotCategory.SelectedIndex = 0;
        snapshotFilters.Controls.Add(Captioned("Category", _snapshotCategory));
        var checkStock = AdminTheme.Button("Check Stock", true);
        checkStock.Click += async (_, _) => await CheckStockAsync(checkStock);
        snapshotFilters.Controls.Add(checkStock);
        var resetSnapshot = AdminTheme.Button("Reset");
        resetSnapshot.Click += (_, _) =>
        {
            _snapshotDate.Checked = false;
            _snapshotCategory.SelectedIndex = 0;
            _snapshotGrid.Rows.Clear();
            _snapshotInfo.Text = "";
        };
        snapshotFilters.Controls.Add(resetSnapshot);
        _sections.Controls.Add(snapshot);

        var report = SectionWithGrid("Stock In / Out Report", _reportGrid, _reportInfo, 385, out var reportFilters);
        reportFilters.Controls.Add(Captioned("From *", _from));
        reportFilters.Controls.Add(Captioned("To *", _to));
        // B04/search_products.php shows up to 10 product names containing typed text.
        _productSuggestionsPopup.Items.Add(new ToolStripControlHost(_productSuggestions)
        { AutoSize = false, Size = new Size(_productSuggestions.Width, _productSuggestions.Height), Margin = Padding.Empty, Padding = Padding.Empty });
        _productSearch.TextChanged += (_, _) => ShowProductSuggestions();
        _productSuggestions.MouseClick += (_, _) =>
        {
            if (_productSuggestions.SelectedItem is not string selected) return;
            _productSuggestionsPopup.Close();
            _productSearch.Text = selected;
        };
        _productSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Down || !_productSuggestionsPopup.Visible) return;
            _productSuggestions.Focus();
            if (_productSuggestions.Items.Count > 0) _productSuggestions.SelectedIndex = 0;
            e.Handled = true;
        };
        _productSuggestions.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter || _productSuggestions.SelectedItem is not string selected) return;
            _productSuggestionsPopup.Close();
            _productSearch.Text = selected;
            e.Handled = true;
        };
        reportFilters.Controls.Add(Captioned("Product Name", _productSearch));
        var showReport = AdminTheme.Button("Show Report", true);
        showReport.Click += async (_, _) => await ShowReportAsync(showReport);
        reportFilters.Controls.Add(showReport);
        var resetReport = AdminTheme.Button("Reset");
        resetReport.Click += (_, _) =>
        {
            _from.Checked = false;
            _to.Checked = false;
            _productSuggestionsPopup.Close();
            _productSearch.Clear();
            _reportGrid.Rows.Clear();
            _reportInfo.Text = "";
        };
        reportFilters.Controls.Add(resetReport);
        _sections.Controls.Add(report);

        var alerts = SectionWithGrid("Low Stock Alert", _alertGrid, _alertInfo, 360, out var thresholdControls);
        thresholdControls.Controls.Add(new Label
        {
            Text = "Alert when stock <=", TextAlign = ContentAlignment.MiddleLeft, AutoSize = true,
            Margin = new Padding(0, 12, 8, 0), ForeColor = AdminTheme.Muted
        });
        thresholdControls.Controls.Add(_threshold);
        _set.BackColor = Color.FromArgb(255, 193, 7);
        _set.ForeColor = AdminTheme.Text;
        _set.FlatAppearance.BorderColor = Color.FromArgb(255, 193, 7);
        _set.Click += async (_, _) => await RefreshSummaryAndAlertsAsync();
        thresholdControls.Controls.Add(_set);
        _sections.Controls.Add(alerts);

        foreach (var grid in new[] { _snapshotGrid, _reportGrid, _alertGrid }) HookImagePainting(grid);
        SizeSections();
    }

    private static Panel WhiteSection(int height) => new()
    {
        Height = height, BackColor = Color.White, Padding = new Padding(18), Margin = new Padding(0, 0, 0, 14)
    };

    private static Panel Metric(Label value, string title, Color background, Color numberColor)
        => Metric(value, AdminTheme.Label(title, 9, color: AdminTheme.Muted), background, numberColor);

    private static Panel Metric(Label value, Label title, Color background, Color numberColor)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = background, Margin = new Padding(5) };
        value.ForeColor = numberColor;
        title.AutoSize = false;
        title.Dock = DockStyle.Top;
        title.Height = 28;
        title.TextAlign = ContentAlignment.MiddleCenter;
        card.Controls.Add(title);
        card.Controls.Add(value);
        return card;
    }

    private static Panel SectionWithGrid(string title, DataGridView grid, Label status, int height, out FlowLayoutPanel filters)
    {
        var section = WhiteSection(height);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 79));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        section.Controls.Add(layout);
        var heading = AdminTheme.Label(title, 13, FontStyle.Bold);
        heading.Dock = DockStyle.Fill;
        heading.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(heading, 0, 0);
        filters = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, BackColor = Color.White, Padding = new Padding(0, 4, 0, 4) };
        layout.Controls.Add(filters, 0, 1);
        layout.Controls.Add(grid, 0, 2);
        status.Dock = DockStyle.Fill;
        status.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(status, 0, 3);
        return section;
    }

    private static Control Captioned(string title, Control field)
    {
        var wrapper = new Panel { Width = field.Width + 6, Height = 58, Margin = new Padding(0, 0, 12, 0) };
        var label = AdminTheme.Label(title, 8.5f);
        label.Location = new Point(0, 0);
        field.Location = new Point(0, 23);
        wrapper.Controls.Add(label);
        wrapper.Controls.Add(field);
        return wrapper;
    }

    private void ShowProductSuggestions()
    {
        _productSuggestionsPopup.Close();
        var term = _productSearch.Text.Trim();
        if (term.Length == 0 || !_productSearch.Focused) return;
        var matches = _productNames.Where(name => name.Contains(term, StringComparison.OrdinalIgnoreCase)).Take(10).ToArray();
        if (matches.Length == 0) return;
        _productSuggestions.Items.Clear();
        _productSuggestions.Items.AddRange(matches);
        _productSuggestionsPopup.Show(_productSearch, new Point(0, _productSearch.Height));
    }

    private void SizeSections()
    {
        var width = Math.Max(450, _sections.ClientSize.Width - 24);
        foreach (Control control in _sections.Controls) control.Width = width;
    }

    private async Task InitializeAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            var categories = await _service.GetCategoriesAsync();
            if (IsDisposed) return;
            foreach (var category in categories) _snapshotCategory.Items.Add(category);
            var names = await _service.GetProductNamesAsync();
            if (IsDisposed) return;
            _productNames = names;
        }
        catch (Exception ex)
        {
            if (!IsDisposed) _alertInfo.Text = "Unable to load data from SQL Server. Check the connection.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally { _busy = false; }
        if (!IsDisposed) await RefreshSummaryAndAlertsAsync();
    }

    private async Task RefreshSummaryAndAlertsAsync()
    {
        if (_busy) return;
        _busy = true;
        _set.Enabled = false;
        try
        {
            var limit = (int)_threshold.Value;
            var summary = await _service.GetOverviewAsync(limit);
            var rows = await _service.GetLowStockAsync(limit);
            if (IsDisposed) return;
            _available.Text = summary.AvailableProducts.ToString("N0");
            _inStock.Text = summary.ItemsInStock.ToString("N0");
            _runningLow.Text = summary.RunningLow.ToString("N0");
            _outOfStock.Text = summary.OutOfStock.ToString("N0");
            _runningLowLabel.Text = $"Running Low (<= {limit})";
            _alertGrid.Rows.Clear();
            foreach (var item in rows)
            {
                var index = _alertGrid.Rows.Add(item.Product, item.Category, $"{item.CurrentStock} units",
                    item.CurrentStock == 0 ? "Out of Stock" : "Low Stock");
                var gridRow = _alertGrid.Rows[index];
                gridRow.Tag = new ImageKey(item.ProductId, item.Image);
                gridRow.DefaultCellStyle.BackColor = item.CurrentStock == 0
                    ? Color.FromArgb(248, 215, 218) : Color.FromArgb(255, 243, 205);
            }
            _alertInfo.Text = rows.Count == 0 ? $"No products are running low ({limit} units)."
                : $"{rows.Count} product(s) are at or below {limit} units!";
        }
        catch (Exception ex)
        {
            if (!IsDisposed) _alertInfo.Text = "Could not load low stock data.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally { _set.Enabled = true; _busy = false; }
    }

    private async Task CheckStockAsync(Button button)
    {
        if (_busy) return;
        if (!_snapshotDate.Checked)
        {
            MessageBox.Show(this, "Please select a date.", "Metropolitan Coffee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _busy = true;
        button.Enabled = false;
        try
        {
            var category = _snapshotCategory.SelectedIndex > 0 ? _snapshotCategory.Text : null;
            var rows = await _service.GetSnapshotAsync(_snapshotDate.Value.Date, category);
            if (IsDisposed) return;
            _snapshotGrid.Rows.Clear();
            string? lastCategory = null;
            foreach (var item in rows)
            {
                if (lastCategory != item.Category)
                {
                    lastCategory = item.Category;
                    var groupIndex = _snapshotGrid.Rows.Add(item.Category, "", "");
                    _snapshotGrid.Rows[groupIndex].DefaultCellStyle.BackColor = Color.FromArgb(226, 227, 229);
                    _snapshotGrid.Rows[groupIndex].DefaultCellStyle.Font = AdminTheme.Font(9, FontStyle.Bold);
                }
                var index = _snapshotGrid.Rows.Add(item.Product, item.Category, item.StockQuantity);
                var gridRow = _snapshotGrid.Rows[index];
                gridRow.Tag = new ImageKey(item.ProductId, item.Image);
                gridRow.Cells[2].Style.BackColor = item.StockQuantity <= 0 ? Color.FromArgb(220, 53, 69)
                    : item.StockQuantity <= _threshold.Value ? Color.FromArgb(255, 193, 7)
                    : Color.FromArgb(25, 135, 84);
                gridRow.Cells[2].Style.ForeColor = item.StockQuantity <= 0 || item.StockQuantity > _threshold.Value
                    ? Color.White : AdminTheme.Text;
            }
            _snapshotInfo.Text = rows.Count == 0 ? "No data found."
                : $"Stock as of {_snapshotDate.Value:dd/MM/yyyy}";
        }
        catch (Exception ex)
        {
            if (!IsDisposed) _snapshotInfo.Text = "Could not load stock data.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally { button.Enabled = true; _busy = false; }
    }

    private async Task ShowReportAsync(Button button)
    {
        if (_busy) return;
        if (!_from.Checked || !_to.Checked)
        {
            MessageBox.Show(this, !_from.Checked ? "Please select From date." : "Please select To date.",
                "Metropolitan Coffee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_from.Value.Date > _to.Value.Date)
        {
            MessageBox.Show(this, "From date must be earlier than or equal to To date.",
                "Metropolitan Coffee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _busy = true;
        button.Enabled = false;
        try
        {
            var rows = await _service.GetReportAsync(_from.Value.Date, _to.Value.Date, _productSearch.Text);
            if (IsDisposed) return;
            _reportGrid.Rows.Clear();
            foreach (var item in rows)
            {
                var index = _reportGrid.Rows.Add(item.Product, item.Category, item.TotalImported, item.TotalExported);
                var row = _reportGrid.Rows[index];
                row.Tag = new ImageKey(item.ProductId, item.Image);
                row.Cells[2].Style.ForeColor = Color.FromArgb(25, 135, 84);
                row.Cells[3].Style.ForeColor = Color.FromArgb(220, 53, 69);
            }
            _reportInfo.Text = rows.Count == 0 ? "No data found for this period." : "";
        }
        catch (Exception ex)
        {
            if (!IsDisposed) _reportInfo.Text = "Could not load report data.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally { button.Enabled = true; _busy = false; }
    }

    private sealed record ImageKey(int ProductId, string? File);
}
