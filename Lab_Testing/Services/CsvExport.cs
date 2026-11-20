using System.Globalization;
using System.Text;

namespace Lab_Testing;

// Експорт таблиці результатів у CSV — запасний варіант для інших програм (основний — книга Excel, ExcelExport):
//  - кодування UTF-8 з BOM — за цією позначкою Excel розуміє, що в файлі кирилиця;
//  - роздільник стовпців і десятковий знак — з регіональних налаштувань Windows (ті самі бере Excel),
//    тому на будь-якому комп'ютері дані розкладаються по стовпцях
public static class CsvExport
{
    public static void Save(string path, IEnumerable<ResultEntry> entries, BankSettings settings)
    {
        CultureInfo culture = CultureInfo.CurrentCulture;
        string separator = culture.TextInfo.ListSeparator;
        StringBuilder sb = new();

        List<string> header = new() { "№", "ПІБ", "Група", "Дата", "Час проходження", "Спроба" };
        header.AddRange(Enum.GetValues<QuestionType>().Select((t, i) => $"П{i + 1} {t.ToUkrainian()}"));
        header.AddRange(new[] { "Разом", "З", "%", "Оцінка", "Рівень", "Порушення", "Завершення", "Файл справжній", "Примітка", "Файл" });
        AppendRow(sb, header, separator);

        int number = 1;
        Dictionary<ResultEntry, AttemptInfo> attempts = AttemptGroups.Analyze(entries.ToList());   // повторні спроби

        foreach (ResultEntry entry in entries)
        {
            List<string> row = new() { number++.ToString() };
            TestResult? r = entry.Result;

            if (r == null)
            {
                // Пошкоджений файл: даних немає, лише його стан, примітка і назва
                row.AddRange(Enumerable.Repeat("", header.Count - 4));
            }
            else
            {
                row.Add(r.StudentName);
                row.Add(r.Group);
                row.Add(r.FinishedAt.ToString("dd.MM.yyyy HH:mm", culture));
                row.Add(Grading.FormatDuration(r.Duration));
                row.Add((attempts.GetValueOrDefault(entry) is { Number: > 0 } attempt ? attempt.Number : r.AttemptNumber).ToString());
                row.AddRange(r.Questions.Select(q => q.Score.ToString("0.#", culture)));
                row.Add(r.TotalScore.ToString("0.#", culture));
                row.Add(r.MaxScore.ToString("0.#", culture));
                row.Add(r.Percent.ToString("0.#", culture));
                row.Add(r.Grade.ToString());
                row.Add(Grading.Level(r.Grade));
                row.Add(IntegrityText.Summary(r));
                row.Add(IntegrityText.FinishText(r));
            }

            row.Add(ResultChecks.StatusText(entry.Status));
            row.Add(ResultChecks.FullNote(entry, settings, attempts.GetValueOrDefault(entry)));
            row.Add(Path.GetFileName(entry.FilePath));
            AppendRow(sb, row, separator);
        }

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    // Рядок CSV: значення з роздільником, лапками чи переносом беремо в лапки
    private static void AppendRow(StringBuilder sb, IEnumerable<string> values, string separator)
    {
        sb.AppendLine(string.Join(separator, values.Select(v => Escape(v, separator))));
    }

    private static string Escape(string value, string separator)
    {
        bool needQuotes = value.Contains(separator) || value.Contains('"') || value.Contains('\n');
        return needQuotes ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
    }
}
