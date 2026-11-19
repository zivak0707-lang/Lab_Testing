using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Lab_Testing;

// Тип 4 «Слова в пропуски»: слова з палітри КОПІЮЮТЬСЯ в пропуски тексту (ефект Copy),
// тому одне слово можна вставити кілька разів. Слово з пропуску можна перетягнути
// в інший пропуск (Move) або назад у палітру — тоді воно зникає з пропуску
public class GapsView : QuestionView
{
    private readonly DropZone palette;                // слова-шаблони
    private readonly List<DropZone> gaps = new();     // пропуски в тексті

    public GapsView(Question question, Random random) : base(question, random)
    {
        palette = CreateZone("palette");
        palette.AutoSize = true;
        palette.MinimumSize = new Size(0, S(56));
        palette.Padding = new Padding(S(8));

        // Палітра: кожне слово один раз (правильні без повторів + зайві), перемішані
        IEnumerable<string> words = question.GetGapAnswers().Distinct().Concat(question.ExtraWords);

        foreach (string word in Shuffle(words))
        {
            palette.Controls.Add(CreateCard(word, isTemplate: true));
        }

        // Текст: слова — звичайні написи, пропуски — зони на одну картку
        FlowLayoutPanel text = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.White,
            Padding = new Padding(S(14), S(10), S(14), S(10))
        };

        int lineHeight = S(46);   // однакова висота рядка для слів і пропусків

        // Розбиваємо текст на частини: "{слово}" — пропуск, решта — звичайний текст
        foreach (string part in Regex.Split(question.Text, @"(\{[^{}]+\})"))
        {
            if (part.StartsWith('{') && part.EndsWith('}'))
            {
                DropZone gap = CreateZone("gap:" + gaps.Count, 1);
                gap.AutoSize = true;
                gap.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                gap.WrapContents = false;
                gap.MinimumSize = new Size(S(84), lineHeight);
                gap.Padding = new Padding(S(2), 0, S(2), 0);
                gap.Margin = new Padding(0, 0, S(5), 0);
                gaps.Add(gap);
                text.Controls.Add(gap);
                continue;
            }

            foreach (string word in part.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                text.Controls.Add(CreateWordLabel(word, lineHeight));
            }
        }

        // Згори палітра слів (висота за вмістом), під нею текст (решта місця)
        TableLayoutPanel layout = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        palette.Dock = DockStyle.Fill;
        palette.Margin = new Padding(S(6), 0, S(6), S(8));
        text.Margin = new Padding(S(6), 0, S(6), S(6));

        layout.Controls.Add(CreateHeader("Слова (кожне можна вставити кілька разів)"), 0, 0);
        layout.Controls.Add(palette, 0, 1);
        layout.Controls.Add(CreateHeader("Текст"), 0, 2);
        layout.Controls.Add(text, 0, 3);

        Controls.Add(layout);
    }

    // Відповідь почата, якщо заповнено хоча б один пропуск
    public override bool HasAnswer => gaps.Any(g => g.Cards.Count > 0);

    // Бали: частка правильно заповнених пропусків
    public override QuestionResult Evaluate(bool highlight)
    {
        QuestionResult result = NewResult();
        List<string> answers = Question.GetGapAnswers();
        int correct = 0;

        for (int i = 0; i < gaps.Count; i++)
        {
            CardLabel? card = gaps[i].Cards.FirstOrDefault();

            if (highlight)
            {
                gaps[i].Missing = card == null;
            }

            if (card == null)
            {
                result.Mistakes.Add($"Пропуск {i + 1}: не заповнено (правильно: {Quote(answers[i])})");
                result.Issues.Add($"Пропуск {i + 1}: ще не заповнено.");
            }
            else if (card.Value == answers[i])
            {
                correct++;
                Mark(card, true, highlight);
            }
            else
            {
                Mark(card, false, highlight);
                result.Mistakes.Add($"Пропуск {i + 1}: вставлено {Quote(card.Value)}, правильно: {Quote(answers[i])}");
                result.Issues.Add($"Пропуск {i + 1}: слово {Quote(card.Value)} тут не підходить.");
            }
        }

        SetScore(result, correct, gaps.Count);
        result.Summary = $"правильно {correct} з {gaps.Count}";
        return result;
    }

    // У палітру можна повернути лише слово з пропуску (шаблон і так лежить у палітрі)
    protected override bool CanAccept(CardLabel card, DropZone zone)
    {
        return zone != palette || !card.IsTemplate;
    }

    // Розміщення слова: копія з палітри, переміщення між пропусками або видалення
    public override void PlaceCard(CardLabel card, DropZone zone, DragDropEffects effect)
    {
        if (card.Parent == zone)
        {
            return;
        }

        // Слово з пропуску повернули в палітру — просто прибираємо його з пропуску
        if (zone == palette)
        {
            RemoveCard(card);
            OnAnswerChanged();
            return;
        }

        // Слово, що вже стояло в цьому пропуску, прибираємо (слова в палітрі не закінчуються)
        foreach (CardLabel old in zone.Cards)
        {
            RemoveCard(old);
        }

        // Copy — у пропуск кладемо нову картку з тим самим словом; Move — ту саму картку
        CardLabel placed = card.IsTemplate ? CreateCard(card.Value) : card;
        zone.Controls.Add(placed);
        OnAnswerChanged();
    }

    // Одне слово тексту; висота як у пропуску, щоб рядок був рівним,
    // ширина — саме слово плюс звичайний пробіл
    private static WordLabel CreateWordLabel(string word, int height)
    {
        const TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
        Font font = Theme.ReadingFont;

        int wordWidth = TextRenderer.MeasureText(word, font, Size.Empty, flags).Width;
        int spaceWidth = TextRenderer.MeasureText("a a", font, Size.Empty, flags).Width
                       - TextRenderer.MeasureText("aa", font, Size.Empty, flags).Width;

        return new WordLabel(word, new Size(wordWidth + spaceWidth, height));
    }

    // Слово тексту без внутрішніх полів (у звичайного Label поля роблять проміжки між словами завеликими)
    [DesignerCategory("Code")]
    private sealed class WordLabel : Control
    {
        public WordLabel(string word, Size size)
        {
            Text = word;
            Font = Theme.ReadingFont;
            Size = size;
            Margin = Padding.Empty;
            BackColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }
}
