using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Lab_Testing;

// Підсвітка картки після перевірки
public enum CardState
{
    Normal,    // звичайна
    Correct,   // правильно — зелена
    Wrong      // помилка — червона
}

// Картка з відповіддю, яку перетягують мишею (ДЖЕРЕЛО Drag-and-Drop).
// Перетягування запускається в QuestionView.Card_MouseDown викликом DoDragDrop
[DesignerCategory("Code")]
public class CardLabel : Label
{
    private bool isRound;
    private CardState state = CardState.Normal;

    // Відповідь, яку несе картка (для номерка — текст елемента, а на картці видно лише номер)
    public string Value { get; }

    // Картка-шаблон у палітрі слів: при перетягуванні вона не переміщується, а копіюється (ефект Copy)
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsTemplate { get; set; }

    // value — відповідь; display — що написати на картці (якщо не вказано, то сама відповідь)
    public CardLabel(string value, string? display = null)
    {
        Value = value;
        Text = display ?? value;
        UseMnemonic = false;            // інакше "&&" показувалось би як один символ "&"
        AutoSize = true;
        Font = Theme.FontFor(Text);
        ForeColor = Theme.CardText;
        BackColor = Color.Transparent;  // кути картки прозорі, видно фон зони
        Cursor = Cursors.Hand;

        Padding = new Padding(LogicalToDeviceUnits(12), LogicalToDeviceUnits(6), LogicalToDeviceUnits(12), LogicalToDeviceUnits(6));
        Margin = new Padding(LogicalToDeviceUnits(4));
        MinimumSize = new Size(LogicalToDeviceUnits(46), LogicalToDeviceUnits(34));
        MaximumSize = new Size(LogicalToDeviceUnits(520), 0);   // довгий текст переноситься на новий рядок
    }

    // Кругла картка-номерок фіксованого розміру
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsRound
    {
        get => isRound;
        set
        {
            isRound = value;

            if (value)
            {
                AutoSize = false;
                MinimumSize = Size.Empty;
                Padding = Padding.Empty;
                Size = new Size(LogicalToDeviceUnits(40), LogicalToDeviceUnits(40));
                Font = Theme.NumberFont;
            }
        }
    }

    // Підсвітка після перевірки (режим «Навчання»)
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CardState State
    {
        get => state;
        set
        {
            state = value;
            Invalidate();
        }
    }

    // Малюємо картку самі: білий заокруглений прямокутник (або коло) з рамкою і текстом по центру
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle r = new(1, 1, Width - 3, Height - 3);
        using GraphicsPath path = isRound ? new GraphicsPath() : Theme.RoundedRect(r, LogicalToDeviceUnits(7));

        if (isRound)
        {
            path.AddEllipse(r);
        }

        // Кольори залежать від підсвітки: зелена — правильно, червона — помилка
        Color backColor = state switch
        {
            CardState.Correct => Theme.CorrectBack,
            CardState.Wrong => Theme.WrongBack,
            _ => isRound ? Theme.SelectedBack : Color.White
        };
        Color borderColor = state switch
        {
            CardState.Correct => Theme.Success,
            CardState.Wrong => Theme.Error,
            _ => isRound ? Theme.Accent : Theme.CardBorder
        };
        float borderWidth = LogicalToDeviceUnits(2) * (state == CardState.Normal ? 0.75f : 1.2f);

        using SolidBrush back = new(backColor);
        using Pen border = new(borderColor, borderWidth);

        g.FillPath(back, path);
        g.DrawPath(border, path);

        Rectangle textRect = new(Padding.Left, Padding.Top, Width - Padding.Horizontal, Height - Padding.Vertical);
        TextRenderer.DrawText(g, Text, Font, textRect, isRound ? Theme.Primary : ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
    }
}
