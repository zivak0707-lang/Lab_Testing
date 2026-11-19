using System.Text.RegularExpressions;

namespace Lab_Testing;

// Показує в RichTextBox текст із простою розміткою (як у Markdown).
// Цією розміткою записані теорія і пояснення в банку питань:
//   ## Заголовок     — короткий заголовок розділу
//   ```              — рядок, що відкриває і закриває блок коду (моноширинний шрифт на сірому тлі)
//   `код`            — код усередині речення
//   **текст**        — жирний текст
//   - пункт          — пункт списку
//   1. пункт         — нумерований пункт
//   порожній рядок   — невеликий відступ між абзацами
public static class RichTextFormatter
{
    private static readonly Font HeadingFont = new("Segoe UI", 10.5F, FontStyle.Bold);
    private static readonly Font BoldFont = new("Segoe UI", 10F, FontStyle.Bold);
    private static readonly Font CodeBlockFont = new("Consolas", 9.5F);
    private static readonly Font InlineCodeFont = new("Consolas", 10F, FontStyle.Bold);
    private static readonly Font GapFont = new("Segoe UI", 4F);   // низький порожній рядок — відступ

    private static readonly Color CodeBack = Color.FromArgb(229, 232, 237);   // сіре тло коду
    private static readonly Color CodeText = Color.FromArgb(25, 35, 48);
    private static readonly Regex NumberedItem = new(@"^\d+\.\s");

    // Очищає поле і показує в ньому розмічений текст (прокрутка — на початок)
    public static void Show(RichTextBox box, string markup)
    {
        box.Clear();
        AppendMarkup(box, markup);
        ScrollToTop(box);
    }

    // Показує сіру підказку замість тексту
    public static void ShowHint(RichTextBox box, string hint)
    {
        box.Clear();
        AppendParagraph(box, hint, Theme.MutedText);
        ScrollToTop(box);
    }

    // Додає в кінець поля розмічений текст; indent — відступ зліва в пікселях
    public static void AppendMarkup(RichTextBox box, string markup, int indent = 0)
    {
        List<string> code = new();
        bool inCode = false;

        foreach (string rawLine in markup.Replace("\r", "").Split('\n'))
        {
            string line = rawLine.TrimEnd();

            // Рядок ``` відкриває або закриває блок коду
            if (line.Trim() == "```")
            {
                if (inCode)
                {
                    AppendCodeBlock(box, code, indent);
                    code.Clear();
                }

                inCode = !inCode;
                continue;
            }

            if (inCode)
            {
                code.Add(line);
            }
            else if (line.Length == 0)
            {
                AppendGap(box);
            }
            else if (line.StartsWith("## "))
            {
                AppendHeading(box, line[3..], indent);
            }
            else if (line.StartsWith("- "))
            {
                AppendParagraph(box, line[2..], box.ForeColor, indent: indent, bullet: true);
            }
            else if (NumberedItem.IsMatch(line))
            {
                AppendParagraph(box, line, box.ForeColor, indent: indent, hanging: 16);
            }
            else
            {
                AppendParagraph(box, line, box.ForeColor, indent: indent);
            }
        }

        if (inCode && code.Count > 0)
        {
            AppendCodeBlock(box, code, indent);   // блок коду не закрили — все одно показуємо
        }
    }

    // Один абзац: `код` і **жирне** всередині тексту виділяються
    public static void AppendParagraph(RichTextBox box, string text, Color color, bool bold = false,
                                       int indent = 0, bool bullet = false, int hanging = 0)
    {
        // Пункт списку: маркер «•», а перенесені рядки вирівнюються під текстом, а не під маркером
        if (bullet)
        {
            text = "•  " + text;
            hanging = Math.Max(hanging, 13);
        }

        StartParagraph(box, indent, false, hanging);
        string[] parts = text.Split('`');   // непарні частини (між зворотними лапками) — код

        for (int i = 0; i < parts.Length; i++)
        {
            if (i % 2 == 1)
            {
                AppendRun(box, parts[i], InlineCodeFont, CodeText, CodeBack);
                continue;
            }

            string[] boldParts = parts[i].Split("**");

            for (int j = 0; j < boldParts.Length; j++)
            {
                AppendRun(box, boldParts[j], bold || j % 2 == 1 ? BoldFont : box.Font, color, box.BackColor);
            }
        }

        AppendRun(box, "\n", box.Font, color, box.BackColor);

        // Новий порожній абзац успадкував би маркер списку — скидаємо його формат
        StartParagraph(box, 0, false, 0);
    }

    // Заголовок розділу: жирний синій рядок з відступом зверху
    public static void AppendHeading(RichTextBox box, string text, int indent = 0)
    {
        if (box.TextLength > 0)
        {
            AppendGap(box);
        }

        StartParagraph(box, indent, false, 0);
        AppendRun(box, text + "\n", HeadingFont, Theme.Primary, box.BackColor);
    }

    // Невеликий відступ між абзацами
    public static void AppendGap(RichTextBox box)
    {
        StartParagraph(box, 0, false, 0);
        AppendRun(box, "\n", GapFont, box.ForeColor, box.BackColor);
    }

    // Блок коду: рядки доповнюються пробілами до однакової довжини, тож сіре тло — рівний прямокутник
    private static void AppendCodeBlock(RichTextBox box, List<string> lines, int indent)
    {
        int width = lines.Max(l => l.Length) + 1;
        AppendGap(box);

        foreach (string line in lines)
        {
            StartParagraph(box, indent + 10, false, 0);
            AppendRun(box, " " + line.PadRight(width), CodeBlockFont, CodeText, CodeBack);
            AppendRun(box, "\n", CodeBlockFont, CodeText, box.BackColor);
        }

        AppendGap(box);
    }

    // Новий абзац у кінці тексту: відступ, маркер списку, висячий відступ
    private static void StartParagraph(RichTextBox box, int indent, bool bullet, int hanging)
    {
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;
        box.SelectionBullet = bullet;
        box.SelectionIndent = indent;
        box.SelectionHangingIndent = hanging;
    }

    // Шматок тексту заданим шрифтом і кольором
    private static void AppendRun(RichTextBox box, string text, Font font, Color color, Color back)
    {
        if (text.Length == 0)
        {
            return;
        }

        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;
        box.SelectionFont = font;
        box.SelectionColor = color;
        box.SelectionBackColor = back;
        box.SelectedText = text;
    }

    private static void ScrollToTop(RichTextBox box)
    {
        box.SelectionStart = 0;
        box.SelectionLength = 0;
        box.ScrollToCaret();
    }
}
