using System.Data;
using MetropolitanCoffee.Admin.Ui;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Forms;

public sealed class OrderDetailForm : Form
{
    private readonly AdminReadService _reader;
    private readonly int _orderId;

    private readonly Dictionary<string, Image?> _productImages =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly AdminOrderService _orders = Program.Orders;

    private readonly ComboBox _statusSelect = new()
    {
        Width = 155,
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private readonly Button _updateStatusButton =
        AdminTheme.Button("Update", true);

    private readonly Label _lockedMessage =
        AdminTheme.Label("", 9);

    private string _currentStatus = "";

    private readonly FlowLayoutPanel _updateActions = new()
    {
        Width = 410,
        Height = 42,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = false,
        Margin = Padding.Empty
    };

    private readonly Dictionary<string, Label> _fields = new();
    private readonly DataGridView _items = AdminTheme.Grid();
    private readonly Label _timeline = AdminTheme.Label("", 10);
    private readonly Label _total = AdminTheme.Label("", 12, FontStyle.Bold);

    public OrderDetailForm(AdminReadService reader, int orderId)
    {
        _reader = reader;
        _orderId = orderId;

        Text = $"Order Detail #{orderId} | Metropolitan Coffee";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1120, 900);
        MinimumSize = new Size(900, 750);
        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();

        BuildInterface();

        Load += async (_, _) => await LoadDetailsAsync();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 3
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 420));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Controls.Add(root);

        var heading = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };

        heading.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));
        heading.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 190));

        var title = AdminTheme.Label(
            $"Order Detail #{_orderId}", 17, FontStyle.Bold);
        title.Dock = DockStyle.Fill;
        title.TextAlign = ContentAlignment.MiddleLeft;

        var back = AdminTheme.Button("Back to Orders");
        back.Click += (_, _) => Close();

        heading.Controls.Add(title, 0, 0);
        heading.Controls.Add(back, 1, 0);
        root.Controls.Add(heading, 0, 0);

        var top = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(0, 8, 0, 14)
        };

        top.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50));
        top.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50));

        root.Controls.Add(top, 0, 1);

        var general = CreateCard("General Info");
        general.Margin = new Padding(0, 0, 10, 0);
        top.Controls.Add(general, 0, 0);

        foreach (var (caption, key) in new[]
        {
            ("Order ID", "Order ID"),
            ("Customer", "Customer"),
            ("Username", "Username"),
            ("Email", "Email"),
            ("Phone", "Phone"),
            ("Date", "Date"),
            ("Payment", "Payment"),
            ("Note", "Note"),
            ("Status", "Status")
        })
        {
            AddField(general, caption, key);
        }

        var updateTitle = AdminTheme.Label(
            "Update Status", 11, FontStyle.Bold);

        updateTitle.Margin = new Padding(0, 12, 0, 6);
        general.Controls.Add(updateTitle);

        /* updateActions moved to a class field */
        var unusedUpdateActions = new FlowLayoutPanel
        {
            Width = 410,
            Height = 42,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty
        };

        _updateStatusButton.Click += async (_, _) =>
            await UpdateOrderStatusAsync();

        _updateActions.Controls.Add(_statusSelect);
        _updateActions.Controls.Add(_updateStatusButton);

        general.Controls.Add(_updateActions);

        _lockedMessage.MaximumSize = new Size(410, 0);
        _lockedMessage.ForeColor = AdminTheme.Muted;
        general.Controls.Add(_lockedMessage);

        var shipping = CreateCard("Shipping Address");
        shipping.Margin = new Padding(10, 0, 0, 0);
        top.Controls.Add(shipping, 1, 0);

        AddField(shipping, "Address", "Address");
        AddField(shipping, "Ward", "Ward");
        AddField(shipping, "City", "City");

        var timelineTitle = AdminTheme.Label(
            "Status Timeline", 11, FontStyle.Bold);
        timelineTitle.Margin = new Padding(0, 14, 0, 8);

        _timeline.AutoSize = true;
        _timeline.MaximumSize = new Size(420, 0);

        shipping.Controls.Add(timelineTitle);
        shipping.Controls.Add(_timeline);

        var bottom = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(14)
        };

        root.Controls.Add(bottom, 0, 2);

        var itemsTitle = AdminTheme.Label(
            "Order Items", 12, FontStyle.Bold);
        itemsTitle.Dock = DockStyle.Top;
        itemsTitle.Height = 32;
        itemsTitle.AutoSize = false;

        _total.Dock = DockStyle.Bottom;
        _total.Height = 40;
        _total.TextAlign = ContentAlignment.MiddleRight;
        _total.AutoSize = false;

        _items.AutoGenerateColumns = false;
        _items.ReadOnly = true;
        _items.RowTemplate.MinimumHeight = 58;
        _items.CellPainting += PaintProductImage;

        Disposed += (_, _) =>
        {
            foreach (var image in _productImages.Values)
                image?.Dispose();

            _productImages.Clear();
        };
        _items.DefaultCellStyle.WrapMode =
            DataGridViewTriState.True;
        // B04: fixed row height for images and product options.
        _items.AutoSizeRowsMode =
            DataGridViewAutoSizeRowsMode.None;

        _items.RowTemplate.Height = 64;
        _items.RowTemplate.MinimumHeight = 64;
        _items.ScrollBars = ScrollBars.Both;

        foreach (var (key, header, weight) in new[]
        {
            ("Product", "Product", 210f),
            ("Category", "Category", 110f),
            ("Unit Price", "Unit Price", 100f),
            ("Quantity", "Qty", 65f),
            ("Subtotal", "Subtotal", 110f)
        })
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = key == "Product" ? "ProductDisplay" : key,
                HeaderText = header,
                FillWeight = weight,
                ReadOnly = true
            };

            if (key is "Unit Price" or "Subtotal")
                column.DefaultCellStyle.Format = "$#,##0.00";

            if (key == "Product")
            {
                column.DefaultCellStyle.Padding =
                    new Padding(54, 4, 4, 4);

                column.DefaultCellStyle.WrapMode =
                    DataGridViewTriState.True;
            }

            _items.Columns.Add(column);
        }

        bottom.Controls.Add(_items);
        bottom.Controls.Add(_total);
        bottom.Controls.Add(itemsTitle);
    }

    private static FlowLayoutPanel CreateCard(string title)
    {
        var card = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.White,
            Padding = new Padding(18)
        };

        var heading = AdminTheme.Label(
            title, 12, FontStyle.Bold);
        heading.Margin = new Padding(0, 0, 0, 12);

        card.Controls.Add(heading);

        return card;
    }

    private void AddField(
        FlowLayoutPanel container,
        string caption,
        string key)
    {
        var line = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(430, 0),
            Font = AdminTheme.Font(9.5f),
            ForeColor = AdminTheme.Text,
            Margin = new Padding(0, 2, 0, 5)
        };

        line.Text = caption + ": ...";

        container.Controls.Add(line);
        _fields[key] = line;
    }

    private void ConfigureStatusUpdate(string status)
    {
        _currentStatus = status;

        string[] choices = status switch
        {
            "pending" => new[]
            {
                "confirmed", "shipping",
                "delivered", "cancelled"
            },
            "confirmed" => new[]
            {
                "shipping", "delivered", "cancelled"
            },
            "shipping" => new[] { "delivered" },
            _ => Array.Empty<string>()
        };

        _statusSelect.Items.Clear();

        foreach (string choice in choices)
            _statusSelect.Items.Add(choice);

        bool locked = choices.Length == 0;

        _statusSelect.Enabled = !locked;
        _statusSelect.Visible = !locked;
        _updateActions.Visible = !locked;
        _updateStatusButton.Visible = !locked;
        _updateStatusButton.Enabled = !locked;

        _lockedMessage.Text = locked
            ? $"This order is {status} - no further updates allowed."
            : "* Status can only move forward. Cannot revert.";

        if (!locked)
            _statusSelect.SelectedIndex = 0;
    }

    private async Task UpdateOrderStatusAsync()
    {
        if (_statusSelect.SelectedItem is not string newStatus)
            return;

        if (_currentStatus is "delivered" or "cancelled")
            return;

        var answer = MessageBox.Show(
            this,
            $"Update order #{_orderId} from {_currentStatus} to {newStatus}?",
            "Update Order Status",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (answer != DialogResult.Yes)
            return;

        _updateStatusButton.Enabled = false;

        try
        {
            var result = await _orders.ChangeStatusAsync(
                _orderId,
                _currentStatus,
                newStatus);

            if (IsDisposed)
                return;

            MessageBox.Show(
                this,
                result.Message,
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                result.Success
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning);

            // Refresh even on failure because another action
            // may have changed the order status.
            await LoadDetailsAsync();
        }
        catch (Exception ex)
        {
            if (IsDisposed)
                return;

            MessageBox.Show(
                this,
                ex.Message,
                "Order update failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            if (!IsDisposed &&
                _currentStatus is not ("delivered" or "cancelled"))
            {
                _updateStatusButton.Enabled = true;
            }
        }
    }
    private void PaintProductImage(
        object? sender,
        DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        if (_items.Columns[e.ColumnIndex].DataPropertyName
            != "ProductDisplay")
            return;

        if (e.Graphics is null)
            return;

        e.Paint(e.CellBounds, DataGridViewPaintParts.All);

        if (_items.Rows[e.RowIndex].DataBoundItem
            is DataRowView row)
        {
            string fileName = Convert.ToString(
                row["Image"])?.Trim() ?? "";

            var image = GetProductImage(fileName);

            if (image is not null)
            {
                var rectangle = new Rectangle(
                    e.CellBounds.Left + 7,
                    e.CellBounds.Top +
                        Math.Max(0, (e.CellBounds.Height - 40) / 2),
                    40,
                    40);

                e.Graphics.DrawImage(image, rectangle);
            }
        }

        e.Handled = true;
    }

    private Image? GetProductImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        // Only accept a file name, never an arbitrary path.
        fileName = Path.GetFileName(
            fileName.Replace('\\', '/'));

        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        if (_productImages.TryGetValue(fileName, out var cached))
            return cached;

        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Products",
            fileName);

        Image? result = null;

        try
        {
            if (File.Exists(path))
            {
                using var original = Image.FromFile(path);
                result = new Bitmap(original);
            }
        }
        catch (Exception ex) when (
            ex is IOException or
            UnauthorizedAccessException or
            ArgumentException or
            OutOfMemoryException)
        {
            // Missing or invalid images do not prevent
            // the order details from being displayed.
        }

        _productImages[fileName] = result;
        return result;
    }
    private static string BuildProductDisplay(DataRow item)
    {
        string name = Convert.ToString(item["Product"]) ?? "";

        var options = new List<string>();

        foreach (var (column, label) in new[]
        {
            ("Size", "Size"),
            ("Ice", "Ice"),
            ("Sugar", "Sugar")
        })
        {
            if (!item.Table.Columns.Contains(column))
                continue;

            string value = Convert.ToString(item[column])?.Trim() ?? "";

            if (string.IsNullOrEmpty(value) ||
                value.Equals("none", StringComparison.OrdinalIgnoreCase))
                continue;

            options.Add($"{label}: {value}");
        }

        return options.Count == 0
            ? name
            : name + Environment.NewLine +
              string.Join("  |  ", options);
    }
    private async Task LoadDetailsAsync()
    {
        try
        {
            var details = await _reader.GetOrderDetailAsync(_orderId);
            var items = await _reader.GetDetailItemsAsync(
                "orders", _orderId);

            if (IsDisposed)
                return;

            if (details.Rows.Count == 0)
            {
                MessageBox.Show(this, "Order not found.");
                Close();
                return;
            }

            var order = details.Rows[0];

            foreach (var (key, label) in _fields)
            {
                string value = Convert.ToString(order[key]) ?? "";

                if (key == "Date" &&
                    order[key] != DBNull.Value)
                {
                    value = Convert.ToDateTime(order[key])
                        .ToString("MM/dd/yyyy HH:mm");
                }

                if (key == "Order ID")
                    value = "#" + value;

                if (string.IsNullOrWhiteSpace(value))
                    value = "none";

                label.Text = key + ": " + value;
            }

            string status = Convert.ToString(order["Status"])?
                .Trim().ToLowerInvariant() ?? "";

            ConfigureStatusUpdate(status);

            string[] steps =
            {
                "pending", "confirmed",
                "shipping", "delivered"
            };

            int index = Array.IndexOf(steps, status);

            var lines = new List<string>();

            foreach (var step in steps)
            {
                int stepIndex = Array.IndexOf(steps, step);

                bool done = status != "cancelled" &&
                            stepIndex <= index;

                lines.Add(
                    (done ? "✓ " : "○ ") +
                    char.ToUpperInvariant(step[0]) +
                    step.Substring(1) +
                    (step == status ? "  ← current" : ""));
            }

            if (status == "cancelled")
                lines.Add("✕ Cancelled  ← current");

            _timeline.Text = string.Join(
                Environment.NewLine, lines);

            if (!items.Columns.Contains("ProductDisplay"))
                items.Columns.Add("ProductDisplay", typeof(string));

            foreach (DataRow item in items.Rows)
            {
                item["ProductDisplay"] = BuildProductDisplay(item);
            }

            _items.DataSource = items;

            decimal total = Convert.ToDecimal(order["Total"]);
            _total.Text = $"Total: ${total:N2}";
        }
        catch (Exception ex)
        {
            if (IsDisposed)
                return;

            MessageBox.Show(
                this,
                ex.Message,
                "Unable to load order details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
