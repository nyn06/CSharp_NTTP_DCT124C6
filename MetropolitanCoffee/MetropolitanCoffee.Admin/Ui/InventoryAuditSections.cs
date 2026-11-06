using MetropolitanCoffee.Data.Repositories;

namespace MetropolitanCoffee.Admin.Ui;

// DES-18 and DES-22 are read-only additions.
// The four original B04 sections remain unchanged.
internal static class InventoryAuditSections
{
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("METROPOLITANCOFFEE_CONNECTION")
        ?? "Server=localhost;Database=MetropolitanCoffeeDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

    internal static void Attach(FlowLayoutPanel host)
    {
        var reader = new SqlInventoryAuditReader(ConnectionString);

        AddHistory(host, reader);
        AddReconciliation(host, reader);
    }

    private static DataGridView CreateGrid(params string[] headers)
    {
        var grid = AdminTheme.Grid();
        grid.AutoGenerateColumns = false;
        grid.RowTemplate.Height = 38;

        foreach (var header in headers)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = header,
                HeaderText = header,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                FillWeight = header == "Product" ? 160 : 100
            });
        }

        return grid;
    }

    private static (
        FlowLayoutPanel Filters,
        Label Status
    ) AddSection(
        FlowLayoutPanel host,
        string title,
        DataGridView grid)
    {
        var section = new Panel
        {
            Height = 405,
            BackColor = Color.White,
            Padding = new Padding(18),
            Margin = new Padding(0, 0, 0, 14)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 79));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        var heading = AdminTheme.Label(
            title, 13, FontStyle.Bold
        );
        heading.Dock = DockStyle.Fill;
        heading.TextAlign = ContentAlignment.MiddleLeft;

        var filters = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            BackColor = Color.White,
            Padding = new Padding(0, 4, 0, 4)
        };

        var status = AdminTheme.Label(
            "", 8.5f, color: AdminTheme.Muted
        );
        status.Dock = DockStyle.Fill;
        status.TextAlign = ContentAlignment.MiddleLeft;

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(filters, 0, 1);
        layout.Controls.Add(grid, 0, 2);
        layout.Controls.Add(status, 0, 3);

        section.Controls.Add(layout);
        host.Controls.Add(section);

        return (filters, status);
    }

    private static DateTimePicker DateInput(DateTime value) => new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "MM/dd/yyyy",
        Width = 135,
        MaxDate = DateTime.Today,
        Value = value.Date
    };

    private static Control Field(string label, Control input)
    {
        var wrapper = new Panel
        {
            Width = input.Width + 8,
            Height = 58,
            Margin = new Padding(0, 0, 12, 0)
        };

        var caption = AdminTheme.Label(label, 8.5f);
        caption.Location = new Point(0, 0);
        input.Location = new Point(0, 23);

        wrapper.Controls.Add(caption);
        wrapper.Controls.Add(input);
        return wrapper;
    }

    private static void AddHistory(
        FlowLayoutPanel host,
        SqlInventoryAuditReader reader)
    {
        var grid = CreateGrid(
            "Date / Time", "Product ID", "Product",
            "Movement", "Change", "Source", "Reference ID"
        );

        var (filters, status) = AddSection(
            host, "Stock Movement History", grid
        );

        var from = DateInput(DateTime.Today.AddYears(-1));
        var to = DateInput(DateTime.Today);
        var search = new TextBox
        {
            Width = 180,
            PlaceholderText = "Search product..."
        };

        filters.Controls.Add(Field("From *", from));
        filters.Controls.Add(Field("To *", to));
        filters.Controls.Add(Field("Product Name", search));

        var show = AdminTheme.Button("Show History", true);
        var reset = AdminTheme.Button("Reset");

        show.Click += async (_, _) =>
        {
            if (from.Value.Date > to.Value.Date)
            {
                MessageBox.Show(
                    "From date must not be later than To date.",
                    "Metropolitan Coffee",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            show.Enabled = false;

            try
            {
                var rows = await reader.GetHistoryAsync(
                    from.Value.Date,
                    to.Value.Date,
                    search.Text
                );

                if (grid.IsDisposed) return;

                grid.Rows.Clear();

                foreach (var item in rows)
                {
                    int index = grid.Rows.Add(
                        item.EventAt.ToString("yyyy-MM-dd HH:mm"),
                        item.ProductId,
                        item.Product,
                        item.MovementType,
                        item.QuantityDelta.ToString("+0;-0;0"),
                        item.Source,
                        item.ReferenceId
                    );

                    grid.Rows[index].Cells[4].Style.ForeColor =
                        item.QuantityDelta < 0
                            ? Color.FromArgb(220, 53, 69)
                            : Color.FromArgb(25, 135, 84);
                }

                status.Text = rows.Count == 0
                    ? "No movements found for this period."
                    : $"Showing {rows.Count} row(s), maximum 500. Legacy documents are not ledger transactions.";
            }
            catch (Exception ex)
            {
                if (!status.IsDisposed)
                    status.Text = $"Unable to load history: {ex.Message}";
            }
            finally
            {
                if (!show.IsDisposed) show.Enabled = true;
            }
        };

        reset.Click += (_, _) =>
        {
            from.Value = DateTime.Today.AddYears(-1);
            to.Value = DateTime.Today;
            search.Clear();
            grid.Rows.Clear();
            status.Text = "";
        };

        filters.Controls.Add(show);
        filters.Controls.Add(reset);
    }

    private static void AddReconciliation(
        FlowLayoutPanel host,
        SqlInventoryAuditReader reader)
    {
        var grid = CreateGrid(
            "Product ID", "Product", "Current Stock",
            "Ledger Balance", "Difference", "Status"
        );

        var (filters, status) = AddSection(
            host, "Stock Reconciliation", grid
        );

        var run = AdminTheme.Button("Run Check", true);
        var differencesOnly = new CheckBox
        {
            Text = "Differences only",
            AutoSize = true,
            Margin = new Padding(10, 12, 8, 0)
        };

        var results = Array.Empty<InventoryBalanceRow>();

        void DisplayResults()
        {
            if (grid.IsDisposed) return;

            grid.Rows.Clear();

            var visible = differencesOnly.Checked
                ? results.Where(x => x.Status != "Balance OK")
                : results.AsEnumerable();

            foreach (var item in visible)
            {
                int index = grid.Rows.Add(
                    item.ProductId,
                    item.Product,
                    item.CurrentStock,
                    item.LedgerBalance,
                    item.Difference,
                    item.Status
                );

                if (item.Status != "Balance OK")
                {
                    grid.Rows[index].DefaultCellStyle.BackColor =
                        Color.FromArgb(255, 243, 205);
                }
            }

            int differenceCount = results.Count(
                x => x.Status != "Balance OK"
            );

            status.Text =
                $"Checked: {results.Length} product(s). " +
                $"Differences or missing ledger: {differenceCount}. " +
                "This check compares balances, not document completeness.";
        }

        run.Click += async (_, _) =>
        {
            run.Enabled = false;

            try
            {
                var rows = await reader.GetReconciliationAsync();

                if (grid.IsDisposed) return;

                results = rows.ToArray();
                DisplayResults();
            }
            catch (Exception ex)
            {
                if (!status.IsDisposed)
                    status.Text = $"Unable to reconcile stock: {ex.Message}";
            }
            finally
            {
                if (!run.IsDisposed) run.Enabled = true;
            }
        };

        differencesOnly.CheckedChanged += (_, _) =>
        {
            DisplayResults();
        };

        filters.Controls.Add(run);
        filters.Controls.Add(differencesOnly);
    }
}