using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Lab_Testing;

// Картка з відповіддю, яку перетягують мишею (ДЖЕРЕЛО Drag-and-Drop).
// Перетягування запускається в QuestionView.Card_MouseDown викликом DoDragDrop
[DesignerCategory("Code")]
public class CardLabel : Label
{
    // Текст відповіді, який несе картка
    public string Value { get; }

    // Картка-шаблон у палітрі слів: при перетягуванні вона не переміщується, а копіюється (ефект Copy)
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsTemplate { get; set; }

    public CardLabel(string value)
    {
        Value = value;
        Text = value;
        UseMnemonic = false;            // інакше "&&" показувалось би як один символ "&"
        AutoSize = true;
        Font = Theme.FontFor(value);
        ForeColor = Theme.CardText;
        BackColor = Color.Transparent;  // кути картки прозорі, видно фон зони
        Cursor = Cursors.Hand;

        Padding = new Padding(LogicalToDeviceUnits(12), LogicalToDeviceUnits(6), LogicalToDeviceUnits(12), LogicalToDeviceUnits(6));
        Margin = new Padding(LogicalToDeviceUnits(4));
        MinimumSize = new Size(LogicalToDeviceUnits(46), LogicalToDeviceUnits(34));
        MaximumSize = new Size(LogicalToDeviceUnits(520), 0);   // довгий текст переноситься на новий рядок
    }

    // Малюємо картку самі: білий заокруглений прямокутник з рамкою і текстом по центру
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle r = new(1, 1, Width - 3, Height - 3);
        using GraphicsPath path = Theme.RoundedRect(r, LogicalToDeviceUnits(7));
        using SolidBrush back = new(Color.White);
        using Pen border = new(Theme.CardBorder, LogicalToDeviceUnits(2) * 0.75f);

        g.FillPath(back, path);
        g.DrawPath(border, path);

        Rectangle textRect = new(Padding.Left, Padding.Top, Width - Padding.Horizontal, Height - Padding.Vertical);
        TextRenderer.DrawText(g, Text, Font, textRect, ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
    }
}
