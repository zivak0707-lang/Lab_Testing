namespace Lab_Testing;

// Тип 5 «Порядок кроків»: перемішані кроки алгоритму розставляють у пронумеровані клітинки 1, 2, 3 ...
public class OrderView : QuestionView
{
    private readonly DropZone palette;                 // кроки, які ще не розставлено
    private readonly List<DropZone> slots = new();     // пронумеровані клітинки (по одному кроку)

    public OrderView(Question question, Random random) : base(question, random)
    {
        palette = CreateZone("palette", 0, "Усі кроки розставлено");
        palette.Padding = new Padding(S(8));
        palette.FlowDirection = FlowDirection.TopDown;   // кроки один під одним
        palette.WrapContents = false;
        palette.AutoScroll = false;  // до 8 кроків вміщуються без прокрутки
        HomeZone = palette;

        foreach (string step in Shuffle(question.Steps))
        {
            palette.Controls.Add(CreateCard(step));
        }

        // Картки кроків однакової ширини — на всю ширину списку (довгий текст переноситься)
        palette.Resize += (_, _) => FitCardWidths();

        // Ліва частина: номер кроку і клітинка для картки
        TableLayoutPanel table = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Theme.PageBack
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(40)));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < question.Steps.Count; i++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, S(66)));   // вміщує картку у два рядки

            Label number = new()
            {
                Text = (i + 1).ToString(),
                Dock = DockStyle.Fill,
                Font = Theme.NumberFont,
                ForeColor = Theme.Primary,
                TextAlign = ContentAlignment.MiddleCenter
            };

            DropZone slot = CreateZone("slot:" + i, 1, "крок " + (i + 1));
            slot.Dock = DockStyle.Fill;
            slot.Margin = new Padding(0, S(2), S(4), S(2));
            slot.Padding = new Padding(S(2));
            slots.Add(slot);

            table.Controls.Add(number, 0, i);
            table.Controls.Add(slot, 1, i);
        }

        table.RowCount = question.Steps.Count;

        Panel tableHolder = new() { AutoScroll = true };
        tableHolder.Controls.Add(table);

        Controls.Add(CreateColumns(54, CreateSection("Порядок виконання", tableHolder), CreateSection("Кроки", palette)));
    }

    // Відповідь почата, якщо хоча б одна клітинка заповнена
    public override bool HasAnswer => slots.Any(s => s.Cards.Count > 0);

    // Бали: частка кроків, що стоять на своєму місці
    public override QuestionResult Evaluate(bool highlight)
    {
        QuestionResult result = NewResult();
        int correct = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            string expected = Question.Steps[i];
            CardLabel? card = slots[i].Cards.FirstOrDefault();

            if (highlight)
            {
                slots[i].Missing = card == null;
            }

            if (card == null)
            {
                result.Mistakes.Add($"Крок {i + 1}: клітинку не заповнено (правильно: {Quote(expected)})");
                result.Issues.Add($"Крок {i + 1}: клітинку ще не заповнено.");
            }
            else if (card.Value == expected)
            {
                correct++;
                Mark(card, true, highlight);
            }
            else
            {
                Mark(card, false, highlight);
                result.Mistakes.Add($"Крок {i + 1}: стоїть {Quote(card.Value)}, правильно: {Quote(expected)}");
                result.Issues.Add($"Крок {i + 1}: {Quote(card.Value)} стоїть не на своєму місці.");
            }
        }

        result.Score = Grading.Points(correct, slots.Count);
        result.Summary = $"на своєму місці {correct} з {slots.Count}";
        return result;
    }

    // Ширина карток кроків підлаштовується під ширину списку кроків
    private void FitCardWidths()
    {
        int width = palette.ClientSize.Width - palette.Padding.Horizontal - S(10) - SystemInformation.VerticalScrollBarWidth;

        if (width < S(150))
        {
            return;
        }

        foreach (CardLabel card in Cards)
        {
            card.MinimumSize = new Size(width, S(34));
            card.MaximumSize = new Size(width, 0);
        }
    }

    // Кладе крок у клітинку. Якщо клітинка зайнята: при перенесенні з іншої клітинки кроки
    // міняються місцями, а при перенесенні зі списку кроків старий крок повертається в список
    public override void PlaceCard(CardLabel card, DropZone zone, DragDropEffects effect)
    {
        if (card.Parent == zone)
        {
            return;
        }

        DropZone? from = card.Parent as DropZone;

        if (zone != palette && zone.Cards.Count > 0)
        {
            CardLabel old = zone.Cards[0];
            DropZone target = from != null && from != palette ? from : palette;
            target.Controls.Add(old);
        }

        zone.Controls.Add(card);
        OnAnswerChanged();
    }
}
