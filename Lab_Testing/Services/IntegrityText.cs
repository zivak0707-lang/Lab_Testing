namespace Lab_Testing;

// Тексти режиму доброчесності для людей: стовпець «Порушення», хронологія подій, причина завершення
public static class IntegrityText
{
    // Питання, на яке пішло менше стількох секунд при 75 % балів і більше, позначаємо «дуже швидко»
    public const int FastSeconds = 8;

    // Коротко для таблиці: «вийшов 3 рази, разом 47 с; коротких 2»
    public static string Summary(TestResult result)
    {
        IntegrityReport? report = result.Integrity;

        if (report == null)
        {
            return result.Mode == TestMode.Control ? "не відстежувалось" : "";
        }

        List<string> parts = new();
        List<IntegrityEvent> exits = report.Events.Where(e => e.Kind == IntegrityEventKind.Exit).ToList();

        if (exits.Count > 0)
        {
            parts.Add($"вийшов {Times(exits.Count)}, разом {Duration(exits.Sum(e => e.Seconds))}");
        }

        if (report.ShortExits > 0)
        {
            parts.Add($"коротких {report.ShortExits}");
        }

        if (report.Screenshots > 0)
        {
            parts.Add($"знімок екрана: {report.Screenshots}");
        }

        int copies = report.Events.Count(e => e.Kind == IntegrityEventKind.SecondCopy);

        if (copies > 0)
        {
            parts.Add($"друга копія програми: {copies}");
        }

        if (result.Reason == FinishReason.ExitLimit)
        {
            parts.Add("завершено достроково");
        }

        parts.AddRange(Signals(result));
        return parts.Count > 0 ? string.Join("; ", parts) : "немає";
    }

    // Чи є порушення (рядок у таблиці виділяється). Короткі виходи і сигнали самі по собі — не порушення
    public static bool HasViolations(TestResult result)
    {
        IntegrityReport? report = result.Integrity;

        return report != null && (report.Exits > 0 || report.Screenshots > 0 || result.Reason == FinishReason.ExitLimit ||
                                  report.Events.Any(e => e.Kind == IntegrityEventKind.SecondCopy));
    }

    // Сигнали для вчителя — не порушення, але варто знати: кілька моніторів, віддалений доступ, дуже швидкі відповіді
    public static List<string> Signals(TestResult result)
    {
        List<string> signals = new();
        IntegrityReport? report = result.Integrity;

        if (report == null)
        {
            return signals;
        }

        int monitors = Math.Max(report.Monitors, report.Events.Where(e => e.Kind == IntegrityEventKind.Monitors)
            .Select(e => int.TryParse(e.Details.Split(' ').Last(), out int n) ? n : 0).DefaultIfEmpty(0).Max());

        if (monitors > 1)
        {
            signals.Add($"моніторів: {monitors}");
        }

        if (report.RemoteSession || report.Events.Any(e => e.Kind == IntegrityEventKind.RemoteSession))
        {
            signals.Add("віддалений доступ");
        }

        List<int> fast = FastQuestions(result);

        if (fast.Count > 0)
        {
            signals.Add("дуже швидко: " + string.Join(", ", fast.Select(i => $"П{i + 1}")));
        }

        return signals;
    }

    // Номери питань (від 0), на які відповіли підозріло швидко і майже без помилок.
    // Час на питання записують лише файли з режимом доброчесності
    public static List<int> FastQuestions(TestResult result)
    {
        if (result.Integrity == null)
        {
            return new List<int>();
        }

        return result.Questions
            .Select((q, i) => (q, i))
            .Where(x => x.q.Seconds < FastSeconds && x.q.Score >= x.q.MaxScore * 0.75)
            .Select(x => x.i)
            .ToList();
    }

    // Чому завершено
    public static string FinishText(TestResult result)
    {
        if (result.Mode != TestMode.Control)
        {
            return "";
        }

        return result.Reason switch
        {
            FinishReason.TimeUp => "скінчився час",
            FinishReason.ExitLimit => "достроково: ліміт виходів",
            FinishReason.WindowClosed => "закрив вікно тесту",
            _ => "завершив сам"
        };
    }

    // Хронологія: «10:03:15 — вийшов з вікна тесту на 12 с (питання 2)»
    public static List<string> Timeline(TestResult result)
    {
        return result.Integrity?.Events.OrderBy(e => e.At).Select(e => $"{e.At:HH:mm:ss} — {Describe(e)}").ToList() ?? new();
    }

    // Налаштування, з якими проходили контрольну
    public static string SettingsText(IntegrityReport report)
    {
        string limit = report.ExitLimit > 0
            ? $"контрольна завершується після {report.ExitLimit}-го виходу з вікна"
            : "виходи лише записуються (без дострокового завершення)";

        return $"{Capitalize(limit)}; короткий вихід — до {report.ShortExitSeconds} с; " +
               (report.HideFromCapture ? $"захист від знімків екрана: {report.CaptureProtection}" : "захист від знімків екрана вимкнено") +
               $"; моніторів на початку: {report.Monitors}" + (report.RemoteSession ? "; віддалений робочий стіл" : "") + ".";
    }

    public static string Describe(IntegrityEvent e)
    {
        string question = e.Question > 0 ? $" (питання {e.Question})" : "";
        string details = e.Details.Length > 0 ? $" — {e.Details}" : "";

        return e.Kind switch
        {
            IntegrityEventKind.RulesAccepted => "підтвердив правила: «Я проходжу тест самостійно і чесно»",
            IntegrityEventKind.Exit => $"вийшов з вікна тесту на {Duration(e.Seconds)}{question}{details}",
            IntegrityEventKind.ShortExit => $"коротка втрата фокусу ({Duration(e.Seconds)}){question} — не рахується",
            IntegrityEventKind.Screenshot => $"можлива спроба знімка екрана{question}{details}",
            IntegrityEventKind.Monitors => $"змінилася кількість моніторів{details}",
            IntegrityEventKind.RemoteSession => $"віддалений доступ{details}",
            IntegrityEventKind.SecondCopy => "спроба відкрити ще одну копію програми (не відкрилася)",
            IntegrityEventKind.EarlyFinish => $"контрольну завершено достроково{details}",
            _ => e.Kind.ToString()
        };
    }

    // «менше 1 с», «12 с», «1 хв 05 с»
    public static string Duration(int seconds)
    {
        return seconds <= 0 ? "менше 1 с" : seconds < 60 ? $"{seconds} с" : $"{seconds / 60} хв {seconds % 60:00} с";
    }

    // «1 раз», «3 рази», «5 разів»
    public static string Times(int n)
    {
        int last = n % 10, lastTwo = n % 100;
        string word = last == 1 && lastTwo != 11 ? "раз"
            : last is >= 2 and <= 4 && lastTwo is < 12 or > 14 ? "рази"
            : "разів";
        return $"{n} {word}";
    }

    private static string Capitalize(string text)
    {
        return text.Length == 0 ? text : char.ToUpper(text[0]) + text[1..];
    }
}
