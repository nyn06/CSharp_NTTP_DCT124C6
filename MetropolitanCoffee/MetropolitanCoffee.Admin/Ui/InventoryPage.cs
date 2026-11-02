using MetropolitanCoffee.Data.Inventory;
using MetropolitanCoffee.Data.Repositories;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

public sealed class InventoryPage : UserControl
{
    private readonly SqlInventoryReader reader;
    private readonly DataGridView grid = AdminTheme.Grid();
    private readonly ComboBox mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly TextBox search = new() { PlaceholderText = "Mã, tên hoặc danh mục", Width = 180 };
    private readonly ComboBox category = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
    private readonly NumericUpDown threshold = new() { Minimum = 0, Maximum = int.MaxValue, Value = 10, Width = 75 };
    private readonly DateTimePicker from = DayPicker(), through = DayPicker();
    private readonly Label status = AdminTheme.Label("", 9);
    private readonly Button refresh = AdminTheme.Button("Tải lại");
    private InventorySnapshot? data;

    public InventoryPage(string connectionString)
    {
        reader = new(connectionString);
        mode.Items.AddRange(new object[] { "Tồn kho", "Lịch sử biến động", "Cảnh báo", "Báo cáo", "Tồn theo ngày", "Đối chiếu" });
        mode.SelectedIndex = 0;
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
        bar.Controls.AddRange(new Control[] { mode, search, category, AdminTheme.Label("Ngưỡng xem"), threshold,
            AdminTheme.Label("Từ"), from, AdminTheme.Label("Đến"), through, refresh });
        status.Dock = DockStyle.Bottom;
        Controls.Add(grid);
        Controls.Add(bar);
        Controls.Add(status);
        mode.SelectedIndexChanged += (_, _) => Render();
        category.SelectedIndexChanged += (_, _) => Render();
        search.TextChanged += (_, _) => Render();
        threshold.ValueChanged += (_, _) => Render();
        from.ValueChanged += (_, _) => Render();
        through.ValueChanged += (_, _) => Render();
        refresh.Click += async (_, _) => await ReloadAsync();
        Load += async (_, _) => await ReloadAsync();
    }

    private static DateTimePicker DayPicker() => new() { Format = DateTimePickerFormat.Short,
        Width = 110, MaxDate = DateTime.Today, Value = DateTime.Today };

    private async Task ReloadAsync()
    {
        refresh.Enabled = false;
        try
        {
            data = await reader.ReadAsync();
            if (IsDisposed) return;
            var selected = category.SelectedItem;
            category.Items.Clear();
            category.Items.Add("Tất cả danh mục");
            foreach (var name in data.Products.Select(p => p.Category).Distinct().OrderBy(n => n))
                category.Items.Add(name);
            category.SelectedIndex = selected != null && category.Items.Contains(selected) ? category.Items.IndexOf(selected) : 0;
            Render();
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            data = null;
            grid.DataSource = null;
            status.Text = "Không tải được dữ liệu: " + ex.Message;
        }
        finally { if (!IsDisposed) refresh.Enabled = true; }
    }

    private void Render()
    {
        bool dated = mode.SelectedIndex is 1 or 3;
        from.Enabled = dated;
        through.Enabled = dated || mode.SelectedIndex == 4;
        threshold.Enabled = mode.SelectedIndex is 0 or 2;
        if (data is null) return;
        grid.DataSource = null;
        try
        {
            if (dated && from.Value.Date > through.Value.Date) throw new ArgumentException("Ngày bắt đầu vượt ngày kết thúc.");
            var term = search.Text.Trim();
            var products = data.Products.Where(p => p.Id.ToString() == term ||
                p.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                p.Category.Contains(term, StringComparison.CurrentCultureIgnoreCase)).Where(p => category.SelectedIndex <= 0 || p.Category == category.Text).ToDictionary(p => p.Id);
            if (mode.SelectedIndex == 1)
            {
                grid.DataSource = data.Movements.Where(m => products.ContainsKey(m.ProductId) &&
                    m.AtUtc.ToLocalTime().Date >= from.Value.Date && m.AtUtc.ToLocalTime().Date <= through.Value.Date)
                    .Select(m => new { Thời_điểm = m.AtUtc.ToLocalTime(), Sản_phẩm = products[m.ProductId].Name,
                        Loại = m.Type, Thay_đổi = m.Delta, Dòng_đơn = m.OrderItemId, Dòng_phiếu = m.PurchaseItemId, Ghi_chú = m.Note }).ToList();
            }
            else if (mode.SelectedIndex == 3)
            {
                grid.DataSource = InventoryAnalysis.Report(data, from.Value, through.Value).Where(r => products.ContainsKey(r.ProductId))
                    .Select(r => new { Mã = r.ProductId, Sản_phẩm = r.Product, Danh_mục = r.Category, Đầu_kỳ = r.Opening,
                        Nhập = r.Imported, Xuất = r.Exported, Cuối_kỳ = r.Closing, Ước_tính_lịch_sử = r.Estimated }).ToList();
            }
            else if (mode.SelectedIndex == 4)
            {
                grid.DataSource = InventoryAnalysis.Report(data, through.Value, through.Value).Where(r => products.ContainsKey(r.ProductId))
                    .Select(r => new { Mã = r.ProductId, Sản_phẩm = r.Product, Danh_mục = r.Category, Tồn_cuối_ngày = r.Closing,
                        Ước_tính = r.Estimated }).ToList();
            }
            else if (mode.SelectedIndex == 5)
            {
                grid.DataSource = InventoryAnalysis.Reconcile(data).Where(r => products.ContainsKey(r.ProductId))
                    .OrderByDescending(r => r.Expected != r.Actual)
                    .Select(r => new { Mã = r.ProductId, Sản_phẩm = r.Product, Kiểm_tra = r.Check, Chứng_từ = r.Detail,
                        Dự_kiến = r.Expected, Thực_tế = r.Actual, Kết_quả = r.Expected == r.Actual ? "Khớp" : "Lệch" }).ToList();
            }
            else
            {
                var rows = products.Values.Select(p => new { Mã = p.Id, Sản_phẩm = p.Name, Danh_mục = p.Category,
                    Tồn = p.Quantity, Cảnh_báo = InventoryAnalysis.LowStock(p, (int)threshold.Value) });
                grid.DataSource = rows.Where(p => mode.SelectedIndex != 2 || p.Cảnh_báo).OrderBy(p => p.Tồn).ToList();
            }
            var active = data.Products.Where(p => p.Active).ToList();
            status.Text = $"{grid.Rows.Count} dòng · Toàn kho đang bán: {active.Count} SP · Tồn: {active.Sum(p => (long)p.Quantity)}" +
                $" · Hết: {active.Count(p => p.Quantity == 0)} · Sắp hết: {active.Count(p => p.Quantity > 0 && p.Quantity <= threshold.Value)}";
        }
        catch (ArgumentException ex) { status.Text = ex.Message; }
    }
}
