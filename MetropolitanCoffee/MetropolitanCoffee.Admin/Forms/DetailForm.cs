using MetropolitanCoffee.Admin.Ui;

using System.Data;

namespace MetropolitanCoffee.Admin.Forms;

public sealed class DetailForm : Form
{
    public DetailForm(string title, (string Label, string Value)[] fields, bool editable = false, DataTable? items = null)
    {
        Text = title + " · Metropolitan Coffee";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(600, 570);
        MinimumSize = new Size(520, 430);
        BackColor = AdminTheme.Canvas;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = AdminTheme.Font();
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(25), RowCount = items is null ? 3 : 4 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, items is null ? 100 : 55));
        if (items is not null) root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);
        root.Controls.Add(AdminTheme.Label(title, 18, FontStyle.Bold), 0, 0);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White, Padding = new Padding(20) };
        root.Controls.Add(scroll, 0, 1);
        var form = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
        scroll.Controls.Add(form);
        foreach (var field in fields)
        {
            form.Controls.Add(AdminTheme.Label(field.Label, 9, FontStyle.Bold));
            var input = new TextBox
            {
                Text = field.Value, ReadOnly = !editable, Dock = DockStyle.Top,
                Font = AdminTheme.Font(10), Height = 32, Margin = new Padding(0, 0, 0, 16),
                BackColor = editable ? Color.White : AdminTheme.Canvas
            };
            form.Controls.Add(input);
        }
        if (items is not null)
        {
            var grid = AdminTheme.Grid();
            grid.DataSource = items;
            root.Controls.Add(grid, 0, 2);
        }
        var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
        var close = AdminTheme.Button("Close");
        close.Click += (_, _) => Close();
        footer.Controls.Add(close);
        if (editable)
        {
            var save = AdminTheme.Button("Save", true);
            save.Enabled = false;
            footer.Controls.Add(save);
        }
        root.Controls.Add(footer, 0, items is null ? 2 : 3);
    }
}
