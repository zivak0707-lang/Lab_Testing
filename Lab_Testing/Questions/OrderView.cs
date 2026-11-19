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
        palette.AutoScroll = true;
        HomeZone = palette;

        int cardWidth = S(380);   // ширина картки кроку (довгий текст переноситься)

        foreach (string step in Shuffle(question.Steps))
        {
            CardLabel card = CreateCard(step);
            card.MaximumSize = new Size(cardWidth, 0);
            card.MinimumSize = new Size(cardWidth, S(34));
            palette.Controls.Add(card);
        }

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
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, S(58)));

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
