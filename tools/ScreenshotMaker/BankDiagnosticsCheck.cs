using System.Text.RegularExpressions;
using Lab_Testing;

namespace ScreenshotMaker;

// Самоперевірка пояснень помилок у банку: типові помилки вчителя -> зрозумілий текст українською
internal static class BankDiagnosticsCheck
{
    public static void Run()
    {
        string template = QuestionBank.ReadTemplateJson();

        Expect(Replace(template, "\"## Суть\",", "\"## Суть,"), "пропущена лапка",
            "не закрито лапку", "питання №1", "«теорія»", "Рядок ");

        Expect(Replace(template, "\"title\": \"Типи даних\",", "\"title\": \"Типи даних\""), "пропущена кома",
            "не вистачає коми", "Рядок ");

        Expect(Replace(template, "\"title\": \"Оператор → дія\"", "\"titel\": \"Оператор → дія\""), "одруківка в назві поля",
            "невідоме поле «titel»", "«title»", "Питання №2");

        Expect(Replace(template, "{ \"text\": \"int\", \"keep\": true }", "{ \"text\": \"int\", \"keep\": \"так\" }"), "неправильний тип значення",
            "true або false", "Питання №1");

        Expect(template[..template.LastIndexOf('}')], "незакрита дужка",
            "дужку");

        Expect(Replace(template, "[\"Числа\", \"Текст\"]", "[“Числа”, \"Текст\"]"), "друкарські лапки",
            "друкарські");

        Console.WriteLine("  Помилки в банку: 6 типових помилок пояснено українською, з рядком і полем, без технічного тексту");
    }

    private static string Replace(string text, string from, string to)
    {
        return text.Contains(from) ? text.Replace(from, to) : throw new InvalidOperationException("У шаблоні немає: " + from);
    }

    // Банк з помилкою має не завантажитися, а пояснення — містити потрібні слова і не містити англійського тексту
    private static void Expect(string json, string what, params string[] phrases)
    {
        string message;

        try
        {
            QuestionBank.Parse(json, "банк");
            throw new InvalidOperationException($"Помилки в банку ({what}): банк з помилкою завантажився");
        }
        catch (BankException ex)
        {
            message = ex.Message;
        }

        foreach (string phrase in phrases)
        {
            if (!message.Contains(phrase))
            {
                throw new InvalidOperationException($"Помилки в банку ({what}): у поясненні немає «{phrase}»:\n{message}");
            }
        }

        if (Regex.IsMatch(message, @"\b(invalid|Expected|Path|LineNumber|BytePosition|JSON value|0x0A)\b"))
        {
            throw new InvalidOperationException($"Помилки в банку ({what}): у поясненні є технічний текст англійською:\n{message}");
        }
    }
}
