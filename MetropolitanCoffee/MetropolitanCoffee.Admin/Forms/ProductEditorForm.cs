using MetropolitanCoffee.Admin.Ui;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Forms;

internal sealed class ProductEditorForm : Form
{
    private readonly AdminCatalogService _catalog;
    private readonly AdminProduct? _product;
    private readonly ProductImageStore _images = new();
    private readonly TextBox _name = new() { Dock = DockStyle.Fill, MaxLength = 200 };
    private readonly TextBox _short = new() { Dock = DockStyle.Fill, MaxLength = 255 };
    private readonly TextBox _description = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _ingredients = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly ComboBox _category = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _status = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _profit = new() { Dock = DockStyle.Left, Width = 140, DecimalPlaces = 2, Maximum = 999.99m };
    private readonly CheckBox _size = new() { Text = "Size", AutoSize = true };
    private readonly CheckBox _ice = new() { Text = "Ice", AutoSize = true };
    private readonly CheckBox _sugar = new() { Text = "Sugar", AutoSize = true };
    private readonly PictureBox _preview = new() { Size = new Size(130, 110), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
    private readonly Label _imageName = AdminTheme.Label("No image selected", 9, color: AdminTheme.Muted);
    private readonly Label _sell = AdminTheme.Label("$0.00", 10, FontStyle.Bold);
    private readonly Label _error = AdminTheme.Label("", 9, color: Color.Firebrick);
    private readonly Button _save = AdminTheme.Button("Save", true);
    private string? _selectedImagePath;
    private bool _removeImage;

    internal ProductEditorForm(AdminCatalogService catalog, AdminProduct? product)
    {
        _catalog = catalog;
        _product = product;
        Text = product is null ? "Add Product" : "Edit Product";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 810);
        MinimumSize = new Size(650, 670);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();
        BuildLayout();
        Load += async (_, _) => await LoadDataAsync();
    }

    private void BuildLayout()
    {
        var outer = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, Padding = new Padding(20) };
        outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        Controls.Add(outer);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
        outer.Controls.Add(scroll, 0, 0);
        var fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(18), ColumnCount = 2 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        scroll.Controls.Add(fields);
        AddField(fields, "Product Name *", _name, 38);
        AddField(fields, "Category *", _category, 38);
        AddField(fields, "Short Description", _short, 38);
        AddField(fields, "Description", _description, 95);
        AddField(fields, "Ingredients", _ingredients, 75);
        AddField(fields, "Product Image", BuildImagePanel(), 130);
        AddField(fields, "Cost Price", AdminTheme.Label(_product is null ? "$0.00 — set by purchase orders" : $"${_product.CostPrice:N2} — set by purchase orders", 10), 35);
        AddField(fields, "Quantity", AdminTheme.Label(_product is null ? "0 — set by purchase orders" : $"{_product.Quantity:N0} — set by purchase orders", 10), 35);
        AddField(fields, "Profit Percentage *", _profit, 38);
        AddField(fields, "Sell Price", _sell, 35);
        var options = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        options.Controls.AddRange([_size, _ice, _sugar]);
        AddField(fields, "Options", options, 38);
        AddField(fields, "Status", _status, 38);
        _status.Items.AddRange(["active", "inactive"]);
        _status.SelectedIndex = 0;
        _profit.ValueChanged += (_, _) => _sell.Text = $"${AdminCatalogService.CalculateSellPrice(_product?.CostPrice ?? 0, _profit.Value):N2}";
        _imageName.AutoSize = false;
        _imageName.Width = 350;
        _error.AutoSize = false;
        _error.Dock = DockStyle.Fill;
        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footer.Controls.Add(_error, 0, 0);
        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        var cancel = AdminTheme.Button("Cancel");
        cancel.Click += (_, _) => Close();
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        footer.Controls.Add(buttons, 1, 0);
        outer.Controls.Add(footer, 0, 1);
        _save.Click += async (_, _) => await SaveAsync();
        AcceptButton = _save;
        CancelButton = cancel;
    }

    private Control BuildImagePanel()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(_preview);
        var commands = new FlowLayoutPanel { Width = 380, Height = 120, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        commands.Controls.Add(_imageName);
        var browse = AdminTheme.Button("Browse Image");
        browse.Click += (_, _) =>
        {
            using var picker = new OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png;*.webp;*.gif" };
            if (picker.ShowDialog(this) != DialogResult.OK) return;
            try { SetPreview(_images.LoadLocalPreview(picker.FileName)); _selectedImagePath = picker.FileName; _removeImage = false; _imageName.Text = Path.GetFileName(picker.FileName); }
            catch (Exception ex) { _error.Text = "Unable to open the selected image."; System.Diagnostics.Debug.WriteLine(ex); }
        };
        commands.Controls.Add(browse);
        var remove = AdminTheme.Button("Remove Image");
        remove.Click += (_, _) => { _selectedImagePath = null; _removeImage = true; _imageName.Text = "No image selected"; SetPreview(null); };
        commands.Controls.Add(remove);
        panel.Controls.Add(commands);
        return panel;
    }

    private static void AddField(TableLayoutPanel table, string title, Control input, int height)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        var label = AdminTheme.Label(title, 9.5f, FontStyle.Bold);
        label.Dock = DockStyle.Fill;
        label.TextAlign = ContentAlignment.MiddleLeft;
        table.Controls.Add(label, 0, row);
        input.Margin = new Padding(4, 2, 4, 7);
        table.Controls.Add(input, 1, row);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var categories = await _catalog.GetCategoriesAsync();
            if (IsDisposed) return;
            _category.DataSource = categories.ToList();
            _category.DisplayMember = nameof(AdminCategory.Name);
            _category.ValueMember = nameof(AdminCategory.Id);
            if (_product is null) return;
            _category.SelectedValue = _product.CategoryId;
            _name.Text = _product.Name;
            _short.Text = _product.ShortDescription ?? "";
            _description.Text = _product.Description ?? "";
            _ingredients.Text = _product.Ingredients ?? "";
            _profit.Value = Math.Clamp(_product.ProfitPercent, _profit.Minimum, _profit.Maximum);
            _status.SelectedItem = _product.Status;
            _size.Checked = _product.HasSize;
            _ice.Checked = _product.HasIce;
            _sugar.Checked = _product.HasSugar;
            _imageName.Text = _product.Image ?? "No image selected";
            SetPreview(_images.Load(_product.Image, _product.Id));
        }
        catch (Exception ex) { _error.Text = "Unable to load product data."; System.Diagnostics.Debug.WriteLine(ex); }
    }

    private void SetPreview(Image? image)
    {
        var previous = _preview.Image;
        _preview.Image = image;
        previous?.Dispose();
    }

    private async Task SaveAsync()
    {
        _error.Text = "";
        _save.Enabled = false;
        string? uploaded = null;
        try
        {
            if (_category.SelectedItem is not AdminCategory category) { _error.Text = "Select a category."; return; }
            if (_selectedImagePath is not null) uploaded = await _images.SaveUploadAsync(_selectedImagePath);
            var image = uploaded ?? (_removeImage ? null : _product?.Image);
            var input = new AdminProductInput(category.Id, _name.Text, _short.Text, _description.Text,
                _ingredients.Text, image, _profit.Value, _status.SelectedItem?.ToString() ?? "", _size.Checked, _ice.Checked, _sugar.Checked);
            var result = await _catalog.SaveProductAsync(_product?.Id, input);
            if (!result.Success) { _error.Text = result.Message; return; }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { _error.Text = ex is InvalidOperationException ? ex.Message : "Unable to save the product."; System.Diagnostics.Debug.WriteLine(ex); }
        finally
        {
            if (DialogResult != DialogResult.OK && uploaded is not null) _images.DeleteFailedUpload(uploaded);
            _save.Enabled = true;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _preview.Image?.Dispose();
        base.Dispose(disposing);
    }
}
