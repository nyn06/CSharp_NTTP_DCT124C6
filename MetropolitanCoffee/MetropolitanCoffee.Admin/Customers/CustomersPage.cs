
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MetropolitanCoffee.Data.Repositories;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Customers;

public sealed class CustomersPage : UserControl
{
    private readonly TextBox _username = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new();
    private readonly TextBox _confirmPassword = new();

    private readonly Button _addButton = new();
    private readonly TextBox _search = new();
    private readonly ComboBox _filter = new();
    private readonly DataGridView _grid = new();

    private readonly Button _resetButton = new();
    private readonly Button _lockButton = new();
    private readonly Button _unlockButton = new();

    private DataTable? _table;

    private readonly SqlCustomerManagementRepository _repository;
    private readonly CustomerManagementService _service;
    private bool _busy;


    public CustomersPage(string connectionString)
    {
        _repository = new SqlCustomerManagementRepository(connectionString);
        _service = new CustomerManagementService(_repository);

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(245, 246, 249);
        Font = new Font("Segoe UI", 9);

        BuildInterface();

        _addButton.Click += async (_, _) => await AddCustomerAsync();
        _resetButton.Click += async (_, _) => await ChangeCustomerAsync("reset");
        _lockButton.Click += async (_, _) => await ChangeCustomerAsync("lock");
        _unlockButton.Click += async (_, _) => await ChangeCustomerAsync("unlock");

        _ = LoadCustomersAsync();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20),
            AutoScroll = true
        };

        root.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildAddUserPanel(), 0, 0);
        root.Controls.Add(BuildUserListPanel(), 0, 1);

        Controls.Add(root);
    }

    private Control BuildAddUserPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 4,
            BackColor = Color.White,
            Padding = new Padding(15)
        };

        panel.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50));

        var title = new Label
        {
            Text = "Add New User",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true
        };

        panel.Controls.Add(title, 0, 0);
        panel.SetColumnSpan(title, 2);

        AddField(panel, "Username", _username, 1, 0);
        AddField(panel, "Email", _email, 1, 1);
        AddField(panel, "Password", _password, 2, 0);
        AddField(panel, "Confirm Password",
            _confirmPassword, 2, 1);

        _password.UseSystemPasswordChar = true;
        _confirmPassword.UseSystemPasswordChar = true;

        StyleButton(_addButton, "Add User");
        _addButton.Margin = new Padding(5, 12, 5, 5);
        panel.Controls.Add(_addButton, 0, 3);

        return panel;
    }

    private Control BuildUserListPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.White,
            Padding = new Padding(15),
            Margin = new Padding(0, 15, 0, 0)
        };

        panel.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "User List",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 15)
        };
        panel.Controls.Add(title, 0, 0);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        _search.Width = 230;
        _search.PlaceholderText = "Search users...";
        _search.Margin = new Padding(0, 0, 10, 5);
        _search.TextChanged += (_, _) => ApplyFilter();

        _filter.Width = 150;
        _filter.DropDownStyle =
            ComboBoxStyle.DropDownList;
        _filter.Items.AddRange(
            new object[] { "All", "Active", "Locked" });
        _filter.SelectedIndex = 0;
        _filter.SelectedIndexChanged +=
            (_, _) => ApplyFilter();

        toolbar.Controls.Add(_search);
        toolbar.Controls.Add(_filter);
        panel.Controls.Add(toolbar, 0, 1);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.RowHeadersVisible = false;
        _grid.AutoGenerateColumns = true;

        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle.BackColor =
            Color.FromArgb(64, 51, 45);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font =
            new Font("Segoe UI", 9, FontStyle.Bold);
        _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor =
            Color.FromArgb(64, 51, 45);
        _grid.ColumnHeadersDefaultCellStyle.SelectionForeColor =
            Color.White;
        _grid.ColumnHeadersHeight = 42;

        _grid.SelectionChanged +=
            (_, _) => UpdateActionButtons();

        panel.Controls.Add(_grid, 0, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Padding = new Padding(0, 12, 0, 0)
        };

        StyleButton(_resetButton, "Reset Password");
        StyleButton(_lockButton, "Lock");
        StyleButton(_unlockButton, "Unlock");
        _lockButton.BackColor = Color.FromArgb(180, 83, 9);
        _unlockButton.BackColor = Color.FromArgb(21, 128, 61);

        actions.Controls.Add(_resetButton);
        actions.Controls.Add(_lockButton);
        actions.Controls.Add(_unlockButton);

        panel.Controls.Add(actions, 0, 3);

        UpdateActionButtons();

        return panel;
    }

    private static void AddField(
        TableLayoutPanel panel,
        string label,
        TextBox input,
        int row,
        int column)
    {
        var container = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(5)
        };

        container.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Font = new Font(
                "Segoe UI", 9, FontStyle.Bold)
        });

        input.Width = 250;
        input.Height = 30;
        container.Controls.Add(input);

        panel.Controls.Add(container, column, row);
    }

    private static void StyleButton(
        Button button, string text)
    {
        button.Text = text;
        button.AutoSize = true;
        button.MinimumSize = new Size(100, 36);
        button.BackColor = Color.FromArgb(110, 95, 85);
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Margin = new Padding(0, 0, 10, 5);
    }

    private void ApplyFilter()
    {
        if (_table == null) return;

        var search = _search.Text.Trim()
            .Replace("'", "''")
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("*", "[*]");

        var terms = new List<string>();

        if (search.Length > 0)
        {
            var columns = _table.Columns
                .Cast<DataColumn>()
                .Where(c => c.DataType == typeof(string));

            var conditions = columns.Select(c =>
                $"[{c.ColumnName.Replace("]", "]]")}] " +
                $"LIKE '%{search}%'");

            var condition = string.Join(" OR ", conditions);
            if (condition.Length > 0)
                terms.Add("(" + condition + ")");
        }

        if (_filter.SelectedIndex > 0 &&
            _table.Columns.Contains("Status"))
        {
            terms.Add(
                $"[Status] = '{_filter.Text}'");
        }

        _table.DefaultView.RowFilter =
            string.Join(" AND ", terms);
    }
    private void UpdateActionButtons()
    {
        string? status = GetSelectedStatus();

        bool canLock = !_busy && status == "Active";
        bool canUnlock = !_busy && status == "Locked";

        _resetButton.Enabled = !_busy && status != null;

        _lockButton.Enabled = canLock;
        _unlockButton.Enabled = canUnlock;

        _lockButton.BackColor = canLock
            ? Color.FromArgb(180, 83, 9)
            : Color.FromArgb(225, 225, 225);

        _unlockButton.BackColor = canUnlock
            ? Color.FromArgb(21, 128, 61)
            : Color.FromArgb(225, 225, 225);

        _lockButton.ForeColor = canLock
            ? Color.White
            : Color.FromArgb(90, 90, 90);

        _unlockButton.ForeColor = canUnlock
            ? Color.White
            : Color.FromArgb(90, 90, 90);
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            _table = await _repository.GetCustomersAsync();
            _grid.DataSource = _table.DefaultView;
            _grid.ClearSelection();

            ApplyFilter();
            UpdateActionButtons();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Không thể tải danh sách khách hàng:\n" + ex.Message,
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async Task AddCustomerAsync()
    {
        if (_busy) return;

        try
        {
            _busy = true;
            _addButton.Enabled = false;

            var result = await _service.AddCustomerAsync(
                _username.Text,
                _email.Text,
                _password.Text,
                _confirmPassword.Text);

            MessageBox.Show(
                result.Message,
                result.Success ? "Thông báo" : "Kiểm tra dữ liệu",
                MessageBoxButtons.OK,
                result.Success
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning);

            if (!result.Success) return;

            _username.Clear();
            _email.Clear();
            _password.Clear();
            _confirmPassword.Clear();

            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Không thể thêm khách hàng:\n" + ex.Message,
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _busy = false;
            _addButton.Enabled = true;
            UpdateActionButtons();
        }
    }

    private int? GetSelectedCustomerId()
    {
        if (_grid.SelectedRows.Count == 0)
            return null;

        var row = _grid.SelectedRows[0];

        if (row.DataBoundItem is not DataRowView data)
            return null;

        return Convert.ToInt32(data["User ID"]);
    }

    private string? GetSelectedStatus()
    {
        if (_grid.SelectedRows.Count == 0)
            return null;

        if (_grid.SelectedRows[0].DataBoundItem
            is not DataRowView data)
            return null;

        return Convert.ToString(data["Status"]);
    }

    private async Task ChangeCustomerAsync(string action)
    {
        if (_busy) return;

        int? customerId = GetSelectedCustomerId();

        if (customerId == null)
        {
            MessageBox.Show("Vui lòng chọn một khách hàng.");
            return;
        }

        string message = action switch
        {
            "lock" => "Bạn có chắc muốn khóa tài khoản này?",
            "unlock" => "Bạn có chắc muốn mở khóa tài khoản này?",
            "reset" => "Đặt lại mật khẩu tài khoản này thành 123456?",
            _ => ""
        };

        if (message.Length == 0) return;

        var confirm = MessageBox.Show(
            message,
            "Xác nhận",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        try
        {
            _busy = true;
            _addButton.Enabled = false;
            UpdateActionButtons();

            (bool Success, string Message) result = action switch
            {
                "lock" => await _service.LockCustomerAsync(customerId.Value),
                "unlock" => await _service.UnlockCustomerAsync(customerId.Value),
                "reset" => await _service.ResetPasswordAsync(customerId.Value),
                _ => (false, "Thao tác không hợp lệ.")
            };

            MessageBox.Show(
                result.Message,
                "Thông báo",
                MessageBoxButtons.OK,
                result.Success
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning);

            if (result.Success)
                await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Thao tác thất bại:\n" + ex.Message,
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _busy = false;
            _addButton.Enabled = true;
            UpdateActionButtons();
        }
    }
}
