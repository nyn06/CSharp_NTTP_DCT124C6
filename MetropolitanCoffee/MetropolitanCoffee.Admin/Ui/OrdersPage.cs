
using System.Data;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

public sealed class OrdersPage : UserControl
{
    private readonly AdminReadService _reader;
    private readonly AdminOrderService _orders;
    private readonly DataGridView _grid = AdminTheme.Grid();
    private readonly TextBox _search = new()
    {
        Width = 230,
        PlaceholderText = "Search orders..."
    };

    private readonly ComboBox _filter = new()
    {
        Width = 160,
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private readonly Label _state =
        AdminTheme.Label("Loading orders...", 9);

    private DataTable? _table;

    private readonly System.Windows.Forms.Timer
        _ordersAutoRefreshTimer = new()
        {
            Interval = 15000
        };

    private bool _isLoadingOrders;

    public OrdersPage(AdminReadService reader)
    {
        _reader = reader;
        _orders = Program.Orders;

        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();

        BuildInterface();

        _ordersAutoRefreshTimer.Tick += async (_, _) =>
        {
            if (IsDisposed || !Visible || _isLoadingOrders)
                return;

            // Avoid disrupting an administrator who is
            // interacting with filters or editing an order.
            if (_grid.ContainsFocus ||
                _ward.Focused ||
                _street.Focused ||
                _search.Focused ||
                _filter.Focused ||
                _fromDate.Focused ||
                _toDate.Focused)
                return;

            // Do not refresh behind an active modal dialog.
            if (FindForm() is Form owner && !owner.Enabled)
                return;

            await LoadOrdersAsync();
        };

        Load += async (_, _) =>
        {
            await LoadOrdersAsync();

            if (!IsDisposed && Visible)
                _ordersAutoRefreshTimer.Stop();
        };

        VisibleChanged += (_, _) =>
        {
            if (!Visible)
                _ordersAutoRefreshTimer.Stop();
            else if (IsHandleCreated && !_ordersAutoRefreshTimer.Enabled)
                _ordersAutoRefreshTimer.Stop();
        };

        Disposed += (_, _) =>
            _ordersAutoRefreshTimer.Dispose();
    }

    private readonly DateTimePicker _fromDate = new()
    {
        Format = DateTimePickerFormat.Short,
        ShowCheckBox = true,
        Checked = false,
        MaxDate = DateTime.Today,
        Dock = DockStyle.Fill
    };

    private readonly DateTimePicker _toDate = new()
    {
        Format = DateTimePickerFormat.Short,
        ShowCheckBox = true,
        Checked = false,
        MaxDate = DateTime.Today,
        Dock = DockStyle.Fill
    };

    private readonly TextBox _city = new()
    {
        Text = "Ho Chi Minh",
        ReadOnly = true,
        Dock = DockStyle.Fill
    };

    private readonly TextBox _ward = new()
    {
        PlaceholderText = "Ward",
        Dock = DockStyle.Fill
    };

    private readonly TextBox _street = new()
    {
        PlaceholderText = "Street Name",
        Dock = DockStyle.Fill
    };
    private void BuildB04Filters(TableLayoutPanel root)
    {
        var panel = AdminTheme.Card();
        panel.Dock = DockStyle.Fill;
        panel.Margin = new Padding(0, 0, 0, 12);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 3
        };

        for (int i = 0; i < 6; i++)
            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100f / 6));

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        string[] labels =
        {
            "From Date",
            "To Date",
            "Status",
            "City",
            "Ward",
            "Street Name"
        };

        Control[] inputs =
        {
            _fromDate, _toDate, _filter,
            _city, _ward, _street
        };

        for (int i = 0; i < labels.Length; i++)
        {
            var label = AdminTheme.Label(labels[i], 9);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;

            inputs[i].Margin = new Padding(0, 0, 8, 0);
            inputs[i].Dock = DockStyle.Fill;

            layout.Controls.Add(label, i, 0);
            layout.Controls.Add(inputs[i], i, 1);
        }

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        var filterButton = AdminTheme.Button("Filter", true);
        filterButton.Click += (_, _) =>
        {
            if (_fromDate.Checked && _toDate.Checked &&
                _fromDate.Value.Date > _toDate.Value.Date)
            {
                MessageBox.Show(this,
                    "From Date cannot be later than To Date.",
                    "Invalid date range",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ApplyFilter();
        };

        var resetButton = AdminTheme.Button("Reset");
        resetButton.Click += (_, _) =>
        {
            _fromDate.Checked = false;
            _toDate.Checked = false;
            _filter.SelectedIndex = 0;
            _city.Text = "Ho Chi Minh";
            _ward.Clear();
            _street.Clear();
            _search.Clear();
            ApplyFilter();
        };

        buttons.Controls.Add(filterButton);
        buttons.Controls.Add(resetButton);

        layout.Controls.Add(buttons, 0, 2);
        layout.SetColumnSpan(buttons, 6);

        panel.Controls.Add(layout);
        root.Controls.Add(panel, 0, 0);
    }
    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Controls.Add(root);
        BuildB04Filters(root);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 12)
        };

        root.Controls.Add(actions, 0, 1);

        _search.TextChanged += (_, _) => ApplyFilter();
        actions.Controls.Add(_search);

        _filter.Items.AddRange(new object[]
        {
            "All",
            "pending",
            "confirmed",
            "shipping",
            "delivered",
            "cancelled"
        });

        _filter.SelectedIndex = 0;
        _filter.SelectedIndexChanged += (_, _) => ApplyFilter();
        // Status is displayed in the B04 filter panel.

        var refresh = AdminTheme.Button("Refresh");
        refresh.Click += async (_, _) => await LoadOrdersAsync();
        actions.Controls.Add(refresh);

        var details = AdminTheme.Button("Details", true);
        details.Click += async (_, _) => await ShowDetailsAsync();
        actions.Controls.Add(details);

        var confirm = AdminTheme.Button("Confirm");
        confirm.Click += async (_, _) => await ConfirmOrderAsync();
        actions.Controls.Add(confirm);

        var ship = AdminTheme.Button("Ship");
        ship.Click += async (_, _) => await ShipOrderAsync();
        actions.Controls.Add(ship);

        var deliver = AdminTheme.Button("Deliver");
        deliver.Click += async (_, _) => await DeliverOrderAsync();
        actions.Controls.Add(deliver);

        var cancel = AdminTheme.Button("Cancel Order");
        cancel.Click += async (_, _) => await CancelOrderAsync();
        actions.Controls.Add(cancel);

        var card = AdminTheme.Card();
        card.Dock = DockStyle.Fill;
        card.Margin = Padding.Empty;

        root.Controls.Add(card, 0, 2);

        _state.Dock = DockStyle.Bottom;
        _state.Height = 24;

        card.Controls.Add(_grid);
        card.Controls.Add(_state);

        // B04: View button opens the selected order details.
        _grid.CellContentClick += async (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (_grid.Columns[e.ColumnIndex].Name != "B04Detail")
                return;

            _grid.CurrentCell = _grid.Rows[e.RowIndex]
                .Cells["Order ID"];

            await ShowDetailsAsync();
        };
        _grid.CellContentClick += async (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (_grid.Columns[e.ColumnIndex].Name != "B04Save")
                return;

            await SaveB04StatusAsync(e.RowIndex);
        };
        _grid.CellPainting += PaintB04InlineSave;

        _grid.CellMouseClick += async (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (_grid.Columns[e.ColumnIndex].Name != "B04UpdateStatus")
                return;

            string status = Convert.ToString(
                _grid.Rows[e.RowIndex].Cells["Status"].Value
            )?.Trim().ToLowerInvariant() ?? "";

            if (status is "delivered" or "cancelled")
                return;

            var cell = _grid.GetCellDisplayRectangle(
                e.ColumnIndex, e.RowIndex, false);

            if (e.X < cell.Width - 60)
                return;

            await SaveB04StatusAsync(e.RowIndex);
        };
        _grid.ColumnHeaderMouseClick += (_, e) =>
        {
            if (_grid.Columns[e.ColumnIndex].Name == "Address (Ward)")
                SortB04Ward();
        };
        _grid.CellDoubleClick += async (_, _) =>
        await ShowDetailsAsync();
        }

    private async Task LoadOrdersAsync()
    {
        if (_isLoadingOrders)
            return;

        _isLoadingOrders = true;

        try
        {
            _state.Text = "Loading orders...";

            _table = await _reader.GetTableAsync("orders");

            if (IsDisposed)
                return;

            _grid.DataSource = _table;
            ConfigureB04AddressSuggestions();
            ConfigureB04Table();
            BeginInvoke((Action)(() => ConfigureStatusChoices()));

            ApplyFilter();

            if (_table is not null)
            {
                _table.DefaultView.Sort = _wardSortMode switch
                {
                    1 => "[Ward] ASC, [Order Date] DESC",
                    2 => "[Ward] DESC, [Order Date] DESC",
                    _ => "[Order Date] DESC, [Order ID] DESC"
                };
            }
        }
        catch (Exception ex)
        {
            if (IsDisposed)
                return;

            _state.Text = "Unable to load orders.";

            MessageBox.Show(
                this,
                ex.Message,
                "Database error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _isLoadingOrders = false;
        }
    }

    private async Task SaveB04StatusAsync(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= _grid.Rows.Count)
            return;

        _grid.EndEdit();

        var gridRow = _grid.Rows[rowIndex];

        if (gridRow.DataBoundItem is not DataRowView data)
            return;

        int orderId = Convert.ToInt32(data["Order ID"]);

        string currentStatus =
            Convert.ToString(data["Status"])?
            .Trim().ToLowerInvariant() ?? "";

        string newStatus =
            Convert.ToString(gridRow.Cells["B04UpdateStatus"].Value)?
            .Trim().ToLowerInvariant() ?? "";

        // B04: completed and cancelled orders cannot be updated.
        if (currentStatus is "delivered" or "cancelled")
            return;

        if (string.IsNullOrWhiteSpace(newStatus))
            return;

        var answer = MessageBox.Show(
            this,
            $"Update order #{orderId} from {currentStatus} to {newStatus}?",
            "Update Order Status",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (answer != DialogResult.Yes)
            return;

        try
        {
            var result = await _orders.ChangeStatusAsync(
                orderId,
                currentStatus,
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

            await LoadOrdersAsync();
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
    }
    private void ConfigureStatusChoices()
    {
        if (IsDisposed || !_grid.Columns.Contains("B04UpdateStatus"))
            return;

        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.IsNewRow)
                continue;

            string status = Convert.ToString(
                row.Cells["Status"].Value
            )?.Trim().ToLowerInvariant() ?? "";

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

            if (row.Cells["B04UpdateStatus"] is not DataGridViewComboBoxCell cell)
                continue;

            cell.Items.Clear();

            foreach (string choice in choices)
                cell.Items.Add(choice);

            cell.ReadOnly = choices.Length == 0;
            cell.Style.Padding = choices.Length > 0
                ? new Padding(0, 0, 62, 0)
                : Padding.Empty;

            if (choices.Length > 0)
            {
                cell.Value = choices[0];
                cell.DisplayStyle =
                    DataGridViewComboBoxDisplayStyle.DropDownButton;
            }
            else
            {
                cell.Value = null;
                cell.Style.NullValue = "—";
                cell.DisplayStyle =
                    DataGridViewComboBoxDisplayStyle.Nothing;
            }
        }
    }
    private void PaintB04InlineSave(
        object? sender,
        DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 ||
            _grid.Columns[e.ColumnIndex].Name != "B04UpdateStatus")
            return;

        string status = Convert.ToString(
            _grid.Rows[e.RowIndex].Cells["Status"].Value
        )?.Trim().ToLowerInvariant() ?? "";

        if (status is "delivered" or "cancelled")
            return;

        if (e.Graphics is null)
            return;

        e.Paint(e.CellBounds, DataGridViewPaintParts.All);

        var bounds = new Rectangle(
            e.CellBounds.Right - 60,
            e.CellBounds.Top + 5,
            54,
            Math.Max(18, e.CellBounds.Height - 10));

        System.Windows.Forms.ButtonRenderer.DrawButton(
            e.Graphics,
            bounds,
            "Save",
            _grid.Font,
            false,
            System.Windows.Forms.VisualStyles.PushButtonState.Normal);

        e.Handled = true;
    }
    private void ConfigureB04Table()
    {
        foreach (DataGridViewColumn col in _grid.Columns)
        {
            col.Visible = false;
        }

        string[] names =
        {
            "Order ID", "Customer", "Order Date",
            "Total", "Address (Ward)", "Status"
        };

        string[] headers =
        {
            "#ID", "Customer", "Date",
            "Total", "Address (Ward)", "Status"
        };

        for (int i = 0; i < names.Length; i++)
        {
            var col = _grid.Columns[names[i]];
            if (col is null)
                throw new InvalidOperationException(
                    "Missing order column: " + names[i]);

            col.Visible = true;
            col.DisplayIndex = i;
            col.HeaderText = headers[i];
            col.SortMode =
                DataGridViewColumnSortMode.Automatic;
        }

        _grid.Columns["Order ID"]!.FillWeight = 45;
        _grid.Columns["Customer"]!.FillWeight = 100;
        _grid.Columns["Order Date"]!.FillWeight = 100;
        _grid.Columns["Total"]!.FillWeight = 65;
        _grid.Columns["Address (Ward)"]!.FillWeight = 210;
        _grid.Columns["Status"]!.FillWeight = 80;

        _grid.Columns["Order Date"]!
            .DefaultCellStyle.Format = "MM/dd/yyyy HH:mm";

        _grid.Columns["Total"]!
            .DefaultCellStyle.Format = "$#,##0.00";

        _grid.Columns["Total"]!
            .DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

        // B04: always recreate the two action columns.
        foreach (string name in new[]
        {
            "B04UpdateStatus", "B04Save", "B04Detail"
        })
        {
            if (_grid.Columns.Contains(name))
                _grid.Columns.Remove(name);
        }

        var updateColumn = new DataGridViewComboBoxColumn
        {
            Name = "B04UpdateStatus",
            HeaderText = "Update Status",
            ReadOnly = false,
            FillWeight = 180,
            FlatStyle = FlatStyle.Flat,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
        };

        var detailColumn = new DataGridViewButtonColumn
        {
            Name = "B04Detail",
            HeaderText = "Detail",
            Text = "View",
            UseColumnTextForButtonValue = true,
            ReadOnly = true,
            FillWeight = 65
        };

        _grid.Columns.Add(updateColumn);
        _grid.Columns.Add(detailColumn);

        updateColumn.DisplayIndex = 6;
        detailColumn.DisplayIndex = 7;

        _grid.ReadOnly = false;

        foreach (DataGridViewColumn column in _grid.Columns)
            column.ReadOnly = column.Name != "B04UpdateStatus";

        ConfigureStatusChoices();
        _grid.CellFormatting -= FormatB04Status;
        _grid.CellFormatting += FormatB04Status;
        _grid.CellFormatting -= FormatB04Save;
        _grid.CellFormatting += FormatB04Save;
    }

    private void FormatB04Save(
        object? sender,
        DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0 ||
            _grid.Columns[e.ColumnIndex].Name != "B04Save")
            return;

        var row = _grid.Rows[e.RowIndex];

        string status = Convert.ToString(
            row.Cells["Status"].Value
        )?.Trim().ToLowerInvariant() ?? "";

        bool locked = status is "delivered" or "cancelled";

        if (locked)
        {
            e.Value = "—";
            e.FormattingApplied = true;
        }
    }
    private void FormatB04Status(
        object? sender,
        DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0 ||
            _grid.Columns[e.ColumnIndex].Name != "Status")
            return;

        string status = Convert.ToString(e.Value)?
            .Trim().ToLowerInvariant() ?? "";

        var color = status switch
        {
            "pending" => Color.FromArgb(108, 117, 125),
            "confirmed" => Color.FromArgb(13, 202, 240),
            "shipping" => Color.FromArgb(102, 16, 242),
            "delivered" => Color.FromArgb(25, 135, 84),
            "cancelled" => Color.FromArgb(220, 53, 69),
            _ => Color.Gray
        };

        var style = _grid.Rows[e.RowIndex]
            .Cells[e.ColumnIndex].Style;

        style.BackColor = color;
        style.ForeColor = Color.White;
        style.SelectionBackColor = color;
        style.SelectionForeColor = Color.White;
        style.Alignment =
            DataGridViewContentAlignment.MiddleCenter;

        e.Value = status.Length == 0
            ? ""
            : char.ToUpperInvariant(status[0]) +
              status.Substring(1);

        e.FormattingApplied = true;
    }
    private int _wardSortMode = 0;
    private void SortB04Ward()
    {
        if (_table is null)
            return;

        _wardSortMode = (_wardSortMode + 1) % 3;

        _table.DefaultView.Sort = _wardSortMode switch
        {
            1 => "[Ward] ASC, [Order Date] DESC",
            2 => "[Ward] DESC, [Order Date] DESC",
            _ => "[Order Date] DESC, [Order ID] DESC"
        };

        _state.Text = _wardSortMode switch
        {
            1 => "Sorted by Ward: A-Z",
            2 => "Sorted by Ward: Z-A",
            _ => $"{_table.DefaultView.Count} orders displayed"
        };
    }
    private void ConfigureB04AddressSuggestions()
    {
        if (_table is null)
            return;

        static AutoCompleteStringCollection BuildSuggestions(
            DataTable table, string column, bool isWard)
        {
            var collection = new AutoCompleteStringCollection();

            if (!table.Columns.Contains(column))
                return collection;

            var values = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in table.Rows)
            {
                string value =
                    Convert.ToString(row[column])?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (isWard)
                {
                    value = System.Text.RegularExpressions.Regex.Replace(
                        value,
                        @"^(Ward|Phuong|Phường)\s+",
                        "",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                else
                {
                    value = System.Text.RegularExpressions.Regex.Replace(
                        value,
                        @"^\d+[\w/-]*\s+",
                        "");
                }

                value = value.Trim();

                if (!string.IsNullOrWhiteSpace(value))
                    values.Add(value);
            }

            collection.AddRange(
                values.OrderBy(x => x).ToArray());

            return collection;
        }

        _ward.AutoCompleteMode = AutoCompleteMode.Suggest;
        _ward.AutoCompleteSource = AutoCompleteSource.CustomSource;
        _ward.AutoCompleteCustomSource =
            BuildSuggestions(_table, "Ward", true);

        _street.AutoCompleteMode = AutoCompleteMode.Suggest;
        _street.AutoCompleteSource = AutoCompleteSource.CustomSource;
        _street.AutoCompleteCustomSource =
            BuildSuggestions(_table, "Street", false);
    }
    private void ApplyFilter()
    {
        if (_table is null)
            return;

        static string Escape(string value) =>
            value.Trim()
                .Replace("'", "''")
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("*", "[*]");

        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(_search.Text))
            conditions.Add(
                $"Convert([Customer], 'System.String') LIKE '%{Escape(_search.Text)}%'");

        if (_filter.SelectedIndex > 0)
            conditions.Add(
                $"[Status] = '{Escape(_filter.Text)}'");

        if (_fromDate.Checked)
            conditions.Add(
                $"[Order Date] >= #{_fromDate.Value:MM/dd/yyyy}#");

        if (_toDate.Checked)
            conditions.Add(
                $"[Order Date] < #{_toDate.Value.Date.AddDays(1):MM/dd/yyyy}#");

        if (!string.IsNullOrWhiteSpace(_city.Text))
            conditions.Add(
                $"Convert([City], 'System.String') LIKE '%{Escape(_city.Text)}%'");

        if (!string.IsNullOrWhiteSpace(_ward.Text))
            conditions.Add(
                $"Convert([Ward], 'System.String') LIKE '%{Escape(_ward.Text)}%'");

        if (!string.IsNullOrWhiteSpace(_street.Text))
            conditions.Add(
                $"Convert([Street], 'System.String') LIKE '%{Escape(_street.Text)}%'");

        _table.DefaultView.RowFilter =
            string.Join(" AND ", conditions);

        _state.Text =
            $"{_table.DefaultView.Count} orders displayed";
    }
    private Task ShowDetailsAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            MessageBox.Show(
                this,
                "Please select an order.",
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return Task.CompletedTask;
        }

        try
        {
            int orderId = Convert.ToInt32(row["Order ID"]);

            using var form = new MetropolitanCoffee.Admin.Forms.OrderDetailForm(
                _reader, orderId);

            form.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Unable to load order details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        return Task.CompletedTask;
    }

    private async Task ConfirmOrderAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            MessageBox.Show(this,
                "Please select an order.",
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        int orderId = Convert.ToInt32(row["Order ID"]);
        string status = Convert.ToString(row["Status"]) ?? "";

        if (!string.Equals(status, "pending",
                StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this,
                "Only pending orders can be confirmed.",
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var answer = MessageBox.Show(this,
            $"Confirm order #{orderId}?",
            "Confirm Order",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer != DialogResult.Yes)
            return;

        try
        {
            var result = await _orders.UpdateStatusAsync(
                orderId, status);

            MessageBox.Show(this,
                result.Message,
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                result.Success
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning);

            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                ex.Message,
                "Order update failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }


    private async Task ShipOrderAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            MessageBox.Show(
                this,
                "Please select an order.",
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        int orderId = Convert.ToInt32(row["Order ID"]);
        string status = Convert.ToString(row["Status"]) ?? "";

        if (!string.Equals(status, "confirmed",
                StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                this,
                "Only confirmed orders can be shipped.",
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var answer = MessageBox.Show(
            this,
            $"Start shipping order #{orderId}?",
            "Ship Order",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer != DialogResult.Yes)
            return;

        try
        {
            var result = await _orders.UpdateStatusAsync(
                orderId, status);

            MessageBox.Show(
                this,
                result.Message,
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                result.Success
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning);

            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Order update failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }


    private async Task DeliverOrderAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            MessageBox.Show(
                this,
                "Please select an order.",
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        int orderId = Convert.ToInt32(row["Order ID"]);
        string status = Convert.ToString(row["Status"]) ?? "";

        if (!string.Equals(status, "shipping",
                StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                this,
                "Only shipping orders can be delivered.",
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var answer = MessageBox.Show(
            this,
            $"Mark order #{orderId} as delivered?",
            "Deliver Order",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer != DialogResult.Yes)
            return;

        try
        {
            var result = await _orders.UpdateStatusAsync(
                orderId, status);

            MessageBox.Show(
                this,
                result.Message,
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                result.Success
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning);

            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Order update failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async Task CancelOrderAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            MessageBox.Show(
                this,
                "Please select an order.",
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        int orderId = Convert.ToInt32(row["Order ID"]);
        string status = Convert.ToString(row["Status"]) ?? "";


        if (!string.Equals(status, "pending", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(status, "confirmed", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                this,
                "Only pending or confirmed orders can be cancelled.",
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        // Ask administrator for confirmation.
        var answer = MessageBox.Show(
            this,
            $"Are you sure you want to cancel order #{orderId}?\n" +
            "The inventory will be restored automatically.",
            "Cancel Order",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (answer != DialogResult.Yes)
            return;

        try
        {
            var result = await _orders.CancelOrderAsync(orderId);

            MessageBox.Show(
                this,
                result.Message,
                "Metropolitan Coffee",
                MessageBoxButtons.OK,
                result.Success
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning);

            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Order cancellation failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

}
