
using MetropolitanCoffee.Core.Models;

namespace MetropolitanCoffee.Admin.Ui;

internal sealed class PurchaseProductPickerForm : Form
{
    private readonly ComboBox _products = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Dock = DockStyle.Fill
    };

    private readonly NumericUpDown _quantity = new()
    {
        Minimum = 1,
        Maximum = 1000000,
        Value = 1,
        Dock = DockStyle.Fill
    };

    private readonly NumericUpDown _price = new()
    {
        Minimum = 0,
        Maximum = 99999999.99m,
        DecimalPlaces = 2,
        Increment = 0.50m,
        Dock = DockStyle.Fill
    };

    internal PurchaseOrderItemInput? SelectedItem { get; private set; }

    internal PurchaseProductPickerForm(
        IReadOnlyList<PurchaseProduct> products)
    {
        Text = "Add Product";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(520, 340);
        MinimumSize = new Size(450, 320);

        Font = AdminTheme.Font();
        BackColor = AdminTheme.Canvas;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 2,
            RowCount = 4
        };

        root.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 130));
        root.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < 3; i++)
        {
            root.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));
        }

        root.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        Controls.Add(root);

        root.Controls.Add(
            AdminTheme.Label("Product:", 10), 0, 0);
        root.Controls.Add(_products, 1, 0);

        root.Controls.Add(
            AdminTheme.Label("Quantity:", 10), 0, 1);
        root.Controls.Add(_quantity, 1, 1);

        root.Controls.Add(
            AdminTheme.Label("Import Price:", 10), 0, 2);
        root.Controls.Add(_price, 1, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };

        var cancel = AdminTheme.Button("Cancel");
        cancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        var add = AdminTheme.Button("Add");
        add.Click += (_, _) => ConfirmSelection();

        actions.Controls.Add(cancel);
        actions.Controls.Add(add);

        root.Controls.Add(actions, 0, 3);
        root.SetColumnSpan(actions, 2);

        foreach (var product in products)
        {
            _products.Items.Add(new ProductOption(product));
        }

        if (_products.Items.Count > 0)
        {
            _products.SelectedIndex = 0;
        }

        _products.SelectedIndexChanged += (_, _) =>
        {
            if (_products.SelectedItem is ProductOption option)
            {
                _price.Value = Math.Clamp(
                    option.Product.CostPrice,
                    _price.Minimum,
                    _price.Maximum);
            }
        };

        if (_products.SelectedItem is ProductOption first)
        {
            _price.Value = Math.Clamp(
                first.Product.CostPrice,
                _price.Minimum,
                _price.Maximum);
        }
    }

    private void ConfirmSelection()
    {
        if (_products.SelectedItem is not ProductOption option)
        {
            MessageBox.Show(
                this,
                "Please select a product.",
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        SelectedItem = new PurchaseOrderItemInput(
            option.Product.Id,
            (int)_quantity.Value,
            _price.Value);

        DialogResult = DialogResult.OK;
        Close();
    }

    private sealed record ProductOption(PurchaseProduct Product)
    {
        public override string ToString() => Product.Name;
    }
}
