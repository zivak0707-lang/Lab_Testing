using System.Text;
using Lab_Testing;

namespace ScreenshotMaker;

// Створює docs/bank_questions.md — список усіх питань банку з правильними відповідями (для перевірки і звіту)
internal static class BankListWriter
{
    public static void Write(string docsDir)
    {
        QuestionBankData bank = QuestionBank.Parse(QuestionBank.ReadEmbeddedJson(), "вбудований банк");
        StringBuilder sb = new();

        sb.AppendLine("# Банк питань з правильними відповідями");
        sb.AppendLine();
        sb.AppendLine($"Файл: `Lab_Testing/Data/questions.json`. Питань: {bank.Questions.Count}. " +
                      $"Час контрольної: {bank.Settings.ControlTimeMinutes} хв.");
        sb.AppendLine("Кожен тест бере по одному випадковому питанню кожного типу. Список згенеровано автоматично з файлу банку.");

        foreach (QuestionType type in Enum.GetValues<QuestionType>())
        {
            sb.AppendLine();
            sb.AppendLine($"## {type.ToUkrainian()} ({type})");

            foreach (Question q in bank.Questions.Where(q => q.Type == type))
            {
                sb.AppendLine();
                sb.AppendLine($"### №{q.Id}. {q.Title} — *{q.Topic}*");
                sb.AppendLine();
                sb.AppendLine($"**Завдання:** {q.Task}");
                sb.AppendLine();
                AppendAnswer(sb, q);
            }
        }

        File.WriteAllText(Path.Combine(docsDir, "bank_questions.md"), sb.ToString(), new UTF8Encoding(false));
    }

    private static void AppendAnswer(StringBuilder sb, Question q)
    {
        switch (q.Type)
        {
            case QuestionType.Basket:
                sb.AppendLine("- Залишити на слайді: " + Codes(q.Items.Where(i => i.Keep).Select(i => i.Text)));
                sb.AppendLine("- У кошик (зайві): " + Codes(q.Items.Where(i => !i.Keep).Select(i => i.Text)));
                break;

            case QuestionType.Table:
                sb.AppendLine($"| {q.Columns[0]} | {q.Columns[1]} |");
                sb.AppendLine("|---|---|");

                foreach (TableRow row in q.Rows)
                {
                    sb.AppendLine($"| {Code(row.Left)} | {Code(row.Answer)} |");
                }

                sb.AppendLine();
                sb.AppendLine("Зайві картки: " + Codes(q.ExtraCards));
                break;

            case QuestionType.Categories:
                foreach (string category in q.Categories)
                {
                    sb.AppendLine($"- **{category}**: " + Codes(q.Items.Where(i => i.Category == category).Select(i => i.Text)));
                }

                break;

            case QuestionType.Gaps:
                // Правильні слова в тексті виділяються жирним у квадратних дужках
                string text = System.Text.RegularExpressions.Regex.Replace(q.Text, @"\{([^{}]+)\}", "**[$1]**");
                sb.AppendLine("> " + text);
                sb.AppendLine();
                sb.AppendLine("Зайві слова: " + Codes(q.ExtraWords));
                break;

            case QuestionType.Order:
                for (int i = 0; i < q.Steps.Count; i++)
                {
                    sb.AppendLine($"{i + 1}. {Code(q.Steps[i])}");
                }

                break;
        }
    }

    private static string Code(string text)
    {
        return "`" + text.Replace("`", "'") + "`";
    }

    private static string Codes(IEnumerable<string> texts)
    {
        return string.Join(", ", texts.Select(Code));
    }
}
