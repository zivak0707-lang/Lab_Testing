namespace Lab_Testing;

// Тип 3 «Номерки на категорії»: кружечки з номерами елементів перетягують на рядки категорій.
// В одному рядку може бути кілька номерків
public class CategoriesView : QuestionView
{
    private readonly DropZone palette;                 // кружечки, які ще не розкладено
    private readonly List<DropZone> rows = new();      // рядки категорій

    public CategoriesView(Question question, Random random) : base(question, random)
    {
        // Стала висота палітри (до 7 номерків в один ряд), щоб опис під нею не «стрибав»,
        // коли номерки забирають
        const int perRow = 7;
        int paletteRows = (question.Items.Count + perRow - 1) / perRow;

        palette = CreateZone("palette", 0, "Усі номерки розкладено");
        palette.Dock = DockStyle.Top;
        palette.Height = S(16) + paletteRows * S(48);
        palette.Padding = new Padding(S(8));
        HomeZone = palette;

        // Елементи перемішуємо і нумеруємо: у кожного учня свої номери
        List<QuestionItem> items = Shuffle(question.Items);

        // Пояснення номерів: «1 — int», «2 — float» ...
        FlowLayoutPanel legend = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, S(8), 0, 0)
        };

        for (int i = 0; i < items.Count; i++)
        {
            string number = (i + 1).ToString();

            CardLabel circle = CreateCard(items[i].Text, number);
            circle.IsRound = true;
            palette.Controls.Add(circle);

            legend.Controls.Add(new Label
            {
                Text = $"{number}  —  {items[i].Text}",
                Font = Theme.FontFor(items[i].Text),
                AutoSize = true,
                MaximumSize = new Size(S(300), 0),
                Margin = new Padding(S(4), S(3), 0, S(3)),
                UseMnemonic = false
            });
        }

        // Довгі пояснення переносяться за шириною колонки
        legend.Resize += (_, _) =>
        {
            int width = legend.ClientSize.Width - S(12);

            if (width > S(100))
            {
                foreach (Control label in legend.Controls)
                {
                    label.MaximumSize = new Size(width, 0);
                }
            }
        };

        // Ліва частина: таблиця «категорія — зона для номерків»
        TableLayoutPanel table = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            BackColor = Color.White
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));

        for (int i = 0; i < question.Categories.Count; i++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, S(60)));

            Label name = new()
            {
                Text = question.Categories[i],
                Dock = DockStyle.Fill,
                Font = Theme.FontFor(question.Categories[i]) == Theme.CodeFont ? Theme.CodeFont : Theme.SectionFont,
                ForeColor = Theme.Primary,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(S(8), 0, S(4), 0),
                UseMnemonic = false
            };

            DropZone row = CreateZone("cat:" + i, 0, "перетягніть сюди номерки");
            row.Dock = DockStyle.Fill;
            row.Margin = new Padding(S(3));
            row.Padding = new Padding(S(3));
            rows.Add(row);

            table.Controls.Add(name, 0, i);
            table.Controls.Add(row, 1, i);
        }

        table.RowCount = question.Categories.Count;

        Panel tableHolder = new() { AutoScroll = true };
        tableHolder.Controls.Add(table);

        // Права частина: кружечки згори, під ними — що означає кожен номер
        Panel right = new();
        right.Controls.Add(legend);    // Fill
        right.Controls.Add(palette);   // Top

        Controls.Add(CreateColumns(58, CreateSection("Категорії", tableHolder), CreateSection("Номерки", right)));
    }

    // Відповідь почата, якщо хоча б один номерок лежить на рядку категорії
    public override bool HasAnswer => rows.Any(r => r.Cards.Count > 0);

    // Бали: частка номерків, покладених на правильний рядок
    public override QuestionResult Evaluate(bool highlight)
    {
        QuestionResult result = NewResult();
        List<string> notPlaced = new();
        int correct = 0;

        // Перебираємо номерки по порядку номерів: 1, 2, 3 ...
        foreach (CardLabel circle in Cards.OrderBy(c => int.Parse(c.Text)))
        {
            QuestionItem item = Question.Items.First(i => i.Text == circle.Value);
            int rowIndex = rows.IndexOf((circle.Parent as DropZone)!);   // -1 — номерок ще в палітрі
            string name = $"№{circle.Text} {Quote(item.Text)}";

            if (rowIndex < 0)
            {
                result.Mistakes.Add($"{name} не розкладено (правильно: {Quote(item.Category)})");
                notPlaced.Add("№" + circle.Text);
            }
            else if (Question.Categories[rowIndex] == item.Category)
            {
                correct++;
                Mark(circle, true, highlight);
            }
            else
            {
                Mark(circle, false, highlight);
                result.Mistakes.Add($"{name} покладено в {Quote(Question.Categories[rowIndex])}, правильно: {Quote(item.Category)}");
                result.Issues.Add($"{name} лежить не в тій категорії.");
            }
        }

        // Нерозкладені номерки видно в палітрі, тому їх можна назвати; палітра — у помаранчевій рамці
        if (notPlaced.Count > 0)
        {
            result.Issues.Add($"Ще не розкладено номерки: {string.Join(", ", notPlaced)}.");

            if (highlight)
            {
                palette.Attention = true;
            }
        }

        result.Score = Grading.Points(correct, Question.Items.Count);
        result.Summary = $"правильно {correct} з {Question.Items.Count}";
        return result;
    }
}
