using System.Data;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

public sealed class OrdersPage : UserControl
{
    private readonly AdminReadService _reader;
    private readonly AdminOrderService _orders = Program.Orders;
    private readonly DataGridView _grid = AdminTheme.Grid();
    private readonly Label _count = AdminTheme.Label("Loading orders...", 9);
    private readonly DateTimePicker _from = DatePicker(), _to = DatePicker();
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _city = new() { Text = "Ho Chi Minh" };
    private readonly TextBox _ward = new() { PlaceholderText = "Ward" };
    private readonly TextBox _street = new() { PlaceholderText = "Street Name" };
    private readonly Button _sort = new() { Text = "Sort ↕", Width = 78, Height = 28 };
    private readonly Dictionary<int, (ComboBox Choice, Button Save)> _editors = new();
    private readonly System.Windows.Forms.Timer _refresh = new() { Interval = 15000 };
    private DataTable? _table;
    private int _sortMode;
    private bool _loading, _saving, _configuringColumns;

    private static DateTimePicker DatePicker() => new()
    {
        Format = DateTimePickerFormat.Custom, CustomFormat = "MM/dd/yyyy",
        ShowCheckBox = true, Checked = false, MaxDate = DateTime.Today
    };

    public OrdersPage(AdminReadService reader)
    {
        _reader = reader;
        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();
        BuildInterface();
        _refresh.Tick += async (_, _) =>
        {
            if (IsDisposed || !Visible || _loading || _saving || ContainsFocus) return;
            await LoadOrdersAsync();
        };
        Load += async (_, _) =>
        {
            await LoadOrdersAsync();
            if (!IsDisposed) _refresh.Start();
        };
        VisibleChanged += async (_, _) =>
        {
            if (!Visible) _refresh.Stop();
            else if (IsHandleCreated && _table is not null)
            {
                await LoadOrdersAsync();
                if (!IsDisposed) _refresh.Start();
            }
        };
        Disposed += (_, _) => _refresh.Dispose();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
            Padding = new Padding(0, 0, 0, 8)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 151));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var title = AdminTheme.Card();
        title.Dock = DockStyle.Fill;
        title.Margin = new Padding(0, 0, 0, 12);
        title.Padding = new Padding(18, 12, 18, 12);
        var heading = AdminTheme.Label("Orders Management", 14, FontStyle.Bold);
        heading.Dock = DockStyle.Fill;
        heading.TextAlign = ContentAlignment.MiddleLeft;
        title.Controls.Add(heading);
        root.Controls.Add(title, 0, 0);

        var filterCard = AdminTheme.Card();
        filterCard.Dock = DockStyle.Fill;
        filterCard.Margin = new Padding(0, 0, 0, 12);
        filterCard.Padding = new Padding(16, 12, 16, 12);
        filterCard.AutoScroll = false;
        root.Controls.Add(filterCard, 0, 1);
        var filters = new TableLayoutPanel
        {
            ColumnCount = 6, RowCount = 3, Width = 900, Height = 108,
            Margin = Padding.Empty
        };
        for (int i = 0; i < 6; i++)
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6));
        filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        filterCard.Controls.Add(filters);
        filterCard.Resize += (_, _) =>
        {
            int available = filterCard.ClientSize.Width - filterCard.Padding.Horizontal;
            filterCard.AutoScroll = available < 920;
            filters.Width = Math.Max(900, available - 20);
        };
        var fields = new (string Name, Control Input)[]
        {
            ("From Date", _from), ("To Date", _to), ("Status", _status),
            ("City", _city), ("Ward", _ward), ("Street Name", _street)
        };
        for (int i = 0; i < fields.Length; i++)
        {
            var label = AdminTheme.Label(fields[i].Name, 9);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Margin = new Padding(0, 0, 10, 0);
            fields[i].Input.Dock = DockStyle.Fill;
            fields[i].Input.Margin = new Padding(0, 0, 10, 0);
            filters.Controls.Add(label, i, 0);
            filters.Controls.Add(fields[i].Input, i, 1);
        }
        _status.Items.AddRange(new object[]
        {
            "All Status", "Pending", "Confirmed", "Shipping", "Delivered", "Cancelled"
        });
        _status.SelectedIndex = 0;
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, WrapContents = false,
            Margin = Padding.Empty, Padding = new Padding(0, 8, 0, 0)
        };
        filters.Controls.Add(actions, 0, 2);
        filters.SetColumnSpan(actions, 6);
        var filter = AdminTheme.Button("Filter", true);
        filter.Click += (_, _) => ApplyFilter(true);
        actions.Controls.Add(filter);
        var reset = AdminTheme.Button("Reset");
        reset.Click += (_, _) =>
        {
            _from.Checked = _to.Checked = false;
            _status.SelectedIndex = 0;
            _city.Text = "Ho Chi Minh";
            _ward.Clear();
            _street.Clear();
            _sortMode = 0;
            ApplyFilter(false);
        };
        actions.Controls.Add(reset);
        var refresh = AdminTheme.Button("Refresh");
        refresh.Click += async (_, _) => await LoadOrdersAsync();
        actions.Controls.Add(refresh);

        var tableCard = AdminTheme.Card();
        tableCard.Dock = DockStyle.Fill;
        tableCard.Margin = Padding.Empty;
        tableCard.Padding = new Padding(16);
        root.Controls.Add(tableCard, 0, 2);
        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
            Margin = Padding.Empty, Padding = Padding.Empty
        };
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
        tableCard.Controls.Add(tableLayout);
        _grid.Dock = DockStyle.Fill;
        _grid.Margin = Padding.Empty;
        tableLayout.Controls.Add(_grid, 0, 0);
        var footer = new Panel { Dock = DockStyle.Fill, Height = 29 };
        _count.Dock = DockStyle.Fill;
        _count.TextAlign = ContentAlignment.MiddleLeft;
        footer.Controls.Add(_count);
        tableLayout.Controls.Add(footer, 0, 1);
        _sort.FlatStyle = FlatStyle.Flat;
        _sort.BackColor = Color.FromArgb(238, 234, 19);
        _sort.ForeColor = Color.Black;
        _sort.FlatAppearance.BorderSize = 0;
        _sort.Click += (_, _) => { _sortMode = (_sortMode + 1) % 3; UpdateSort(); };
        _sort.Visible = false;
        _grid.Controls.Add(_sort);
        _grid.ReadOnly = true;
        _grid.AutoGenerateColumns = false;
        _grid.ColumnHeadersVisible = true;
        _grid.ColumnHeadersHeight = 44;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.ScrollBars = ScrollBars.Both;
        _grid.RowTemplate.Height = 54;
        _grid.CellPainting += PaintCell;
        _grid.CellClick += async (_, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                _grid.Columns[e.ColumnIndex].Name == "B04Detail")
                await ShowDetailsAsync(OrderId(e.RowIndex));
        };
        _grid.Scroll += (_, _) => LayoutEditors();
        _grid.Resize += (_, _) => LayoutEditors();
        _grid.ColumnWidthChanged += (_, _) => LayoutEditors();
        _grid.ColumnDisplayIndexChanged += (_, _) => LayoutEditors();
        _grid.DataBindingComplete += (_, _) => RebuildEditors();
        ConfigureTable();
    }

    private async Task LoadOrdersAsync()
    {
        if (_loading || IsDisposed) return;
        _loading = true;
        try
        {
            _table = await _reader.GetTableAsync("orders");
            if (IsDisposed) return;
            _table.Columns.Add("B04 Address", typeof(string));
            foreach (DataRow row in _table.Rows)
                row["B04 Address"] = string.Join(", ", new[]
                {
                    Convert.ToString(row["Street"]), Convert.ToString(row["Ward"]),
                    Convert.ToString(row["City"])
                }.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()));
            SetSuggestions(_ward, "Ward");
            SetSuggestions(_street, "Street");
            _grid.DataSource = _table;
            ApplyFilter(false);
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            _count.Text = "Unable to load orders.";
            MessageBox.Show(this, ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _loading = false; }
    }

    private void SetSuggestions(TextBox input, string column)
    {
        if (_table is null) return;
        var source = new AutoCompleteStringCollection();
        source.AddRange(_table.Rows.Cast<DataRow>()
            .Select(row => Convert.ToString(row[column])?.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .Select(value => value!)
            .ToArray());
        input.AutoCompleteMode = AutoCompleteMode.Suggest;
        input.AutoCompleteSource = AutoCompleteSource.CustomSource;
        input.AutoCompleteCustomSource = source;
    }

    private void ConfigureTable()
    {
        if (_grid.Columns.Count != 0) return;
        _configuringColumns = true;
        try
        {
            // Name is the UI key; DataPropertyName is the DataTable key.
            // Only B04 Address is computed locally. The other values come from SQL aliases.
            var columns = new (string Name, string DataProperty, string Header, int Width)[]
            {
                ("Order ID", "Order ID", "#ID", 62),
                ("Customer", "Customer", "Customer", 145),
                ("Order Date", "Order Date", "Date", 128),
                ("Total", "Total", "Total", 105),
                ("B04 Address", "B04 Address", "Address (Ward)", 300),
                ("Status", "Status", "Status", 105)
            };
            foreach (var column in columns)
                _grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = column.Name, DataPropertyName = column.DataProperty,
                    HeaderText = column.Header, MinimumWidth = column.Width,
                    FillWeight = column.Width, ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "B04Update", HeaderText = "Update Status",
                MinimumWidth = 230, FillWeight = 230, ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            _grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "B04Detail", HeaderText = "Detail", Text = "View",
                UseColumnTextForButtonValue = true, MinimumWidth = 85, FillWeight = 85,
                FlatStyle = FlatStyle.Flat, SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }
        finally { _configuringColumns = false; }
        Column("Order Date").DefaultCellStyle.Format = "MM/dd/yyyy HH:mm";
        Column("Total").DefaultCellStyle.Format = "$#,##0.00";
        Column("Total").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        Column("B04 Address").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        Column("B04 Address").DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        Column("B04 Address").HeaderCell.Style.Alignment =
            DataGridViewContentAlignment.MiddleLeft;
        Column("B04 Address").HeaderCell.Style.Padding = new Padding(8, 0, 78, 0);
        Column("Order ID").DefaultCellStyle.ForeColor = AdminTheme.Text;
        _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(33, 37, 41);
        _grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
        _grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grid.GridColor = AdminTheme.Border;
        foreach (DataGridViewRow row in _grid.Rows) row.Height = 54;
        RebuildEditors();
    }

    private DataGridViewColumn Column(string name) => _grid.Columns[name]
        ?? throw new InvalidOperationException($"Orders grid is missing column '{name}'.");

    private void ApplyFilter(bool validate)
    {
        if (_table is null) return;
        if (validate && _from.Checked && _to.Checked && _from.Value.Date > _to.Value.Date)
        {
            MessageBox.Show(this, "From Date must not be after To Date.", "Invalid date range",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        static string Escape(string value) => value.Trim().Replace("'", "''")
            .Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
        var conditions = new List<string>();
        if (_from.Checked) conditions.Add($"[Order Date] >= #{_from.Value:MM/dd/yyyy}#");
        if (_to.Checked) conditions.Add($"[Order Date] < #{_to.Value.Date.AddDays(1):MM/dd/yyyy}#");
        if (_status.SelectedIndex > 0) conditions.Add($"[Status] = '{Escape(_status.Text)}'");
        // B04 shows this city by default. Avoid hiding other SQL Server orders until edited.
        if (_city.Text.Trim().Length > 0 &&
            !string.Equals(_city.Text.Trim(), "Ho Chi Minh", StringComparison.OrdinalIgnoreCase))
            conditions.Add($"Convert([City], 'System.String') LIKE '%{Escape(_city.Text)}%'");
        if (_ward.Text.Trim().Length > 0)
            conditions.Add($"Convert([Ward], 'System.String') LIKE '%{Escape(_ward.Text)}%'");
        if (_street.Text.Trim().Length > 0)
        {
            string q = Escape(_street.Text);
            conditions.Add($"(Convert([Street], 'System.String') LIKE '%{q}%' OR " +
                $"Convert([Customer], 'System.String') LIKE '%{q}%' OR " +
                $"Convert([Username], 'System.String') LIKE '%{q}%')");
        }
        _table.CaseSensitive = false;
        _table.DefaultView.RowFilter = string.Join(" AND ", conditions);
        UpdateSort();
        _count.Text = _table.DefaultView.Count == 0 && conditions.Count > 0
            ? "No matching orders found" : $"{_table.DefaultView.Count} orders displayed";
        RebuildEditors();
    }

    private void UpdateSort()
    {
        if (_table is null) return;
        _table.DefaultView.Sort = _sortMode switch
        {
            1 => "[Ward] ASC, [Order ID] DESC",
            2 => "[Ward] DESC, [Order ID] DESC",
            _ => "[Order Date] DESC, [Order ID] DESC"
        };
        _sort.Text = _sortMode switch { 1 => "Sort A↓Z", 2 => "Sort Z↓A", _ => "Sort ↕" };
        RebuildEditors();
    }

    private int OrderId(int index) => _grid.Rows[index].DataBoundItem is DataRowView row
        ? Convert.ToInt32(row["Order ID"]) : 0;

    private void RebuildEditors()
    {
        foreach (var (choice, save) in _editors.Values)
        {
            _grid.Controls.Remove(choice);
            _grid.Controls.Remove(save);
            choice.Dispose();
            save.Dispose();
        }
        _editors.Clear();
        if (_configuringColumns || _grid.IsDisposed ||
            _grid.Columns["B04Update"] is null ||
            _grid.Columns["Status"] is null) return;
        for (int i = 0; i < _grid.Rows.Count; i++)
        {
            if (_grid.Rows[i].DataBoundItem is not DataRowView row) continue;
            string current = Convert.ToString(row["Status"])?.Trim().ToLowerInvariant() ?? "";
            string[] choices = current switch
            {
                "pending" => ["confirmed", "shipping", "delivered", "cancelled"],
                "confirmed" => ["shipping", "delivered", "cancelled"],
                "shipping" => ["delivered"],
                _ => []
            };
            if (choices.Length == 0) continue;
            int orderId = Convert.ToInt32(row["Order ID"]);
            var select = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, Font = AdminTheme.Font(9),
                IntegralHeight = false
            };
            foreach (string choice in choices) select.Items.Add(choice);
            select.SelectedIndex = 0;
            var save = new Button
            {
                Text = "Save", Font = AdminTheme.Font(9), FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(33, 37, 41), ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            save.FlatAppearance.BorderSize = 0;
            save.Click += async (_, _) => await SaveAsync(orderId, current, select, save);
            _grid.Controls.Add(select);
            _grid.Controls.Add(save);
            _editors.Add(orderId, (select, save));
        }
        LayoutEditors();
    }

    private void LayoutEditors()
    {
        if (_grid.IsDisposed || _configuringColumns) return;
        var updateColumn = _grid.Columns["B04Update"];
        var addressColumn = _grid.Columns["B04 Address"];
        if (updateColumn is null || addressColumn is null)
        {
            _sort.Visible = false;
            foreach (var (choice, save) in _editors.Values)
                choice.Visible = save.Visible = false;
            return;
        }
        Rectangle addressHeader = _grid.GetCellDisplayRectangle(addressColumn.Index, -1, false);
        _sort.Visible = _grid.ColumnHeadersVisible &&
            addressHeader.Width >= 205 && addressHeader.Height >= 28;
        if (_sort.Visible)
        {
            _sort.SetBounds(addressHeader.Right - 73, addressHeader.Top + 8, 66, 28);
            _sort.BringToFront();
        }
        int column = updateColumn.Index;
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (!_editors.TryGetValue(OrderId(row.Index), out var controls)) continue;
            Rectangle cell = _grid.GetCellDisplayRectangle(column, row.Index, false);
            bool visible = row.Visible && cell.Width >= 215 && cell.Height >= 30 &&
                cell.Bottom > _grid.ColumnHeadersHeight && cell.Top < _grid.ClientSize.Height;
            controls.Choice.Visible = controls.Save.Visible = visible;
            if (!visible) continue;
            int top = cell.Top + (cell.Height - 29) / 2;
            controls.Choice.SetBounds(cell.Left + 8, top, 137, 29);
            controls.Save.SetBounds(cell.Left + 151, top, 66, 29);
            controls.Choice.BringToFront();
            controls.Save.BringToFront();
        }
    }

    private async Task SaveAsync(int orderId, string current, ComboBox choice, Button save)
    {
        if (_saving || choice.SelectedItem is not string next) return;
        if (MessageBox.Show(this, $"Update order #{orderId} from {current} to {next}?",
                "Update Order Status", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        _saving = true;
        save.Enabled = false;
        try
        {
            var result = await _orders.ChangeStatusAsync(orderId, current, next);
            if (IsDisposed) return;
            MessageBox.Show(this, result.Message, "Metropolitan Coffee", MessageBoxButtons.OK,
                result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
                MessageBox.Show(this, ex.Message, "Order update failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _saving = false;
            if (!save.IsDisposed) save.Enabled = true;
        }
    }

    private async Task ShowDetailsAsync(int orderId)
    {
        if (orderId <= 0) return;
        try
        {
            using var form = new MetropolitanCoffee.Admin.Forms.OrderDetailForm(_reader, orderId);
            form.ShowDialog(this);
            if (!IsDisposed) await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Unable to load order details", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PaintCell(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics is null) return;
        string name = _grid.Columns[e.ColumnIndex].Name;
        if (name == "Status")
        {
            string status = Convert.ToString(e.Value)?.Trim().ToLowerInvariant() ?? "";
            Color color = status switch
            {
                "confirmed" => Color.FromArgb(13, 202, 240),
                "shipping" => Color.FromArgb(13, 110, 253),
                "delivered" => Color.FromArgb(25, 135, 84),
                "cancelled" => Color.FromArgb(220, 53, 69),
                _ => Color.FromArgb(108, 117, 125)
            };
            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
            if (status.Length > 0)
            {
                string caption = char.ToUpperInvariant(status[0]) + status[1..];
                var badge = new Rectangle(e.CellBounds.Left + 7,
                    e.CellBounds.Top + (e.CellBounds.Height - 24) / 2,
                    e.CellBounds.Width - 14, 24);
                using var brush = new SolidBrush(color);
                e.Graphics.FillRectangle(brush, badge);
                TextRenderer.DrawText(e.Graphics, caption, _grid.Font, badge, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            e.Handled = true;
        }
        else if (name == "B04Update")
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
            if (_grid.Rows[e.RowIndex].DataBoundItem is DataRowView row &&
                Convert.ToString(row["Status"])?.Trim().ToLowerInvariant() is "delivered" or "cancelled")
                TextRenderer.DrawText(e.Graphics, "—", _grid.Font, e.CellBounds, AdminTheme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            e.Handled = true;
        }
    }
}
