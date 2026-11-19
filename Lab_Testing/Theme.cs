namespace Lab_Testing;

// Кольори інтерфейсу в одному місці
public static class Theme
{
    public static readonly Color Primary = Color.FromArgb(31, 78, 121);         // темно-синій (шапка)
    public static readonly Color Accent = Color.FromArgb(46, 117, 182);         // синій (кнопки, рамка вибору)
    public static readonly Color Border = Color.FromArgb(200, 208, 218);        // світло-сіра рамка
    public static readonly Color SelectedBack = Color.FromArgb(232, 242, 252);  // фон вибраної картки режиму
    public static readonly Color PageBack = Color.FromArgb(245, 247, 250);      // фон сторінки
    public static readonly Color Success = Color.FromArgb(46, 125, 50);         // зелений: правильно
    public static readonly Color Error = Color.FromArgb(198, 40, 40);           // червоний: помилка
    public static readonly Color MutedText = Color.FromArgb(96, 108, 122);      // сірий текст підказок
}
