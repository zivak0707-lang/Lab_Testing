namespace Lab_Testing;

// Тип 2 «Таблиця відповідностей»: картку треба покласти в клітинку навпроти відповідного опису
public class TableView : QuestionView
{
    private readonly DropZone palette;                 // картки, які ще не розкладено
    private readonly List<DropZone> cells = new();     // праві клітинки таблиці (по одній картці)

    public TableView(Question question, Random random) : base(question, random)
    {
        palette = CreateZone("palette", 0, "Усі картки розкладено");
        palette.Padding = new Padding(S(8));
        HomeZone = palette;

        // Таблиця: рядок заголовків і по рядку на кожну відповідність
        TableLayoutPanel table = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            BackColor = Color.White
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        table.RowStyles.Add(new RowStyle(SizeType.Absolute, S(34)));
        table.Controls.Add(CreateHeaderCell(question.Columns[0]), 0, 0);
        table.Controls.Add(CreateHeaderCell(question.Columns[1]), 1, 0);

        for (int i = 0; i < question.Rows.Count; i++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, S(52)));

            Label left = new()
            {
                Text = question.Rows[i].Left,
                Dock = DockStyle.Fill,
                Font = Theme.FontFor(question.Rows[i].Left),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(S(8), 0, S(4), 0),
                UseMnemonic = false
            };

            DropZone cell = CreateZone("row:" + i, 1, "перетягніть сюди");
            cell.Dock = DockStyle.Fill;
            cell.Margin = new Padding(S(3));
            cell.Padding = new Padding(S(2));
            cells.Add(cell);

            table.Controls.Add(left, 0, i + 1);
            table.Controls.Add(cell, 1, i + 1);
        }

        table.RowCount = question.Rows.Count + 1;

        // Картки: правильні відповіді + зайві, перемішані
        IEnumerable<string> allCards = question.Rows.Select(r => r.Answer).Concat(question.ExtraCards);

        foreach (string text in Shuffle(allCards))
        {
            palette.Controls.Add(CreateCard(text));
        }

        // Ліворуч таблиця (з прокруткою, якщо не вміщується), праворуч картки
        Panel tableHolder = new() { AutoScroll = true };
        tableHolder.Controls.Add(table);

        Controls.Add(CreateColumns(64, CreateSection("Таблиця", tableHolder), CreateSection("Картки", palette)));
    }

    // Відповідь почата, якщо хоча б одна клітинка заповнена
    public override bool HasAnswer => cells.Any(c => c.Cards.Count > 0);

    // Бали: частка правильно заповнених клітинок
    public override QuestionResult Evaluate(bool highlight)
    {
        QuestionResult result = NewResult();
        int correct = 0;

        for (int i = 0; i < cells.Count; i++)
        {
            TableRow row = Question.Rows[i];
            CardLabel? card = cells[i].Cards.FirstOrDefault();

            if (highlight)
            {
                cells[i].Missing = card == null;
            }

            if (card == null)
            {
                result.Mistakes.Add($"{Quote(row.Left)} — клітинку не заповнено (правильно: {Quote(row.Answer)})");
            }
            else if (card.Value == row.Answer)
            {
                correct++;
                Mark(card, true, highlight);
            }
            else
            {
                Mark(card, false, highlight);
                result.Mistakes.Add($"{Quote(row.Left)} — покладено {Quote(card.Value)}, правильно: {Quote(row.Answer)}");
            }
        }

        result.Score = Grading.Points(correct, cells.Count);
        result.Summary = $"правильно {correct} з {cells.Count}";
        return result;
    }

    private Label CreateHeaderCell(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = Theme.SectionFont,
            BackColor = Theme.SelectedBack,
            ForeColor = Theme.Primary,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(S(8), 0, 0, 0),
            Margin = Padding.Empty,
            UseMnemonic = false
        };
    }
}
