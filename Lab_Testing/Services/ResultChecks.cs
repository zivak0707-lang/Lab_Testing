namespace Lab_Testing;

// Пояснення для вчителя: стан файлу словами і звірка даних усередині файлу.
// Підпис каже лише «файл змінено». Звірка підказує, ЩО саме не сходиться, якщо змінили числа:
// бали за питанням, суму, відсоток, оцінку чи час проходження
public static class ResultChecks
{
    // Стан файлу коротко (таблиця, Excel, CSV)
    public static string StatusText(ResultFileStatus status)
    {
        return status switch
        {
            ResultFileStatus.Valid => "✔ Справжній",
            ResultFileStatus.DefaultKey => "⚠ Загальний ключ",
            ResultFileStatus.UnknownKey => "⚠ Інший ключ",
            ResultFileStatus.Modified => "✗ Змінено!",
            _ => "⚠ Пошкоджено"
        };
    }

    // Чому файлу не можна повністю довіряти (попередження перед подробицями)
    public static string WarningText(ResultFileStatus status)
    {
        return status switch
        {
            ResultFileStatus.DefaultKey =>
                "Увага: файл підписано загальним ключем програми, а не вашим. Учень проходив тест без вашого банку " +
                "(questions.dat), тому справжність файлу гарантувати не можна.",
            ResultFileStatus.UnknownKey =>
                "Увага: файл підписано ключем, якого на цьому комп'ютері немає (банк іншого вчителя або підробка). " +
                "Якщо це ваш давній ключ — відновіть його у вікні «Банк питань і ключ».",
            _ => "Увага: цей файл змінено після того, як програма його зберегла, — дані в ньому недостовірні."
        };
    }

    // Повна примітка для таблиці, Excel і CSV: стан файлу + повторні спроби й написання ПІБ
    public static string FullNote(ResultEntry entry, BankSettings settings, AttemptInfo? attempts)
    {
        return Join(Note(entry, settings), AttemptGroups.Note(attempts));
    }

    // Примітка для вчителя до файлу (порожня — файл справжній)
    public static string Note(ResultEntry entry, BankSettings settings)
    {
        List<string> mismatches = entry.Result != null ? FindMismatches(entry.Result, settings) : new();
        string details = mismatches.Count > 0 ? "Не сходиться: " + string.Join("; ", mismatches) + "." : "";

        return entry.Status switch
        {
            ResultFileStatus.Valid => "",
            ResultFileStatus.DefaultKey => Join("Учень проходив тест без вашого банку (questions.dat).", details),
            ResultFileStatus.UnknownKey => Join("Підписано ключем, якого немає на цьому комп'ютері.", details),
            ResultFileStatus.Modified => mismatches.Count > 0
                ? details
                : "Змінено дані, які не можна звірити (ПІБ, групу, дату чи тексти) — що саме, визначити неможливо.",
            _ => "Файл не вдалося прочитати: він пошкоджений або це не файл результату."
        };
    }

    // Що в даних файлу не сходиться між собою
    public static List<string> FindMismatches(TestResult result, BankSettings settings)
    {
        List<string> found = new();

        for (int i = 0; i < result.Questions.Count; i++)
        {
            QuestionResult q = result.Questions[i];
            string name = $"П{i + 1}";

            if (q.Score < 0 || q.Score > q.MaxScore)
            {
                found.Add($"{name}: {F(q.Score)} балів при максимумі {F(q.MaxScore)}");
            }
            else if (q.Total > 0)
            {
                double expected = Grading.Points(q.Correct, q.Total);

                if (Differs(q.Score, expected))
                {
                    found.Add($"{name}: у файлі {F(q.Score)} балів, а за «правильно {q.Correct} з {q.Total}» — {F(expected)}");
                }
            }
        }

        double sum = Math.Round(result.Questions.Sum(q => q.Score), 1);
        double max = result.Questions.Sum(q => q.MaxScore);
        double percent = max > 0 ? Math.Round(sum / max * 100, 1) : 0;
        int grade = Grading.Grade(percent, settings);

        if (Differs(result.TotalScore, sum))
        {
            found.Add($"сума балів у файлі {F(result.TotalScore)}, а за питаннями — {F(sum)}");
        }

        if (Differs(result.Percent, percent))
        {
            found.Add($"відсоток у файлі {F(result.Percent)} %, а за балами — {F(percent)} %");
        }

        if (result.Grade != grade)
        {
            found.Add($"оцінка у файлі {result.Grade}, а за балами — {grade} (за вашою шкалою)");
        }

        int seconds = (int)(result.FinishedAt - result.StartedAt).TotalSeconds;

        if (Math.Abs(seconds - result.DurationSeconds) > 1)
        {
            found.Add($"час проходження у файлі {Grading.FormatDuration(result.Duration)}, " +
                      $"а за початком і кінцем — {Grading.FormatDuration(TimeSpan.FromSeconds(seconds))}");
        }

        return found;
    }

    private static bool Differs(double a, double b)
    {
        return Math.Abs(a - b) > 0.05;
    }

    private static string F(double value)
    {
        return Grading.Format(value);
    }

    private static string Join(string first, string second)
    {
        return second.Length == 0 ? first : first.Length == 0 ? second : first + " " + second;
    }
}
