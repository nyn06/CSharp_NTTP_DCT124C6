
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

internal sealed class PurchaseOrderDetailForm : Form
{
    private readonly PurchaseOrderService _service;
    private readonly int _orderId;

    private readonly DataGridView _grid = AdminTheme.Grid();

    private readonly Label _information =
        AdminTheme.Label("", 10);

    private readonly Label _total =
        AdminTheme.Label("", 11, FontStyle.Bold);

    internal PurchaseOrderDetailForm(
        PurchaseOrderService service,
        int orderId)
    {
        _service = service;
        _orderId = orderId;

        Text = $"Purchase Order #{orderId}";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(850, 560);
        MinimumSize = new Size(700, 450);

        Font = AdminTheme.Font();
        BackColor = AdminTheme.Canvas;

        BuildLayout();

        Shown += async (_, _) => await LoadOrderAsync();
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

        root.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 45));

        root.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 45));

        root.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        root.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 55));

        Controls.Add(root);

        var heading = AdminTheme.Label(
            $"Purchase Order #{_orderId}",
            15,
            FontStyle.Bold);

        root.Controls.Add(heading, 0, 0);

        _information.Dock = DockStyle.Fill;
        root.Controls.Add(_information, 0, 1);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                HeaderText = "Product",
                Name = "Product"
            });

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                HeaderText = "Quantity",
                Name = "Quantity"
            });

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                HeaderText = "Import Price",
                Name = "ImportPrice"
            });

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                HeaderText = "Subtotal",
                Name = "Subtotal"
            });

        root.Controls.Add(_grid, 0, 2);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };

        footer.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        footer.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 110));

        _total.Dock = DockStyle.Fill;
        _total.TextAlign = ContentAlignment.MiddleLeft;

        footer.Controls.Add(_total, 0, 0);

        var close = AdminTheme.Button("Close");

        close.Click += (_, _) => Close();

        footer.Controls.Add(close, 1, 0);

        root.Controls.Add(footer, 0, 3);
    }

    private async Task LoadOrderAsync()
    {
        _information.Text = "Loading purchase order...";

        try
        {
            PurchaseOrder? order =
                await _service.GetPurchaseOrderAsync(_orderId);

            if (IsDisposed)
                return;

            if (order is null)
            {
                MessageBox.Show(
                    this,
                    "Purchase order not found.",
                    "Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Close();
                return;
            }

            _information.Text =
                $"Date: {order.CreatedAt:dd/MM/yyyy}    " +
                $"Status: {order.Status}";

            _grid.Rows.Clear();

            foreach (var item in order.Items)
            {
                _grid.Rows.Add(
                    item.ProductName,
                    item.Quantity,
                    $"${item.ImportPrice:N2}",
                    $"${item.Subtotal:N2}"
                );
            }

            _total.Text =
                $"Total Value: ${order.TotalValue:N2}";
        }
        catch (Exception ex)
        {
            if (IsDisposed)
                return;

            _information.Text = "Unable to load purchase order.";

            MessageBox.Show(
                this,
                ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
