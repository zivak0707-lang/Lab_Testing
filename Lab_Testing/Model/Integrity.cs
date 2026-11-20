namespace Lab_Testing;

// Чому завершилася контрольна
public enum FinishReason
{
    Student,       // учень натиснув «Завершити контрольну»
    WindowClosed,  // учень закрив вікно тесту (✕ або Alt+F4)
    TimeUp,        // скінчився час
    ExitLimit      // достроково: вичерпано дозволену кількість виходів з вікна
}

// Що сталося під час контрольної
public enum IntegrityEventKind
{
    RulesAccepted,   // учень підтвердив правила («проходжу тест самостійно і чесно»)
    Exit,            // вихід з вікна тесту, який рахується
    ShortExit,       // короткий вихід (до порогу) — не рахується
    Screenshot,      // натиснуто Print Screen або в буфер обміну потрапило зображення
    Monitors,        // змінилася кількість моніторів
    RemoteSession,   // підключення через віддалений робочий стіл
    SecondCopy,      // спроба відкрити ще одну копію програми
    EarlyFinish      // контрольну завершено достроково через виходи
}

// Одна подія: коли, що, на якому питанні і (для виходів) скільки тривала
public class IntegrityEvent
{
    public DateTime At { get; set; }
    public IntegrityEventKind Kind { get; set; }
    public int Question { get; set; }       // номер питання 1–5 (0 — не стосується питання)
    public int Seconds { get; set; }        // тривалість виходу, секунд
    public string Details { get; set; } = "";
}

// Усе, що записано в режимі доброчесності (зберігається у файлі результату під підписом)
public class IntegrityReport
{
    // Налаштування, з якими проходили контрольну
    public int ExitLimit { get; set; }
    public int ShortExitSeconds { get; set; }
    public bool HideFromCapture { get; set; }

    // Чи вдалося сховати вікно від знімків: «сховано», «чорний прямокутник», «недоступно» або «вимкнено»
    public string CaptureProtection { get; set; } = "";

    // Обставини на початку контрольної
    public int Monitors { get; set; }
    public bool RemoteSession { get; set; }

    public List<IntegrityEvent> Events { get; set; } = new();

    // Підсумки (для таблиці вчителя)
    public int Exits { get; set; }           // зараховані виходи
    public int ShortExits { get; set; }      // короткі, які не рахуються
    public int AwaySeconds { get; set; }     // скільки разом був поза вікном (усі виходи)
    public int Screenshots { get; set; }
}

// Облік виходів з вікна тесту. Лише логіка, без вікон — тому її можна перевірити окремо
public class IntegrityMonitor
{
    // Скільки коротких виходів пробачаємо. Далі кожен короткий рахується як звичайний:
    // інакше можна було б підглядати серіями по одній секунді
    public const int FreeShortExits = 5;

    // Два сигнали про знімок (клавіша і буфер обміну) протягом цього часу — це один знімок
    private static readonly TimeSpan ScreenshotMerge = TimeSpan.FromSeconds(3);

    private DateTime? leftAt;       // коли учень вийшов (null — зараз у вікні тесту)
    private int leftQuestion;       // на якому питанні вийшов

    public IntegrityMonitor(IntegritySettings settings)
    {
        Settings = settings;
        Report = new IntegrityReport
        {
            ExitLimit = settings.ExitLimit,
            ShortExitSeconds = settings.ShortExitSeconds,
            HideFromCapture = settings.HideFromCapture
        };
    }

    public IntegritySettings Settings { get; }
    public IntegrityReport Report { get; }

    // Учень зараз поза вікном тесту
    public bool IsAway => leftAt != null;

    // Скільки ще зарахованих виходів до дострокового завершення (null — обмеження немає)
    public int? ExitsLeft => Settings.ExitLimit > 0 ? Math.Max(0, Settings.ExitLimit - Report.Exits) : null;

    // Вичерпано ліміт виходів — контрольну треба завершити
    public bool LimitReached => ExitsLeft == 0;

    public void Add(IntegrityEventKind kind, DateTime at, int question = 0, string details = "", int seconds = 0)
    {
        Report.Events.Add(new IntegrityEvent { At = Trim(at), Kind = kind, Question = question, Details = details, Seconds = seconds });
    }

    // Вікно тесту втратило фокус: активною стала інша програма
    public void Leave(DateTime at, int question)
    {
        if (leftAt == null)
        {
            leftAt = at;
            leftQuestion = question;
        }
    }

    // Учень повернувся у вікно тесту. Повертає записаний вихід (null — виходу не було)
    public IntegrityEvent? Return(DateTime at)
    {
        if (leftAt == null)
        {
            return null;
        }

        double seconds = Math.Max(0, (at - leftAt.Value).TotalSeconds);
        bool isShort = seconds < Settings.ShortExitSeconds;
        string details = "";

        // Короткий вихід не рахується, доки таких не забагато
        if (isShort && Report.ShortExits >= FreeShortExits)
        {
            isShort = false;
            details = $"короткий, але коротких виходів понад {FreeShortExits} — зараховано";
        }

        IntegrityEvent exit = new()
        {
            At = Trim(leftAt.Value),
            Kind = isShort ? IntegrityEventKind.ShortExit : IntegrityEventKind.Exit,
            Question = leftQuestion,
            Seconds = (int)Math.Round(seconds),
            Details = details
        };

        Report.Events.Add(exit);
        Report.AwaySeconds += exit.Seconds;

        if (isShort)
        {
            Report.ShortExits++;
        }
        else
        {
            Report.Exits++;
        }

        leftAt = null;
        return exit;
    }

    // Знімок екрана: клавіша Print Screen або зображення в буфері обміну (дубль за 3 с — не рахуємо)
    public void AddScreenshot(DateTime at, int question, string how)
    {
        IntegrityEvent? last = Report.Events.LastOrDefault(e => e.Kind == IntegrityEventKind.Screenshot);

        if (last != null && Trim(at) - last.At <= ScreenshotMerge)
        {
            if (!last.Details.Contains(how))
            {
                last.Details += ", " + how;
            }

            return;
        }

        Report.Screenshots++;
        Add(IntegrityEventKind.Screenshot, at, question, how);
    }

    // Контрольна завершилась, коли учень був поза вікном: вихід триває до кінця контрольної
    public void Close(DateTime at)
    {
        Return(at);
    }

    // Час у файлі — з точністю до секунди (як і решта часу в результаті)
    private static DateTime Trim(DateTime time)
    {
        return new DateTime(time.Ticks - time.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Unspecified);
    }
}
