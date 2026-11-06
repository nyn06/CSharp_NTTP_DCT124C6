using MetropolitanCoffee.Admin.Forms;
using MetropolitanCoffee.Core.Models;
using MetropolitanCoffee.Services;

namespace MetropolitanCoffee.Admin.Ui;

internal sealed class CategoriesPage : UserControl
{
    private readonly AdminCatalogService _catalog;
    private readonly DataGridView _grid = AdminTheme.Grid();
    private readonly TextBox _search = new() { Width = 230, PlaceholderText = "Search category..." };
    private readonly Label _status = AdminTheme.Label("", 9, color: AdminTheme.Muted);
    private IReadOnlyList<AdminCategory> _categories = [];

    internal CategoriesPage(AdminCatalogService catalog)
    {
        _catalog = catalog;
        Dock = DockStyle.Fill;
        BackColor = AdminTheme.Canvas;
        Font = AdminTheme.Font();
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = AdminTheme.Canvas, WrapContents = false };
        root.Controls.Add(actions, 0, 0);
        _search.Height = 35;
        _search.Margin = new Padding(0, 0, 8, 0);
        _search.TextChanged += (_, _) => Render();
        actions.Controls.Add(_search);
        var refresh = AdminTheme.Button("Refresh");
        refresh.Click += async (_, _) => await RefreshAsync();
        actions.Controls.Add(refresh);
        var add = AdminTheme.Button("Add Category", true);
        add.Click += async (_, _) => await EditAsync(null);
        actions.Controls.Add(add);

        var card = AdminTheme.Card();
        card.Dock = DockStyle.Fill;
        card.Margin = Padding.Empty;
        root.Controls.Add(card, 0, 1);
        var heading = AdminTheme.Label("Category Management", 13, FontStyle.Bold);
        heading.Dock = DockStyle.Top;
        heading.Height = 35;
        heading.AutoSize = false;
        card.Controls.Add(_grid);
        card.Controls.Add(heading);
        _status.Dock = DockStyle.Bottom;
        _status.AutoSize = false;
        _status.Height = 24;
        card.Controls.Add(_status);

        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.ScrollBars = ScrollBars.Both;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 75, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 160, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Count", HeaderText = "Products", Width = 105, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Actions", Text = "Edit", UseColumnTextForButtonValue = true, Width = 70 });
        _grid.Columns.Add(new DataGridViewButtonColumn { Name = "Delete", HeaderText = "", Text = "Delete", UseColumnTextForButtonValue = true, Width = 75 });
        _grid.CellContentClick += async (_, e) =>
        {
            if (e.RowIndex < 0 || _grid.Rows[e.RowIndex].Tag is not AdminCategory category) return;
            if (_grid.Columns[e.ColumnIndex].Name == "Edit") await EditAsync(category);
            if (_grid.Columns[e.ColumnIndex].Name == "Delete") await DeleteAsync(category);
        };
    }

    internal async Task RefreshAsync()
    {
        _status.Text = "Loading categories...";
        try
        {
            _categories = await _catalog.GetCategoriesAsync();
            if (IsDisposed) return;
            Render();
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            _status.Text = "Unable to load categories from SQL Server.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private void Render()
    {
        _grid.Rows.Clear();
        var search = _search.Text.Trim();
        var filtered = _categories.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        foreach (var category in filtered)
            _grid.Rows[_grid.Rows.Add(category.Id, category.Name, category.ProductCount)].Tag = category;
        _status.Text = $"{_grid.Rows.Count:N0} / {_categories.Count:N0} categories";
    }

    private async Task EditAsync(AdminCategory? category)
    {
        using var form = new CategoryEditorForm(_catalog, category);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        await RefreshAsync();
        MessageBox.Show(this, category is null ? "Category added successfully." : "Category updated successfully.",
            "Categories", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task DeleteAsync(AdminCategory category)
    {
        if (MessageBox.Show(this, $"Delete category \"{category.Name}\"?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            var result = await _catalog.DeleteCategoryAsync(category.Id);
            MessageBox.Show(this, result.Message, "Categories", MessageBoxButtons.OK,
                result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (result.Success) await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Unable to delete the category. Please try again.", "Categories", MessageBoxButtons.OK, MessageBoxIcon.Error);
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
