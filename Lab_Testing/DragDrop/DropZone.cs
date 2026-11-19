using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Lab_Testing;

// Зона, у яку можна кинути картку (ПРИЙМАЧ Drag-and-Drop): кошик, клітинка таблиці, палітра...
// Властивість AllowDrop і обробники DragEnter/DragOver/DragLeave/DragDrop підключає QuestionView.CreateZone
[DesignerCategory("Code")]
public class DropZone : FlowLayoutPanel
{
    private bool highlighted;
    private bool missing;
    private Color normalBackColor = Theme.ZoneBack;

    // Назва зони: "slide", "basket", "palette", "row:0" ...
    public string Key { get; }

    // Скільки карток вміщує зона (0 — без обмежень)
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Capacity { get; set; }

    // Підказка і значок, які видно в порожній зоні
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Caption { get; set; } = "";
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string IconGlyph { get; set; } = "";

    public DropZone(string key)
    {
        Key = key;
        DoubleBuffered = true;   // без мерехтіння під час підсвітки
        BackColor = normalBackColor;
        Padding = new Padding(LogicalToDeviceUnits(4));
        WrapContents = true;

        // Коли картку додали або забрали, перемальовуємо підказку порожньої зони
        ControlAdded += (_, _) => Invalidate();
        ControlRemoved += (_, _) => Invalidate();
    }

    // Звичайний колір фону (без підсвітки)
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color NormalBackColor
    {
        get => normalBackColor;
        set
        {
            normalBackColor = value;

            if (!highlighted)
            {
                BackColor = value;
            }
        }
    }

    // Підсвітка: над зоною тримають картку, яку сюди можна кинути
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Highlighted
    {
        get => highlighted;
        set
        {
            if (highlighted == value)
            {
                return;
            }

            highlighted = value;
            BackColor = value ? Theme.HighlightBack : normalBackColor;
            Invalidate(true);   // перемалювати і картки, бо їхні кути прозорі
        }
    }

    // Після перевірки: зону треба було заповнити, а вона порожня — червона рамка
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Missing
    {
        get => missing;
        set
        {
            missing = value;
            Invalidate();
        }
    }

    // Картки, що зараз лежать у зоні
    public List<CardLabel> Cards => Controls.OfType<CardLabel>().ToList();

    // Зона заповнена (наприклад, клітинка таблиці вже містить картку)
    public bool IsFull => Capacity > 0 && Cards.Count >= Capacity;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        Rectangle r = new(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);

        // Рамка: пунктирна сіра, під час наведення картки — суцільна синя,
        // порожня після перевірки — червона
        bool showMissing = missing && Controls.Count == 0 && !highlighted;
        Color borderColor = highlighted ? Theme.Accent : showMissing ? Theme.Error : Theme.ZoneBorder;

        using (Pen pen = new(borderColor, highlighted || showMissing ? 2 : 1))
        {
            pen.DashStyle = highlighted ? DashStyle.Solid : DashStyle.Dash;
            g.DrawRectangle(pen, new Rectangle(r.X, r.Y, r.Width - (int)pen.Width / 2, r.Height - (int)pen.Width / 2));
        }

        if (Controls.Count > 0)
        {
            return;
        }

        // Порожня зона: значок і підказка по центру
        TextFormatFlags center = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                                 TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;
        Rectangle captionRect = r;

        if (IconGlyph.Length > 0)
        {
            int iconHeight = LogicalToDeviceUnits(56);
            Rectangle iconRect = new(0, r.Height / 2 - iconHeight, r.Width, iconHeight);
            TextRenderer.DrawText(g, IconGlyph, Theme.IconFont, iconRect, Theme.ZoneBorder, center);
            captionRect = new Rectangle(0, r.Height / 2, r.Width, LogicalToDeviceUnits(40));
        }

        TextRenderer.DrawText(g, Caption, Font, captionRect, Theme.MutedText, center);
    }
}
