using MetropolitanCoffee.Admin.Ui;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Forms;

internal sealed class CategoryEditorForm : Form
{
    private readonly AdminCatalogService _catalog;
    private readonly AdminCategory? _category;
    private readonly TextBox _name = new() { Dock = DockStyle.Fill, MaxLength = 100 };
    private readonly Label _error = AdminTheme.Label("", 9, color: Color.Firebrick);
    private readonly Button _save = AdminTheme.Button("Save", true);

    internal CategoryEditorForm(AdminCatalogService catalog, AdminCategory? category = null)
    {
        _catalog = catalog;
        _category = category;
        Text = category is null ? "Add Category" : "Edit Category";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(460, 235);
        MinimumSize = Size;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();
        _name.Text = category?.Name ?? "";

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 1, RowCount = 4 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);
        root.Controls.Add(AdminTheme.Label("Category Name", 10, FontStyle.Bold), 0, 0);
        root.Controls.Add(_name, 0, 1);
        _error.AutoSize = false;
        _error.Dock = DockStyle.Fill;
        root.Controls.Add(_error, 0, 2);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        root.Controls.Add(actions, 0, 3);
        var cancel = AdminTheme.Button("Cancel");
        cancel.Click += (_, _) => Close();
        actions.Controls.Add(cancel);
        actions.Controls.Add(_save);
        _save.Click += async (_, _) => await SaveAsync();
        AcceptButton = _save;
        CancelButton = cancel;
    }

    private async Task SaveAsync()
    {
        _save.Enabled = false;
        _error.Text = "";
        try
        {
            var result = _category is null
                ? await _catalog.AddCategoryAsync(_name.Text)
                : await _catalog.UpdateCategoryAsync(_category.Id, _name.Text);
            if (!result.Success) { _error.Text = result.Message; return; }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _error.Text = "Unable to save the category. Please try again.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally { _save.Enabled = true; }
    }
}
