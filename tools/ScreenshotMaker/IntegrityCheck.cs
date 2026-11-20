using System.Text.RegularExpressions;
using Lab_Testing;

namespace ScreenshotMaker;

// Самоперевірки режиму доброчесності без вікон: облік виходів, тексти для вчителя, підпис, налаштування банку
internal static class IntegrityCheck
{
    public static void Run()
    {
        CheckMonitor();
        CheckTexts();
        CheckBankSettings();
        Console.WriteLine("  Доброчесність: облік виходів (короткі, ліміт, серії коротких, вихід до кінця тесту, знімки), " +
                          "тексти для вчителя, налаштування банку — правильно");
    }

    // Облік виходів: поріг короткого виходу, ліміт, серії коротких виходів, незавершений вихід
    private static void CheckMonitor()
    {
        DateTime t = new(2026, 11, 20, 10, 0, 0);
        IntegrityMonitor m = new(new IntegritySettings { ExitLimit = 3, ShortExitSeconds = 2 });

        Expect(Exit(m, t, 1.5, 1)?.Kind == IntegrityEventKind.ShortExit, "1,5 с — короткий вихід");
        Expect(m.Report.Exits == 0 && m.ExitsLeft == 3, "короткий вихід не рахується");

        Expect(Exit(m, t.AddSeconds(10), 12, 2)?.Kind == IntegrityEventKind.Exit, "12 с — звичайний вихід");
        Expect(m.Report.Exits == 1 && m.ExitsLeft == 2 && !m.LimitReached, "після 1-го виходу лишилось 2");
        Expect(m.Report.Events.Last().Question == 2 && m.Report.Events.Last().Seconds == 12, "вихід: питання і тривалість");

        Expect(m.Return(t.AddSeconds(40)) == null, "повернення без виходу нічого не записує");

        // Ще 4 короткі виходи — разом 5, усі пробачені; 6-й короткий уже рахується
        for (int i = 0; i < 4; i++)
        {
            Exit(m, t.AddSeconds(60 + i * 5), 0.5, 3);
        }

        Expect(m.Report.ShortExits == IntegrityMonitor.FreeShortExits && m.Report.Exits == 1, "5 коротких не рахуються");
        Expect(Exit(m, t.AddSeconds(100), 0.5, 3)?.Kind == IntegrityEventKind.Exit, "6-й короткий вихід рахується");
        Expect(m.Report.Exits == 2 && m.ExitsLeft == 1, "після 6-го короткого лишився 1 вихід");

        // Контрольна закінчилась, поки учень поза вікном: вихід триває до кінця
        m.Leave(t.AddSeconds(200), 4);
        m.Close(t.AddSeconds(230));
        Expect(m.Report.Exits == 3 && m.LimitReached && m.Report.Events.Last().Seconds == 30, "вихід до кінця тесту");

        // Знімок: клавіша і буфер обміну за 3 с — один знімок; через хвилину — другий
        m.AddScreenshot(t.AddSeconds(300), 5, "клавіша Print Screen");
        m.AddScreenshot(t.AddSeconds(301), 5, "зображення в буфері обміну");
        m.AddScreenshot(t.AddSeconds(360), 5, "зображення в буфері обміну");
        Expect(m.Report.Screenshots == 2, "два знімки екрана (перший — з двох сигналів)");

        // Без ліміту: виходи лише записуються
        IntegrityMonitor free = new(new IntegritySettings { ExitLimit = 0, ShortExitSeconds = 2 });

        for (int i = 0; i < 10; i++)
        {
            Exit(free, t.AddMinutes(i), 5, 1);
        }

        Expect(free.Report.Exits == 10 && free.ExitsLeft == null && !free.LimitReached, "exitLimit 0 — без дострокового завершення");
    }

    private static IntegrityEvent? Exit(IntegrityMonitor m, DateTime at, double seconds, int question)
    {
        m.Leave(at, question);
        return m.Return(at.AddSeconds(seconds));
    }

    // Тексти для таблиці вчителя
    private static void CheckTexts()
    {
        Expect(IntegrityText.Times(1) == "1 раз" && IntegrityText.Times(3) == "3 рази" && IntegrityText.Times(5) == "5 разів" &&
               IntegrityText.Times(11) == "11 разів" && IntegrityText.Times(22) == "22 рази", "відмінювання «раз»");

        DateTime t = new(2026, 11, 20, 10, 0, 0);
        IntegrityMonitor m = new(new IntegritySettings { ExitLimit = 0, ShortExitSeconds = 2 });
        Exit(m, t, 20, 1);
        Exit(m, t.AddMinutes(1), 20, 2);
        Exit(m, t.AddMinutes(2), 7, 3);
        Exit(m, t.AddMinutes(3), 1, 3);
        Exit(m, t.AddMinutes(4), 1, 4);
        m.Report.Monitors = 1;

        TestResult result = new() { Mode = TestMode.Control, Integrity = m.Report, Questions = new() };
        string summary = IntegrityText.Summary(result);
        Expect(summary == "вийшов 3 рази, разом 47 с; коротких 2", "стовпець «Порушення»: " + summary);
        Expect(IntegrityText.HasViolations(result), "виходи — це порушення");

        IntegrityMonitor shortOnly = new(new IntegritySettings());
        Exit(shortOnly, t, 1, 1);
        Expect(!IntegrityText.HasViolations(new TestResult { Integrity = shortOnly.Report }), "лише короткі виходи — не порушення");
        Expect(IntegrityText.Summary(new TestResult { Mode = TestMode.Control }) == "не відстежувалось", "файл без режиму доброчесності");
        Expect(IntegrityText.Timeline(result)[0] == "10:00:00 — вийшов з вікна тесту на 20 с (питання 1)", "хронологія");
    }

    // Налаштування в банку: шаблон читається, помилки в розділі integrity пояснюються
    private static void CheckBankSettings()
    {
        QuestionBankData template = QuestionBank.Parse(QuestionBank.ReadTemplateJson(), "шаблон");
        Expect(template.Settings.Integrity is { Enabled: true, ExitLimit: 3, ShortExitSeconds: 2, HideFromCapture: false },
            "шаблон: налаштування доброчесності за замовчуванням");

        string json = QuestionBank.ReadTemplateJson();
        ExpectError(json.Replace("\"exitLimit\": 3", "\"exitLimt\": 3"), "невідоме поле «exitLimt»", "exitLimit");
        ExpectError(json.Replace("\"exitLimit\": 3", "\"exitLimit\": 50"), "exitLimit має бути від 0 до 20");
        ExpectError(Regex.Replace(json, "\"integrity\": \\{[^}]*\\}", "\"integrity\": true"), "потрібні фігурні дужки");
    }

    private static void ExpectError(string json, params string[] phrases)
    {
        try
        {
            QuestionBank.Parse(json, "банк");
        }
        catch (BankException ex)
        {
            foreach (string phrase in phrases)
            {
                Expect(ex.Message.Contains(phrase), $"помилка налаштувань: немає «{phrase}» у\n{ex.Message}");
            }

            return;
        }

        throw new InvalidOperationException("Доброчесність: банк з помилкою в налаштуваннях завантажився");
    }

    private static void Expect(bool condition, string what)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Доброчесність: не виконано — " + what);
        }
    }
}
