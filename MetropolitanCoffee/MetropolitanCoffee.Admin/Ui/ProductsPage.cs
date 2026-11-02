using MetropolitanCoffee.Admin.Forms;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

internal sealed class ProductsPage : UserControl
{
    private readonly AdminCatalogService _catalog;
    private readonly ProductImageStore _images = new();
    private readonly DataGridView _grid = AdminTheme.Grid();
    private readonly TextBox _search = new() { Width = 230, PlaceholderText = "Search products..." };
    private readonly ComboBox _filter = new() { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _status = AdminTheme.Label("", 9, color: AdminTheme.Muted);
    private IReadOnlyList<AdminProduct> _products = [];

    internal ProductsPage(AdminCatalogService catalog)
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
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = AdminTheme.Font(10, FontStyle.Bold) };
        Controls.Add(tabs);
        var productsTab = new TabPage("Products") { BackColor = AdminTheme.Canvas, Padding = new Padding(4) };
        var categoriesTab = new TabPage("Categories") { BackColor = AdminTheme.Canvas, Padding = new Padding(4) };
        tabs.TabPages.AddRange([productsTab, categoriesTab]);
        categoriesTab.Controls.Add(new CategoriesPage(_catalog));
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        productsTab.Controls.Add(root);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true };
        root.Controls.Add(actions, 0, 0);
        _search.TextChanged += (_, _) => Render();
        _search.Margin = new Padding(0, 0, 8, 0);
        actions.Controls.Add(_search);
        _filter.Items.Add("All Categories");
        _filter.SelectedIndex = 0;
        _filter.SelectedIndexChanged += (_, _) => Render();
        actions.Controls.Add(_filter);
        var refresh = AdminTheme.Button("Refresh");
        refresh.Click += async (_, _) => await RefreshAsync();
        actions.Controls.Add(refresh);
        var add = AdminTheme.Button("Add Product", true);
        add.Click += async (_, _) => await EditAsync(null);
        actions.Controls.Add(add);
        var card = AdminTheme.Card();
        card.Dock = DockStyle.Fill;
        card.Margin = Padding.Empty;
        root.Controls.Add(card, 0, 1);
        var heading = AdminTheme.Label("Product Management", 13, FontStyle.Bold);
        heading.Dock = DockStyle.Top;
        heading.AutoSize = false;
        heading.Height = 35;
        card.Controls.Add(_grid);
        card.Controls.Add(heading);
        _status.Dock = DockStyle.Bottom;
        _status.AutoSize = false;
        _status.Height = 24;
        card.Controls.Add(_status);
        ConfigureGrid();
    }

    private void ConfigureGrid()
    {
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.ScrollBars = ScrollBars.Both;
        _grid.RowTemplate.Height = 66;
        _grid.Columns.Add(new DataGridViewImageColumn { Name = "Image", HeaderText = "Image", Width = 75, ImageLayout = DataGridViewImageCellLayout.Zoom });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Product Name", Width = 180 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", Width = 135 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cost", HeaderText = "Cost Price", Width = 95 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Profit", HeaderText = "Profit %", Width = 85 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sell", HeaderText = "Sell Price", Width = 95 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Quantity", Width = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Options", HeaderText = "Options", Width = 125 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 85 });
        _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Actions", Text = "Edit", UseColumnTextForButtonValue = true, Width = 65 });
        _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Delete", HeaderText = "", Text = "Delete", UseColumnTextForButtonValue = true, Width = 70 });
        _grid.CellContentClick += async (_, e) =>
        {
            if (e.RowIndex < 0 || _grid.Rows[e.RowIndex].Tag is not AdminProduct product) return;
            if (_grid.Columns[e.ColumnIndex].Name == "Edit") await EditAsync(product);
            if (_grid.Columns[e.ColumnIndex].Name == "Delete") await DeleteAsync(product);
        };
    }

    internal async Task RefreshAsync()
    {
        _status.Text = "Loading products...";
        try
        {
            var categories = await _catalog.GetCategoriesAsync();
            _products = await _catalog.GetProductsAsync();
            if (IsDisposed) return;
            var selected = (_filter.SelectedItem as AdminCategory)?.Id;
            _filter.BeginUpdate();
            _filter.Items.Clear();
            _filter.Items.Add("All Categories");
            foreach (var category in categories) _filter.Items.Add(category);
            _filter.DisplayMember = nameof(AdminCategory.Name);
            _filter.SelectedItem = _filter.Items.OfType<AdminCategory>().FirstOrDefault(c => c.Id == selected) ?? _filter.Items[0];
            _filter.EndUpdate();
            Render();
        }
        catch (Exception ex) { if (!IsDisposed) _status.Text = "Unable to load products from SQL Server."; System.Diagnostics.Debug.WriteLine(ex); }
    }

    private void Render()
    {
        foreach (DataGridViewRow row in _grid.Rows)
            if (row.Cells["Image"].Value is Image image) image.Dispose();
        _grid.Rows.Clear();
        var query = _search.Text.Trim();
        var categoryId = (_filter.SelectedItem as AdminCategory)?.Id;
        foreach (var product in _products.Where(p =>
                     (categoryId is null || p.CategoryId == categoryId) &&
                     (p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.Category.Contains(query, StringComparison.OrdinalIgnoreCase))))
        {
            Bitmap? image = null;
            try { image = _images.Load(product.Image, product.Id); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
            var options = string.Join(", ", new[] { product.HasSize ? "Size" : null, product.HasIce ? "Ice" : null, product.HasSugar ? "Sugar" : null }.Where(x => x is not null));
            var row = _grid.Rows[_grid.Rows.Add(image, product.Name, product.Category, $"${product.CostPrice:N2}",
                $"{product.ProfitPercent:N2}%", $"${product.SellPrice:N2}", product.Quantity.ToString("N0"), options, product.Status)];
            row.Tag = product;
        }
        _status.Text = $"{_grid.Rows.Count:N0} / {_products.Count:N0} products";
    }

    private async Task EditAsync(AdminProduct? product)
    {
        if (product is not null) product = await _catalog.GetProductAsync(product.Id);
        using var dialog = new ProductEditorForm(_catalog, product);
        if (dialog.ShowDialog(this) == DialogResult.OK) await RefreshAsync();
    }

    private async Task DeleteAsync(AdminProduct product)
    {
        if (MessageBox.Show(this, $"Delete or hide \"{product.Name}\"? Products used by orders or purchases will be hidden.",
                "Confirm Product Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            var result = await _catalog.DeleteOrHideProductAsync(product.Id);
            MessageBox.Show(this, result.Message, "Products", MessageBoxButtons.OK,
                result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (result.Success) await RefreshAsync();
        }
        catch (Exception ex) { MessageBox.Show(this, "Unable to remove the product.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Error); System.Diagnostics.Debug.WriteLine(ex); }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            foreach (DataGridViewRow row in _grid.Rows)
                if (row.Cells["Image"].Value is Image image) image.Dispose();
        base.Dispose(disposing);
    }
}
