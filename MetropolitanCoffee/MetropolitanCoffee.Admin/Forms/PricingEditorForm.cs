using MetropolitanCoffee.Admin.Ui;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Forms;

internal sealed class PricingEditorForm : Form
{
    private readonly AdminCatalogService _catalog;
    private readonly AdminProduct _product;
    private readonly NumericUpDown _profit = new() { DecimalPlaces = 2, Maximum = 999.99m, Width = 160 };
    private readonly Label _preview = AdminTheme.Label("", 11, FontStyle.Bold);
    private readonly Label _error = AdminTheme.Label("", 9, color: Color.Firebrick);
    private readonly Button _save = AdminTheme.Button("Save", true);

    internal PricingEditorForm(AdminCatalogService catalog, AdminProduct product)
    {
        _catalog = catalog;
        _product = product;
        Text = "Edit Profit Percentage";
        Size = new Size(510, 310);
        MinimumSize = Size;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 1, RowCount = 7 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        Controls.Add(layout);
        layout.Controls.Add(AdminTheme.Label(product.Name, 12, FontStyle.Bold), 0, 0);
        layout.Controls.Add(AdminTheme.Label($"Cost Price: ${product.CostPrice:N2}", 10), 0, 1);
        var line = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        line.Controls.Add(AdminTheme.Label("Profit %", 10, FontStyle.Bold));
        line.Controls.Add(_profit);
        layout.Controls.Add(line, 0, 2);
        layout.Controls.Add(_preview, 0, 3);
        _error.AutoSize = false;
        _error.Dock = DockStyle.Fill;
        layout.Controls.Add(_error, 0, 4);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var cancel = AdminTheme.Button("Cancel");
        cancel.Click += (_, _) => Close();
        actions.Controls.Add(cancel);
        actions.Controls.Add(_save);
        layout.Controls.Add(actions, 0, 6);
        _profit.Value = Math.Clamp(product.ProfitPercent, _profit.Minimum, _profit.Maximum);
        _profit.ValueChanged += (_, _) => UpdatePreview();
        UpdatePreview();
        _save.Click += async (_, _) => await SaveAsync();
        AcceptButton = _save;
        CancelButton = cancel;
    }

    private void UpdatePreview() => _preview.Text = $"New Sell Price: ${AdminCatalogService.CalculateSellPrice(_product.CostPrice, _profit.Value):N2}";

    private async Task SaveAsync()
    {
        _save.Enabled = false;
        _error.Text = "";
        try
        {
            var result = await _catalog.UpdateProfitAsync(_product.Id, _profit.Value);
            if (!result.Success) { _error.Text = result.Message; return; }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { _error.Text = "Unable to update pricing."; System.Diagnostics.Debug.WriteLine(ex); }
        finally { _save.Enabled = true; }
    }
}
