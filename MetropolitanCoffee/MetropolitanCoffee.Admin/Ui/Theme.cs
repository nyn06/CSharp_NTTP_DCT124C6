namespace MetropolitanCoffee.Admin.Ui;

internal static class Theme
{
    internal static readonly Color Coffee = Color.FromArgb(65, 41, 30);
    internal static readonly Color CoffeeLight = Color.FromArgb(109, 72, 52);
    internal static readonly Color Cream = Color.FromArgb(250, 246, 240);
    internal static readonly Color Canvas = Color.FromArgb(245, 242, 237);
    internal static readonly Color Border = Color.FromArgb(225, 216, 205);
    internal static readonly Color Muted = Color.FromArgb(115, 104, 95);
    internal static readonly Color Accent = Color.FromArgb(183, 126, 77);

    internal static Font Font(float size = 10, FontStyle style = FontStyle.Regular) => new("Segoe UI", size, style);

    internal static Label Label(string text, float size = 10, FontStyle style = FontStyle.Regular, Color? color = null)
        => new() { Text = text, Font = Font(size, style), ForeColor = color ?? Coffee, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };

    internal static Button Button(string text, bool primary = false)
        => new()
        {
            Text = text, Font = Font(9.5f, FontStyle.Bold), Height = 38, AutoSize = true,
            MinimumSize = new Size(112, 38), Padding = new Padding(12, 0, 12, 0),
            FlatStyle = FlatStyle.Flat, BackColor = primary ? Coffee : Color.White,
            ForeColor = primary ? Color.White : Coffee, Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };

    internal static Panel Card() => new() { BackColor = Color.White, Padding = new Padding(20), Margin = new Padding(0, 0, 0, 16) };

    internal static DataGridView Grid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
            MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false, ColumnHeadersHeight = 44, RowTemplate = { Height = 40 },
            EnableHeadersVisualStyles = false, GridColor = Border, Font = Font(9),
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Cream;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Coffee;
        grid.ColumnHeadersDefaultCellStyle.Font = Font(9, FontStyle.Bold);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Coffee;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 222, 209);
        grid.DefaultCellStyle.SelectionForeColor = Coffee;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(253, 251, 248);
        return grid;
    }
}
