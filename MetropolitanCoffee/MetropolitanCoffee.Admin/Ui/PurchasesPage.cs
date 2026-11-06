
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

internal sealed class PurchasesPage : UserControl
{
    private readonly PurchaseOrderService _purchases;
    private readonly DataGridView _grid = AdminTheme.Grid();
    private readonly TextBox _search = new()
    {
        Width = 230,
        PlaceholderText = "Search purchase orders..."
    };
    private readonly ComboBox _filter = new()
    {
        Width = 170,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly Label _status =
        AdminTheme.Label("", 9, color: AdminTheme.Muted);

    private IReadOnlyList<PurchaseOrderSummary> _orders = [];

    internal PurchasesPage(PurchaseOrderService purchases)
    {
        _purchases = purchases;
        Dock = DockStyle.Fill;
        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();

        BuildLayout();

        Load += async (_, _) => await RefreshAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2
        };

        root.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 54));

        root.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        Controls.Add(root);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = true
        };

        root.Controls.Add(actions, 0, 0);

        _search.Margin = new Padding(0, 0, 8, 0);
        _search.TextChanged += (_, _) => Render();
        actions.Controls.Add(_search);

        _filter.Items.AddRange(
            ["All Statuses", "pending", "completed"]);

        _filter.SelectedIndex = 0;
        _filter.SelectedIndexChanged += (_, _) => Render();
        actions.Controls.Add(_filter);

        var refresh = AdminTheme.Button("Refresh");
        refresh.Click += async (_, _) => await RefreshAsync();
        actions.Controls.Add(refresh);



var create = AdminTheme.Button("Create Purchase Order");

create.Click += async (_, _) =>
{
    using var dialog =
        new PurchaseOrderEditorForm(_purchases);

    if (dialog.ShowDialog(this) == DialogResult.OK)
    {
        await RefreshAsync();
    }
};

actions.Controls.Add(create);



        var card = AdminTheme.Card();
        card.Dock = DockStyle.Fill;
        card.Margin = Padding.Empty;
        root.Controls.Add(card, 0, 1);

        var heading = AdminTheme.Label(
            "Purchase Order Management",
            13,
            FontStyle.Bold);

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
        _grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "Purchase Order ID"
            });

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                Name = "Date",
                HeaderText = "Order Date"
            });

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                Name = "Total",
                HeaderText = "Total Value"
            });

        _grid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status"
            });

        _grid.Columns.Add(
            new DataGridViewButtonColumn
            {
                Name = "Details",
                HeaderText = "Action",
                Text = "Details",
                UseColumnTextForButtonValue = true
            });

        _grid.Columns.Add(
    new DataGridViewButtonColumn
    {
        Name = "Edit",
        HeaderText = "Edit",
        Text = "Edit",
        UseColumnTextForButtonValue = false
    });


_grid.Columns.Add(
    new DataGridViewButtonColumn
    {
        Name = "Complete",
        HeaderText = "Complete",
        Text = "Complete",
        UseColumnTextForButtonValue = false
    });



_grid.CellContentClick += async (_, e) =>
{
    if (e.RowIndex < 0 || e.ColumnIndex < 0)
        return;

    if (_grid.Rows[e.RowIndex].Tag
        is not PurchaseOrderSummary order)
        return;

    string columnName = _grid.Columns[e.ColumnIndex].Name;

    if (columnName == "Details")
    {
        using var dialog =
            new PurchaseOrderDetailForm(_purchases, order.Id);

        dialog.ShowDialog(this);
        return;
    }

    if (columnName == "Edit")
    {
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
            return;
        }

        using var dialog =
            new PurchaseOrderEditorForm(_purchases, order.Id);

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            await RefreshAsync();
        }
    }

if (columnName == "Complete")
{
    if (!order.Status.Equals(
            "pending",
            StringComparison.OrdinalIgnoreCase))
    {
        MessageBox.Show(
            this,
            "Only pending purchase orders can be completed.",
            "Complete Not Allowed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
        return;
    }

    var confirmation = MessageBox.Show(
        this,
        $"Are you sure you want to complete purchase order #{order.Id}?\n\n" +
        "This action will update product stock and cost prices.",
        "Confirm Purchase Order Completion",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);

    if (confirmation != DialogResult.Yes)
        return;

    try
    {
        var result = await _purchases.CompleteAsync(order.Id);

        if (IsDisposed)
            return;

        if (result.Status == PurchaseWriteStatus.Completed)
        {
            MessageBox.Show(
                this,
                $"Purchase order #{order.Id} completed successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await RefreshAsync();
        }
        else
        {
            MessageBox.Show(
                this,
                $"Unable to complete purchase order. Status: {result.Status}",
                "Complete Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            await RefreshAsync();
        }
    }
    catch (Exception ex)
    {
        if (IsDisposed)
            return;

        MessageBox.Show(
            this,
            $"Database error: {ex.Message}",
            "Complete Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}

};


    }

    internal async Task RefreshAsync()
    {
        _status.Text = "Loading purchase orders...";

        try
        {
            _orders = await _purchases.GetPurchaseOrdersAsync();

            if (IsDisposed)
                return;

            Render();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                _status.Text =
                    "Unable to load purchase orders from SQL Server.";

            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private void Render()
    {
        _grid.Rows.Clear();

        string query = _search.Text.Trim();
        string filter = _filter.SelectedItem?.ToString()
            ?? "All Statuses";

        foreach (var order in _orders.Where(o =>
            (filter == "All Statuses" ||
             o.Status.Equals(
                 filter,
                 StringComparison.OrdinalIgnoreCase)) &&
            (o.Id.ToString().Contains(query) ||
             o.Status.Contains(
                 query,
                 StringComparison.OrdinalIgnoreCase))))
        {
            var row = _grid.Rows[_grid.Rows.Add(
                order.Id,
                order.CreatedAt.ToString("dd/MM/yyyy"),
                $"${order.TotalValue:N2}",
                order.Status
            )];

            row.Tag = order;


bool isPending = order.Status.Equals(
    "pending",
    StringComparison.OrdinalIgnoreCase);

row.Cells["Edit"].Value =
    isPending ? "Edit" : "—";

row.Cells["Complete"].Value =
    isPending ? "Complete" : "—";

row.Cells["Edit"].ReadOnly = !isPending;
row.Cells["Complete"].ReadOnly = !isPending;


        }

        _status.Text =
            $"{_grid.Rows.Count:N0} / {_orders.Count:N0} purchase orders";
    }
}
