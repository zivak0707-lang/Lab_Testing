using System.ComponentModel;

namespace Lab_Testing;

// Базовий клас для всіх типів питань.
// Тут уся загальна логіка Drag-and-Drop: картки (джерела) і зони (приймачі).
// Нащадки (BasketView, TableView, ...) лише будують розмітку і вирішують, куди можна класти картки
[DesignerCategory("Code")]
public abstract class QuestionView : UserControl
{
    private readonly List<CardLabel> cards = new();   // усі картки цього питання
    private readonly List<DropZone> zones = new();    // усі зони цього питання

    // Питання, яке показує це подання
    public Question Question { get; }

    // Генератор випадкових чисел спроби (для перемішування карток)
    protected Random Random { get; }

    // Зона, куди повертаються картки, витіснені з заповненої зони (палітра або слайд)
    protected DropZone? HomeZone { get; set; }

    public IReadOnlyList<CardLabel> Cards => cards;
    public IReadOnlyList<DropZone> Zones => zones;

    // Подія: учень переклав картку (вікно тесту оновлює кнопки питань)
    public event EventHandler? AnswerChanged;

    // Чи учень уже почав відповідати на це питання
    public abstract bool HasAnswer { get; }

    protected QuestionView(Question question, Random random)
    {
        Question = question;
        Random = random;
        Dock = DockStyle.Fill;
        AutoScroll = true;
        BackColor = Theme.PageBack;
    }

    // Створює подання потрібного типу для питання
    public static QuestionView Create(Question question, Random random)
    {
        return question.Type switch
        {
            QuestionType.Basket => new BasketView(question, random),
            QuestionType.Table => new TableView(question, random),
            QuestionType.Categories => new CategoriesView(question, random),
            QuestionType.Gaps => new GapsView(question, random),
            _ => new OrderView(question, random)
        };
    }

    // Зона за назвою
    public DropZone GetZone(string key)
    {
        return zones.First(z => z.Key == key);
    }

    // ===================== Створення карток і зон =====================

    // Нова картка; натискання мишею на неї починає перетягування.
    // display — інший текст на картці (наприклад, номер замість назви елемента)
    protected CardLabel CreateCard(string value, string? display = null, bool isTemplate = false)
    {
        CardLabel card = new(value, display) { IsTemplate = isTemplate };
        card.MouseDown += Card_MouseDown;
        cards.Add(card);
        return card;
    }

    // Прибирає картку-копію (слово з пропуску). Знищуємо її трохи пізніше, бо зараз
    // ще може виконуватися її ж DoDragDrop
    protected void RemoveCard(CardLabel card)
    {
        cards.Remove(card);
        card.Parent?.Controls.Remove(card);

        if (IsHandleCreated)
        {
            BeginInvoke(card.Dispose);
        }
        else
        {
            card.Dispose();
        }
    }

    // Нова зона, яка приймає картки
    protected DropZone CreateZone(string key, int capacity = 0, string caption = "")
    {
        DropZone zone = new(key) { Capacity = capacity, Caption = caption };

        zone.AllowDrop = true;              // дозволяємо кидати на зону
        zone.DragEnter += Zone_DragEnter;   // картку завели на зону
        zone.DragOver += Zone_DragOver;     // картку рухають над зоною
        zone.DragLeave += Zone_DragLeave;   // картку вивели за межі зони
        zone.DragDrop += Zone_DragDrop;     // картку відпустили над зоною

        zones.Add(zone);
        return zone;
    }

    // ===================== Drag-and-Drop =====================

    // 1. Натиснули ліву кнопку на картці — запускаємо стандартне перетягування .NET.
    //    Дані перетягування — сама картка. Звичайна картка переміщується (Move),
    //    картка-шаблон з палітри слів копіюється (Copy)
    private void Card_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || sender is not CardLabel card || !Enabled)
        {
            return;
        }

        DragDropEffects allowed = card.IsTemplate ? DragDropEffects.Copy : DragDropEffects.Move;
        card.DoDragDrop(card, allowed);   // повертається, коли кнопку миші відпустять
    }

    // 2. Картку завели на зону: вирішуємо, чи можна її сюди кинути, і підсвічуємо зону
    private void Zone_DragEnter(object? sender, DragEventArgs e)
    {
        if (sender is not DropZone zone)
        {
            return;
        }

        e.Effect = GetDropEffect(e, zone);
        zone.Highlighted = e.Effect != DragDropEffects.None;
    }

    // 3. Картку рухають над зоною: повторно задаємо ефект (від нього залежить вигляд курсора)
    private void Zone_DragOver(object? sender, DragEventArgs e)
    {
        if (sender is DropZone zone)
        {
            e.Effect = GetDropEffect(e, zone);
        }
    }

    // 4. Картку вивели за межі зони: прибираємо підсвітку
    private void Zone_DragLeave(object? sender, EventArgs e)
    {
        if (sender is DropZone zone)
        {
            zone.Highlighted = false;
        }
    }

    // 5. Кнопку миші відпустили над зоною: кладемо картку
    private void Zone_DragDrop(object? sender, DragEventArgs e)
    {
        if (sender is not DropZone zone)
        {
            return;
        }

        zone.Highlighted = false;
        CardLabel? card = GetDraggedCard(e);

        if (card != null && CanAccept(card, zone))
        {
            PlaceCard(card, zone, e.Effect);
        }
    }

    // Яку дію (Move, Copy або None — «не можна») показати для картки над зоною
    private DragDropEffects GetDropEffect(DragEventArgs e, DropZone zone)
    {
        CardLabel? card = GetDraggedCard(e);

        if (card == null || !CanAccept(card, zone))
        {
            return DragDropEffects.None;
        }

        DragDropEffects wanted = card.IsTemplate ? DragDropEffects.Copy : DragDropEffects.Move;
        return e.AllowedEffect & wanted;
    }

    // Дістає картку з даних перетягування (null — тягнуть щось інше, наприклад файл)
    private CardLabel? GetDraggedCard(DragEventArgs e)
    {
        if (e.Data == null || !e.Data.GetDataPresent(typeof(CardLabel)))
        {
            return null;
        }

        CardLabel? card = e.Data.GetData(typeof(CardLabel)) as CardLabel;
        return card != null && cards.Contains(card) ? card : null;   // лише картки цього питання
    }

    // ===================== Розміщення карток =====================

    // Чи можна покласти картку в зону (нащадки можуть заборонити)
    protected virtual bool CanAccept(CardLabel card, DropZone zone)
    {
        return true;
    }

    // Кладе картку в зону. Цей самий метод викликається з обробника DragDrop
    public virtual void PlaceCard(CardLabel card, DropZone zone, DragDropEffects effect)
    {
        if (card.Parent == zone)
        {
            return;   // кинули туди ж, звідки взяли
        }

        // Зона заповнена — картка, що там лежала, повертається додому
        if (zone.IsFull && HomeZone != null)
        {
            foreach (CardLabel old in zone.Cards)
            {
                HomeZone.Controls.Add(old);
            }
        }

        zone.Controls.Add(card);   // Controls.Add сам забирає картку з попередньої зони
        OnAnswerChanged();
    }

    protected void OnAnswerChanged()
    {
        AnswerChanged?.Invoke(this, EventArgs.Empty);
    }

    // ===================== Допоміжне для розмітки =====================

    // Розмір у пікселях з урахуванням масштабу екрана (125 %, 150 %)
    protected int S(int value)
    {
        return LogicalToDeviceUnits(value);
    }

    // Перемішує елементи (алгоритм Фішера–Єйтса)
    protected List<T> Shuffle<T>(IEnumerable<T> items)
    {
        List<T> list = items.ToList();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    // Заголовок області: «Слайд», «Кошик», «Картки» ...
    protected Label CreateHeader(string title)
    {
        return new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = S(28),
            Font = Theme.SectionFont,
            ForeColor = Theme.Primary,
            Margin = new Padding(S(6), S(6), S(6), 0),
            UseMnemonic = false
        };
    }

    // Область із заголовком і вмістом під ним
    protected Panel CreateSection(string title, Control content)
    {
        Panel section = new() { Dock = DockStyle.Fill, Margin = new Padding(S(6)) };

        content.Dock = DockStyle.Fill;
        section.Controls.Add(content);              // спочатку Fill, потім Top — так працює Dock
        section.Controls.Add(CreateHeader(title));
        return section;
    }

    // Дві колонки: ліва займає leftPercent відсотків ширини
    protected TableLayoutPanel CreateColumns(int leftPercent, Control left, Control right)
    {
        TableLayoutPanel layout = new() { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, leftPercent));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 - leftPercent));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(left, 0, 0);
        layout.Controls.Add(right, 1, 0);
        return layout;
    }
}
