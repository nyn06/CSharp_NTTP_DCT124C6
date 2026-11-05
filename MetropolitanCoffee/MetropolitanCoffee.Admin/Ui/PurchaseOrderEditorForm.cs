
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

internal sealed class PurchaseOrderEditorForm : Form
{
    
private readonly PurchaseOrderService _service;

// null: tạo phiếu nhập mới
// Có giá trị: chỉnh sửa phiếu nhập hiện có
private readonly int? _editingOrderId;

private IReadOnlyList<PurchaseProduct> _products = [];

    private readonly DateTimePicker _orderDate = new()
    {
        Format = DateTimePickerFormat.Short,
        Width = 160,
        MaxDate = DateTime.Today
    };

    private readonly DataGridView _grid = AdminTheme.Grid();

    private readonly Label _total =
        AdminTheme.Label("Total Value: $0.00", 11, FontStyle.Bold);

    
internal PurchaseOrderEditorForm(
    PurchaseOrderService service,
    int? editingOrderId = null)
{
    _service = service;
    _editingOrderId = editingOrderId;


        Text = "Create Purchase Order";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(950, 650);
        MinimumSize = new Size(800, 550);

        Font = AdminTheme.Font();
        BackColor = AdminTheme.Canvas;

        
BuildLayout();

Shown += async (_, _) =>
{
    await LoadProductsAsync();

    if (IsDisposed)
        return;

    if (_editingOrderId.HasValue)
    {
        await LoadPurchaseOrderAsync(_editingOrderId.Value);
    }
};

    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 4
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));

        Controls.Add(root);

        
var heading = AdminTheme.Label(
    _editingOrderId.HasValue
        ? $"Edit Purchase Order #{_editingOrderId.Value}"
        : "Create Purchase Order",
    15,
    FontStyle.Bold);


        root.Controls.Add(heading, 0, 0);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false
        };

        toolbar.Controls.Add(
            AdminTheme.Label("Order Date:", 10));

        toolbar.Controls.Add(_orderDate);

        var addItem = AdminTheme.Button("Add Product");
        addItem.Click += (_, _) => AddProductRow();

        toolbar.Controls.Add(addItem);

        root.Controls.Add(toolbar, 0, 1);

        ConfigureGrid();
        root.Controls.Add(_grid, 0, 2);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };

        var cancel = AdminTheme.Button("Cancel");
        cancel.Click += (_, _) => Close();

        
        var save = AdminTheme.Button("Save");

        save.Enabled = true;

        save.Click += async (_, _) =>
        {
            save.Enabled = false;

            try
            {
                await SavePurchaseOrderAsync();
            }
            finally
            {
                if (!IsDisposed)
                    save.Enabled = true;
            }
        };


        footer.Controls.Add(cancel);
        footer.Controls.Add(save);
        footer.Controls.Add(_total);

        root.Controls.Add(footer, 0, 3);
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.ReadOnly = false;

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                Name = "Product",
                HeaderText = "Product",
                ReadOnly = true
            });

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                HeaderText = "Quantity"
            });

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                Name = "ImportPrice",
                HeaderText = "Import Price"
            });

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                Name = "Subtotal",
                HeaderText = "Subtotal",
                ReadOnly = true
            });

        _grid.Columns.Add(
            new DataGridViewButtonColumn
            {
                Name = "Remove",
                HeaderText = "Action",
                Text = "Remove",
                UseColumnTextForButtonValue = true
            });
        
        _grid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (_grid.Columns[e.ColumnIndex].Name != "Remove")
                return;

            _grid.Rows.RemoveAt(e.RowIndex);
            UpdateTotal();
        };

        _grid.CellEndEdit += (_, e) =>
        {
            if (e.RowIndex < 0)
                return;

            var row = _grid.Rows[e.RowIndex];

            if (row.Tag is not PurchaseOrderItemInput item)
                return;

            string quantityText =
                Convert.ToString(row.Cells["Quantity"].Value) ?? "";

            string priceText =
                Convert.ToString(row.Cells["ImportPrice"].Value) ?? "";

            if (!int.TryParse(quantityText, out int quantity) ||
                (quantity <= 0 || quantity > 1000000) ||
                !decimal.TryParse(priceText, out decimal price) ||
                price < 0 ||
                price > 99999999.99m ||
                decimal.Round(price, 2) != price)
            {
                MessageBox.Show(
                    this,
                    "Enter a valid quantity and import price (maximum 2 decimal places).",
                    "Invalid Input",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                row.Cells["Quantity"].Value = item.Quantity;
                row.Cells["ImportPrice"].Value = item.ImportPrice;
                return;
            }

            var updated = item with
            {
                Quantity = quantity,
                ImportPrice = price
            };

            row.Tag = updated;
            row.Cells["Subtotal"].Value =
                updated.Quantity * updated.ImportPrice;

            UpdateTotal();
        };

    }

    
    private void AddProductRow()
    {
        if (_products.Count == 0)
        {
            MessageBox.Show(
                this,
                "No products available. Please check the database connection.",
                "Products",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var picker = new PurchaseProductPickerForm(_products);

        if (picker.ShowDialog(this) != DialogResult.OK ||
            picker.SelectedItem is not { } item)
            return;

        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.Tag is PurchaseOrderItemInput existing &&
                existing.ProductId == item.ProductId)
            {
                MessageBox.Show(
                    this,
                    "This product is already in the purchase order.",
                    "Duplicate Product",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
        }

        var product = _products.First(
            p => p.Id == item.ProductId);

        int rowIndex = _grid.Rows.Add(
            product.Name,
            item.Quantity,
            item.ImportPrice,
            item.Quantity * item.ImportPrice
        );

        _grid.Rows[rowIndex].Tag = item;

        UpdateTotal();
    }

    private void UpdateTotal()
    {
        decimal total = 0;

        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.Tag is PurchaseOrderItemInput item)
            {
                total += item.Quantity * item.ImportPrice;
            }
        }

        _total.Text = $"Total Value: ${total:N2}";
    }
    
    
    
private async Task SavePurchaseOrderAsync()
{
    // Hoàn tất ô đang chỉnh sửa trước khi đọc dữ liệu.
    _grid.EndEdit();

    var items = _grid.Rows
        .Cast<DataGridViewRow>()
        .Where(row => row.Tag is PurchaseOrderItemInput)
        .Select(row => (PurchaseOrderItemInput)row.Tag!)
        .ToList();

    if (items.Count == 0)
    {
        MessageBox.Show(
            this,
            "Please add at least one product.",
            "Invalid Purchase Order",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
        return;
    }

    var input = new PurchaseOrderInput(
        _orderDate.Value.Date,
        items);

    try
    {
        var result = _editingOrderId.HasValue
    ? await _service.UpdateAsync(_editingOrderId.Value, input)
    : await _service.CreateAsync(input);

        if (IsDisposed)
            return;

        if (result.Status == PurchaseWriteStatus.Saved)
        {
            MessageBox.Show(
                this,
                _editingOrderId.HasValue
    ? $"Purchase order #{_editingOrderId.Value} updated successfully."
    : $"Purchase order #{result.Id} created successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        MessageBox.Show(
            this,
            $"Unable to save purchase order. Status: {result.Status}",
            "Save Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }
    catch (Exception ex)
    {
        if (IsDisposed)
            return;

        MessageBox.Show(
            this,
            $"Database error: {ex.Message}",
            "Save Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}


    
private async Task LoadProductsAsync()
{
    try
    {
        _products = await _service.GetProductsAsync();

        if (IsDisposed)
            return;

        if (_products.Count == 0)
        {
            MessageBox.Show(
                this,
                "No products are available.",
                "Products",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
    catch (Exception ex)
    {
        if (IsDisposed)
            return;

        MessageBox.Show(
            this,
            $"Unable to load products: {ex.Message}",
            "Database Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
    
private async Task LoadPurchaseOrderAsync(int orderId)
{
    try
    {
        var order = await _service.GetPurchaseOrderAsync(orderId);

        if (IsDisposed)
            return;

        if (order is null)
        {
            MessageBox.Show(
                this,
                "Purchase order not found.",
                "Not Found",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            Close();
            return;
        }

        if (!order.Status.Equals(
                "pending",
                StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                this,
                "Only pending purchase orders can be edited.",
                "Edit Not Allowed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            Close();
            return;
        }

        Text = $"Edit Purchase Order #{order.Id}";

        _orderDate.Value = order.CreatedAt.Date;

        _grid.Rows.Clear();

        foreach (var line in order.Items)
        {
            var item = new PurchaseOrderItemInput(
                line.ProductId,
                line.Quantity,
                line.ImportPrice);

            int rowIndex = _grid.Rows.Add(
                line.ProductName,
                line.Quantity,
                line.ImportPrice,
                line.Subtotal);

            _grid.Rows[rowIndex].Tag = item;
        }

        UpdateTotal();
    }
    catch (Exception ex)
    {
        if (IsDisposed)
            return;

        MessageBox.Show(
            this,
            $"Unable to load purchase order: {ex.Message}",
            "Database Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);

        Close();
    }
}

}
