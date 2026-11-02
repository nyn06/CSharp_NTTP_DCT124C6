namespace MetropolitanCoffee.Admin.Ui;

internal sealed class AdminNavButton : Button
{
    internal string IconGlyph { get; init; } = "\uE80F";
    internal bool Selected { get; set; }

    internal AdminNavButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Height = 48;
        TextAlign = ContentAlignment.MiddleLeft;
        Cursor = Cursors.Hand;
        ForeColor = Color.White;
        BackColor = AdminTheme.Chrome;
        Font = AdminTheme.Font(11);
        Margin = Padding.Empty;
        UseVisualStyleBackColor = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using var brush = new SolidBrush(Selected ? AdminTheme.ChromeHover : BackColor);
        e.Graphics.FillRectangle(brush, ClientRectangle);
        using var iconFont = new Font("Segoe MDL2 Assets", 18, GraphicsUnit.Pixel);
        TextRenderer.DrawText(e.Graphics, IconGlyph, iconFont,
            new Rectangle(20, 0, 28, Height), Color.White,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        TextRenderer.DrawText(e.Graphics, Text, Font,
            new Rectangle(56, 0, Math.Max(0, Width - 62), Height), Color.White,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
    }
}
