using System.Data;
using MetropolitanCoffee.Admin.Forms;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

public sealed class AdminPage : UserControl
{
    private readonly string _section;
    private readonly AdminReadService _reader;
    private readonly DataGridView _grid = AdminTheme.Grid();
    private readonly Label _state = AdminTheme.Label("Loading data...", 9, color: AdminTheme.Muted);
    private readonly TextBox _search = new() { Width = 230, Font = AdminTheme.Font(9), PlaceholderText = "Search..." };
    private readonly ComboBox _filter = new() { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = AdminTheme.Font(9) };
    private readonly PictureBox _preview = new()
    {
        Width = 70, Height = 52, SizeMode = PictureBoxSizeMode.Zoom,
        BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle
    };
    private DataTable? _table;
    private bool _showingMovements;

    public AdminPage(string section, AdminReadService reader)
    {
        _section = section;
        _reader = reader;
        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();
        BuildList();
        Load += async (_, _) => await LoadDataAsync();
        Disposed += (_, _) => _preview.Image?.Dispose();
    }

    private void BuildList()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, WrapContents = true,
            Padding = new Padding(0, 0, 0, 12), BackColor = AdminTheme.Canvas
        };
        root.Controls.Add(actions, 0, 0);
        _search.Height = 35;
        _search.Margin = new Padding(0, 0, 8, 0);
        _search.TextChanged += (_, _) => ApplyFilter();
        actions.Controls.Add(_search);

        if (_section is "products" or "orders" or "users" or "stock")
        {
            _filter.Items.Add("All");
            foreach (var item in _section switch
            {
                "products" => new[] { "Active", "Hidden" },
                "orders" => new[] { "pending", "confirmed", "shipping", "delivered", "cancelled" },
                "users" => new[] { "Active", "Locked" },
                _ => new[] { "Low Stock", "In Stock" }
            }) _filter.Items.Add(item);
            _filter.SelectedIndex = 0;
            _filter.SelectedIndexChanged += (_, _) => ApplyFilter();
            actions.Controls.Add(_filter);
        }

        AddAction(actions, "Refresh", async () => await LoadDataAsync());
        if (_section is "orders" or "purchases")
            AddAction(actions, "Details", async () => await ShowSelectedAsync(), true);
        if (_section == "stock")
            AddAction(actions, "Stock Movements", async () => await ToggleMovementsAsync(), true);
        if (_section == "products") actions.Controls.Add(_preview);

        var card = AdminTheme.Card();
        card.Dock = DockStyle.Fill;
        card.Margin = Padding.Empty;
        root.Controls.Add(card, 0, 1);
        _state.Dock = DockStyle.Bottom;
        _state.AutoSize = false;
        _state.Height = 24;
        card.Controls.Add(_grid);
        card.Controls.Add(_state);
        _grid.SelectionChanged += (_, _) => UpdatePreview();
        _grid.CellDoubleClick += async (_, _) => await ShowSelectedAsync();
    }

    private static void AddAction(FlowLayoutPanel panel, string text, Action action, bool primary = false)
    {
        var button = AdminTheme.Button(text, primary);
        button.Click += (_, _) => action();
        panel.Controls.Add(button);
    }

    private async Task LoadDataAsync()
    {
        _state.Text = "Loading data...";
        try
        {
            _table = await _reader.GetTableAsync(_showingMovements ? "movements" : _section);
            if (IsDisposed) return;
            _grid.DataSource = _table;
            if (_grid.Columns.Contains("Image")) _grid.Columns["Image"]!.Visible = false;
            FormatGrid();
            _state.Text = _table.Rows.Count == 0 ? "No records found." : $"{_table.Rows.Count:N0} records";
            ApplyFilter();
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            _grid.DataSource = null;
            _table = null;
            _state.Text = "Unable to load data from SQL Server. Check the connection and select Refresh.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private void FormatGrid()
    {
        foreach (DataGridViewColumn column in _grid.Columns)
        {
            if (column.ValueType == typeof(decimal)) column.DefaultCellStyle.Format = "N2";
            if (column.ValueType == typeof(DateTime)) column.DefaultCellStyle.Format = "MM/dd/yyyy HH:mm";
        }
    }

    private void ApplyFilter()
    {
        if (_table is null) return;
        var terms = new List<string>();
        var search = _search.Text.Trim().Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
        if (search.Length > 0)
        {
            var columns = _table.Columns.Cast<DataColumn>().Where(c => c.DataType == typeof(string));
            terms.Add("(" + string.Join(" OR ", columns.Select(c => $"CONVERT([{c.ColumnName.Replace("]", "]]" )}], 'System.String') LIKE '%{search}%'")) + ")");
        }
        if (_filter.Items.Count > 0 && _filter.SelectedIndex > 0)
        {
            var column = _showingMovements ? "Type" : _section == "stock" ? "Alert" : "Status";
            terms.Add($"[{column}] = '{_filter.Text.Replace("'", "''")}'");
        }
        _table.DefaultView.RowFilter = string.Join(" AND ", terms);
        _state.Text = $"{_table.DefaultView.Count:N0} / {_table.Rows.Count:N0} records";
    }

    private async Task ShowSelectedAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            MessageBox.Show(this, "Please select a row.", "Metropolitan Coffee", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var fields = row.Row.Table.Columns.Cast<DataColumn>()
            .Where(c => c.ColumnName != "Image")
            .Select(c => (c.ColumnName, Convert.ToString(row[c.ColumnName]) ?? ""))
            .ToArray();
        DataTable? items = null;
        if (_section is "orders" or "purchases")
        {
            try { items = await _reader.GetDetailItemsAsync(_section, Convert.ToInt32(row[0])); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
        }
        using var form = new DetailForm("Details", fields, items: items);
        form.ShowDialog(this);
    }

    private async Task ToggleMovementsAsync()
    {
        _showingMovements = !_showingMovements;
        _filter.Items.Clear();
        _filter.Items.Add("All");
        foreach (var value in (_showingMovements
            ? new[] { "purchase", "sale", "sale_cancel", "opening", "adjust_in", "adjust_out" }
            : new[] { "Low Stock", "In Stock" })) _filter.Items.Add(value);
        _filter.SelectedIndex = 0;
        await LoadDataAsync();
    }

    private void UpdatePreview()
    {
        if (_section != "products" || _grid.CurrentRow?.DataBoundItem is not DataRowView row) return;
        _preview.Image?.Dispose();
        _preview.Image = null;
        var name = Convert.ToString(row["Image"]);
        if (string.IsNullOrWhiteSpace(name)) return;
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "MetropolitanCoffee.Web", "wwwroot", "assets", "img", Path.GetFileName(name)));
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "MetropolitanCoffee.Web", "wwwroot")) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return;
        try { using var image = Image.FromFile(path); _preview.Image = new Bitmap(image); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
    }
}
