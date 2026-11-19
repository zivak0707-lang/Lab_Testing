using System.Drawing.Drawing2D;

namespace Lab_Testing;

// Оформлення інтерфейсу: кольори, шрифти карток, малювання заокруглених прямокутників
public static class Theme
{
    // ---------- Кольори ----------
    public static readonly Color Primary = Color.FromArgb(31, 78, 121);         // темно-синій (шапка)
    public static readonly Color Accent = Color.FromArgb(46, 117, 182);         // синій (кнопки, рамка вибору)
    public static readonly Color Border = Color.FromArgb(200, 208, 218);        // світло-сіра рамка
    public static readonly Color SelectedBack = Color.FromArgb(232, 242, 252);  // фон вибраної картки режиму
    public static readonly Color PageBack = Color.FromArgb(245, 247, 250);      // фон сторінки
    public static readonly Color Success = Color.FromArgb(46, 125, 50);         // зелений: правильно
    public static readonly Color Error = Color.FromArgb(198, 40, 40);           // червоний: помилка
    public static readonly Color MutedText = Color.FromArgb(96, 108, 122);      // сірий текст підказок

    public static readonly Color ZoneBack = Color.FromArgb(250, 251, 253);      // фон зони для карток
    public static readonly Color ZoneBorder = Color.FromArgb(160, 174, 190);    // пунктирна рамка зони
    public static readonly Color HighlightBack = Color.FromArgb(214, 234, 252); // зона під курсором з карткою
    public static readonly Color CardBorder = Color.FromArgb(110, 138, 170);    // рамка картки
    public static readonly Color CardText = Color.FromArgb(25, 35, 48);         // текст картки
    public static readonly Color StepCurrent = Color.White;                     // кнопка поточного питання
    public static readonly Color StepAnswered = Color.FromArgb(92, 148, 204);   // кнопка питання з відповіддю
    public static readonly Color Warning = Color.FromArgb(191, 96, 0);          // помаранчевий: частково правильно
    public static readonly Color CorrectBack = Color.FromArgb(226, 243, 228);   // фон правильної картки
    public static readonly Color WrongBack = Color.FromArgb(252, 230, 228);     // фон помилкової картки
    public static readonly Color PartlyBack = Color.FromArgb(255, 238, 214);    // фон «частково правильно»
    public static readonly Color TheoryBack = Color.FromArgb(255, 249, 232);    // фон панелі теорії
    public static readonly Color TimerWarning = Color.FromArgb(255, 150, 140);  // таймер в останні хвилини

    // ---------- Шрифти ----------
    public static readonly Font CodeFont = new("Consolas", 11.5F, FontStyle.Bold);  // картки з кодом: int, &&, 3.14
    public static readonly Font TextFont = new("Segoe UI", 10.5F);                   // картки з текстом українською
    public static readonly Font SectionFont = new("Segoe UI", 10.5F, FontStyle.Bold); // заголовки областей
    public static readonly Font IconFont = new("Segoe MDL2 Assets", 30F);           // значки Windows 10
    public static readonly Font NumberFont = new("Segoe UI", 11.5F, FontStyle.Bold); // номерки в кружечках
    public static readonly Font ReadingFont = new("Segoe UI", 11.5F);               // текст із пропусками

    // Картки з кирилицею пишемо звичайним шрифтом, а код — моноширинним
    public static Font FontFor(string text)
    {
        bool hasCyrillic = text.Any(c => c >= 'Ѐ' && c <= 'ӿ');   // блок кирилиці Unicode
        return hasCyrillic ? TextFont : CodeFont;
    }

    // Заокруглений прямокутник для малювання карток
    public static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        int d = radius * 2;
        GraphicsPath path = new();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
