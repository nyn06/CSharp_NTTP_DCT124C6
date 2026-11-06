namespace MetropolitanCoffee.Admin.Ui;

internal static class AdminTheme
{
    internal static readonly Color Chrome = Color.FromArgb(130, 125, 121); // B04 header and sidebar: #827d79
    internal static readonly Color ChromeHover = Color.FromArgb(111, 106, 102);
    internal static readonly Color Canvas = Color.FromArgb(244, 246, 249); // B04 content: #f4f6f9
    internal static readonly Color Border = Color.FromArgb(221, 221, 221);
    internal static readonly Color Text = Color.FromArgb(33, 37, 41);
    internal static readonly Color Muted = Color.FromArgb(108, 117, 125);

    internal static Font Font(float size = 10, FontStyle style = FontStyle.Regular)
        => new("Arial", size, style);

    internal static Label Label(string text, float size = 10, FontStyle style = FontStyle.Regular, Color? color = null)
        => new() { Text = text, Font = Font(size, style), ForeColor = color ?? Text, AutoSize = true };

    internal static Panel Card() => new() { BackColor = Color.White, Padding = new Padding(18) };

    internal static Button Button(string text, bool primary = false)
    {
        var button = new Button
        {
            Text = text, Font = Font(9.5f, FontStyle.Bold), Height = 35,
            AutoSize = true, MinimumSize = new Size(88, 35),
            Padding = new Padding(12, 0, 12, 0), FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Color.FromArgb(74, 64, 58) : Color.White,
            ForeColor = primary ? Color.White : Text, Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };
        button.FlatAppearance.BorderColor = primary ? Color.FromArgb(74, 64, 58) : Border;
        return button;
    }

    internal static DataGridView Grid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
            MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false, ColumnHeadersHeight = 42, RowTemplate = { Height = 38 },
            EnableHeadersVisualStyles = false, GridColor = Border, Font = Font(9),
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 37, 41);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = Font(9, FontStyle.Bold);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Text;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 228, 232);
        grid.DefaultCellStyle.SelectionForeColor = Text;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        return grid;
    }
}
