using MetropolitanCoffee.Admin.Forms;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

internal sealed class PricingPage : UserControl
{
    private readonly AdminCatalogService _catalog;
    private readonly DataGridView _grid = AdminTheme.Grid();
    private readonly TextBox _search = new() { Width = 230, PlaceholderText = "Search products..." };
    private readonly ComboBox _filter = new() { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _status = AdminTheme.Label("", 9, color: AdminTheme.Muted);
    private readonly Label _count = AdminTheme.Label("—", 17, FontStyle.Bold);
    private readonly Label _average = AdminTheme.Label("—", 17, FontStyle.Bold);
    private readonly Label _highest = AdminTheme.Label("—", 17, FontStyle.Bold);
    private readonly Label _lowest = AdminTheme.Label("—", 17, FontStyle.Bold);
    private IReadOnlyList<AdminProduct> _products = [];
    private bool _refreshing;

    internal PricingPage(AdminCatalogService catalog)
    {
        _catalog = catalog;
        Dock = DockStyle.Fill;
        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, Padding = new Padding(4) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);
        var stats = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
        for (var i = 0; i < 4; i++) stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        stats.Controls.Add(Stat("Products Shown", _count, Color.FromArgb(248, 249, 250)), 0, 0);
        stats.Controls.Add(Stat("Average Profit Margin", _average, Color.FromArgb(227, 242, 253)), 1, 0);
        stats.Controls.Add(Stat("Highest Sell Price", _highest, Color.FromArgb(232, 245, 233)), 2, 0);
        stats.Controls.Add(Stat("Lowest Sell Price", _lowest, Color.FromArgb(255, 243, 224)), 3, 0);
        root.Controls.Add(stats, 0, 0);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true };
        root.Controls.Add(actions, 0, 1);
        _search.AutoCompleteMode = AutoCompleteMode.Suggest;
        _search.AutoCompleteSource = AutoCompleteSource.CustomSource;
        _search.TextChanged += (_, _) => { UpdateSuggestions(); Render(); };
        actions.Controls.Add(_search);
        _filter.Items.Add("All Categories");
        _filter.SelectedIndex = 0;
        _filter.SelectedIndexChanged += (_, _) => { UpdateSuggestions(); Render(); };
        actions.Controls.Add(_filter);
        var search = AdminTheme.Button("Search");
        search.Click += (_, _) =>
        {
            Render();
            if (_grid.Rows.Count > 0) _status.Text = $"Search completed successfully! {_grid.Rows.Count:N0} products";
        };
        actions.Controls.Add(search);
        var reset = AdminTheme.Button("Reset");
        reset.Click += (_, _) => { _search.Clear(); _filter.SelectedIndex = 0; Render(); };
        actions.Controls.Add(reset);
        var refresh = AdminTheme.Button("Refresh");
        refresh.Click += async (_, _) => await RefreshAsync();
        actions.Controls.Add(refresh);
        var card = AdminTheme.Card();
        card.Dock = DockStyle.Fill;
        card.Margin = Padding.Empty;
        root.Controls.Add(card, 0, 2);
        var heading = AdminTheme.Label("Pricing Management", 13, FontStyle.Bold);
        heading.Dock = DockStyle.Top;
        heading.AutoSize = false;
        heading.Height = 35;
        card.Controls.Add(_grid);
        card.Controls.Add(heading);
        _status.Dock = DockStyle.Bottom;
        _status.AutoSize = false;
        _status.Height = 24;
        card.Controls.Add(_status);
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.ScrollBars = ScrollBars.Both;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Product ID", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Product", HeaderText = "Product", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", Width = 165 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cost", HeaderText = "Cost Price", Width = 115 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Profit", HeaderText = "Profit %", Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sell", HeaderText = "Sell Price", Width = 115 });
        _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Actions", Text = "Edit Profit", UseColumnTextForButtonValue = true, Width = 115 });
        _grid.CellContentClick += async (_, e) =>
        {
            if (_refreshing || e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Edit" || _grid.Rows[e.RowIndex].Tag is not AdminProduct product) return;
            try
            {
                var current = await _catalog.GetProductAsync(product.Id);
                if (IsDisposed) return;
                if (current is null) { await RefreshAsync(); _status.Text = "Product not found. Pricing refreshed."; return; }
                using var dialog = new PricingEditorForm(_catalog, current);
                if (dialog.ShowDialog(this) == DialogResult.OK) await RefreshAsync();
            }
            catch (Exception ex) { if (!IsDisposed) _status.Text = "Unable to load product pricing. Please refresh and try again."; System.Diagnostics.Debug.WriteLine(ex); }
        };
    }

    private static Panel Stat(string title, Label value, Color background)
    {
        var panel = AdminTheme.Card();
        panel.Dock = DockStyle.Fill;
        panel.Margin = new Padding(4, 4, 8, 8);
        panel.BorderStyle = BorderStyle.FixedSingle;
        panel.BackColor = background;
        value.BackColor = background;
        var heading = AdminTheme.Label(title, 9, color: AdminTheme.Muted);
        heading.BackColor = background;
        heading.Dock = DockStyle.Top;
        heading.AutoSize = false;
        heading.Height = 25;
        value.Dock = DockStyle.Fill;
        value.AutoSize = false;
        panel.Controls.Add(value);
        panel.Controls.Add(heading);
        return panel;
    }

    private async Task RefreshAsync()
    {
        if (_refreshing) return;
        _refreshing = true;
        _status.Text = "Loading pricing...";
        try
        {
            var categories = await _catalog.GetCategoriesAsync();
            var products = await _catalog.GetProductsAsync();
            var summary = await _catalog.GetPricingSummaryAsync();
            if (IsDisposed) return;
            _products = products;
            var selected = (_filter.SelectedItem as AdminCategory)?.Id;
            _filter.BeginUpdate();
            _filter.Items.Clear();
            _filter.Items.Add("All Categories");
            foreach (var category in categories) _filter.Items.Add(category);
            _filter.DisplayMember = nameof(AdminCategory.Name);
            _filter.SelectedItem = _filter.Items.OfType<AdminCategory>().FirstOrDefault(c => c.Id == selected) ?? _filter.Items[0];
            _filter.EndUpdate();
            _count.Text = _products.Count.ToString("N0");
            _average.Text = $"{summary.AverageProfit:N1}%";
            _highest.Text = $"${summary.HighestSell:N2}";
            _lowest.Text = $"${summary.LowestSell:N2}";
            UpdateSuggestions();
            Render();
        }
        catch (Exception ex) { if (!IsDisposed) _status.Text = "Unable to load pricing from SQL Server."; System.Diagnostics.Debug.WriteLine(ex); }
        finally { _refreshing = false; }
    }

    private void UpdateSuggestions()
    {
        var categoryId = (_filter.SelectedItem as AdminCategory)?.Id;
        var names = _products.Where(p => (categoryId is null || p.CategoryId == categoryId) &&
                p.Name.Contains(_search.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Name).Distinct().OrderBy(n => n).Take(10).ToArray();
        var suggestions = new AutoCompleteStringCollection();
        suggestions.AddRange(names);
        _search.AutoCompleteCustomSource = suggestions;
    }

    private void Render()
    {
        _grid.Rows.Clear();
        var query = _search.Text.Trim();
        var categoryId = (_filter.SelectedItem as AdminCategory)?.Id;
        foreach (var product in _products.Where(p =>
                     (categoryId is null || p.CategoryId == categoryId) &&
                     p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(p => p.Category).ThenBy(p => p.Name))
        {
            _grid.Rows[_grid.Rows.Add(product.Id, product.Name, product.Category, $"${product.CostPrice:N2}",
                $"{product.ProfitPercent:N2}%", $"${product.SellPrice:N2}")].Tag = product;
        }
        _status.Text = _grid.Rows.Count == 0 ? "No data found for this period" : $"{_grid.Rows.Count:N0} / {_products.Count:N0} products";
        _count.Text = _grid.Rows.Count.ToString("N0");
    }
}
