using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lab_Testing;

// Помилка у файлі банку питань (текст пояснює, що саме не так)
public class BankException : Exception
{
    public BankException(string message) : base(message)
    {
    }
}

// Завантаження і перевірка банку питань
public static class QuestionBank
{
    public const string JsonFileName = "questions.json";                // відкритий банк поруч з exe
    private const string ResourceName = "Lab_Testing.questions.json";   // банк, вшитий у програму
    private const int MaxErrorsToShow = 15;                             // скільки помилок показувати

    // Налаштування JSON: поля camelCase, тип питання — словом, кирилиця без \u-кодів,
    // дозволені коментарі // і зайва кома в кінці списку (щоб викладачу було легше редагувати)
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    // Тека, у якій лежить exe (працює і для exe з одного файлу)
    public static string ExeFolder => Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;

    // ===================== Завантаження =====================

    // Завантажує банк: файл questions.json поруч з exe, інакше — вбудований у програму.
    // source — звідки взято банк (для підпису на стартовій сторінці)
    public static QuestionBankData Load(out string source)
    {
        string jsonPath = Path.Combine(ExeFolder, JsonFileName);

        if (File.Exists(jsonPath))
        {
            source = "файл " + JsonFileName;
            return Parse(File.ReadAllText(jsonPath), JsonFileName);
        }

        source = "вбудований у програму";
        return Parse(ReadEmbeddedJson(), "вбудований банк");
    }

    // Текст банку, вшитого в exe
    public static string ReadEmbeddedJson()
    {
        using Stream stream = typeof(QuestionBank).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new BankException("У програмі не знайдено вбудованого банку питань.");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    // Перетворює текст JSON на банк і перевіряє його. sourceName — для повідомлень про помилки
    public static QuestionBankData Parse(string json, string sourceName)
    {
        QuestionBankData? bank;

        try
        {
            bank = JsonSerializer.Deserialize<QuestionBankData>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            long line = (ex.LineNumber ?? 0) + 1;
            throw new BankException($"{sourceName}: помилка запису JSON у рядку {line}.\n{ex.Message}");
        }

        if (bank == null)
        {
            throw new BankException($"{sourceName}: файл порожній.");
        }

        List<string> errors = Validate(bank);

        if (errors.Count > 0)
        {
            List<string> shown = errors.Take(MaxErrorsToShow).ToList();

            if (errors.Count > MaxErrorsToShow)
            {
                shown.Add($"... і ще {errors.Count - MaxErrorsToShow} помилок.");
            }

            throw new BankException($"{sourceName} містить помилки:\n• " + string.Join("\n• ", shown));
        }

        return bank;
    }

    // ===================== Перевірка =====================

    // Повертає список помилок банку (порожній список — банк правильний)
    public static List<string> Validate(QuestionBankData bank)
    {
        List<string> errors = new();
        ValidateSettings(bank.Settings, errors);

        if (bank.Questions.Count == 0)
        {
            errors.Add("У банку немає жодного питання.");
            return errors;
        }

        // Номери питань не повинні повторюватися
        foreach (var group in bank.Questions.GroupBy(q => q.Id).Where(g => g.Count() > 1))
        {
            errors.Add($"Номер питання {group.Key} повторюється {group.Count()} рази.");
        }

        foreach (Question q in bank.Questions)
        {
            string prefix = $"Питання №{q.Id} ({q.Type}): ";

            if (string.IsNullOrWhiteSpace(q.Task))
            {
                errors.Add(prefix + "немає тексту завдання (task).");
            }

            // Блоки коду ``` мають і відкриватися, і закриватися
            if (CountCodeFences(q.Theory) % 2 != 0)
            {
                errors.Add(prefix + "у теорії (theory) блок коду ``` не закрито.");
            }

            if (CountCodeFences(q.Explanation) % 2 != 0)
            {
                errors.Add(prefix + "у поясненні (explanation) блок коду ``` не закрито.");
            }

            switch (q.Type)
            {
                case QuestionType.Basket: ValidateBasket(q, prefix, errors); break;
                case QuestionType.Table: ValidateTable(q, prefix, errors); break;
                case QuestionType.Categories: ValidateCategories(q, prefix, errors); break;
                case QuestionType.Gaps: ValidateGaps(q, prefix, errors); break;
                case QuestionType.Order: ValidateOrder(q, prefix, errors); break;
            }
        }

        // Для тесту потрібне хоча б одне питання кожного типу
        foreach (QuestionType type in Enum.GetValues<QuestionType>())
        {
            if (!bank.Questions.Any(q => q.Type == type))
            {
                errors.Add($"Немає жодного питання типу {type} («{type.ToUkrainian()}») — тест не можна скласти.");
            }
        }

        return errors;
    }

    private static void ValidateSettings(BankSettings s, List<string> errors)
    {
        if (s.ControlTimeMinutes < 1 || s.ControlTimeMinutes > 180)
        {
            errors.Add("Налаштування: controlTimeMinutes має бути від 1 до 180 хвилин.");
        }

        List<double> p = s.GradeMinPercent;
        bool ascending = p.Zip(p.Skip(1), (a, b) => a < b).All(x => x);

        if (p.Count != 12 || p[0] != 0 || !ascending || p[^1] > 100)
        {
            errors.Add("Налаштування: gradeMinPercent має містити 12 чисел, що починаються з 0 і зростають (не більше 100).");
        }
    }

    // «Кошик»: є і потрібні, і зайві картки
    private static void ValidateBasket(Question q, string prefix, List<string> errors)
    {
        if (q.Items.Count < 3)
        {
            errors.Add(prefix + "потрібно щонайменше 3 картки (items).");
        }

        if (!q.Items.Any(i => i.Keep) || !q.Items.Any(i => !i.Keep))
        {
            errors.Add(prefix + "мають бути і потрібні картки (keep: true), і зайві (keep: false).");
        }

        CheckTexts(q.Items.Select(i => i.Text), prefix + "картки", errors);
    }

    // «Таблиця»: два заголовки, щонайменше 2 рядки, усі картки різні
    private static void ValidateTable(Question q, string prefix, List<string> errors)
    {
        if (q.Columns.Count != 2)
        {
            errors.Add(prefix + "потрібно рівно 2 заголовки стовпців (columns).");
        }

        if (q.Rows.Count < 2)
        {
            errors.Add(prefix + "потрібно щонайменше 2 рядки (rows).");
        }

        if (q.Rows.Any(r => string.IsNullOrWhiteSpace(r.Left)))
        {
            errors.Add(prefix + "у кожного рядка має бути текст лівої клітинки (left).");
        }

        CheckTexts(q.Rows.Select(r => r.Answer).Concat(q.ExtraCards), prefix + "картки (answer та extraCards)", errors);
    }

    // «Номерки»: категорії існують, кожен елемент має правильну категорію зі списку
    private static void ValidateCategories(Question q, string prefix, List<string> errors)
    {
        if (q.Categories.Count < 2)
        {
            errors.Add(prefix + "потрібно щонайменше 2 категорії (categories).");
        }

        CheckTexts(q.Categories, prefix + "категорії", errors);

        if (q.Items.Count < 3)
        {
            errors.Add(prefix + "потрібно щонайменше 3 елементи (items).");
        }

        foreach (QuestionItem item in q.Items.Where(i => !q.Categories.Contains(i.Category)))
        {
            errors.Add(prefix + $"елемент «{item.Text}» має категорію «{item.Category}», якої немає у списку categories.");
        }

        CheckTexts(q.Items.Select(i => i.Text), prefix + "елементи", errors);
    }

    // «Пропуски»: є хоча б один пропуск, дужки парні, зайві слова не збігаються з правильними
    private static void ValidateGaps(Question q, string prefix, List<string> errors)
    {
        List<string> answers = q.GetGapAnswers();
        int open = q.Text.Count(c => c == '{');
        int close = q.Text.Count(c => c == '}');

        if (answers.Count == 0)
        {
            errors.Add(prefix + "у тексті (text) немає жодного пропуску виду {слово}.");
        }

        if (open != close || open != answers.Count)
        {
            errors.Add(prefix + "у тексті непарні або вкладені фігурні дужки.");
        }

        foreach (string word in q.ExtraWords.Where(w => answers.Contains(w.Trim())))
        {
            errors.Add(prefix + $"зайве слово «{word}» збігається з правильною відповіддю.");
        }

        CheckTexts(q.ExtraWords, prefix + "зайві слова (extraWords)", errors);
    }

    // «Порядок кроків»: від 3 до 8 різних кроків (більше не вміщується у вікні)
    private static void ValidateOrder(Question q, string prefix, List<string> errors)
    {
        if (q.Steps.Count < 3 || q.Steps.Count > 8)
        {
            errors.Add(prefix + "потрібно від 3 до 8 кроків (steps).");
        }

        CheckTexts(q.Steps, prefix + "кроки", errors);
    }

    private static int CountCodeFences(string text)
    {
        return text.Split('\n').Count(line => line.Trim() == "```");
    }

    // Тексти не порожні й не повторюються (інакше картки не відрізнити)
    private static void CheckTexts(IEnumerable<string> texts, string what, List<string> errors)
    {
        List<string> list = texts.Select(t => t.Trim()).ToList();

        if (list.Any(t => t.Length == 0))
        {
            errors.Add(what + ": є порожній текст.");
        }

        foreach (string duplicate in list.Where(t => t.Length > 0).GroupBy(t => t).Where(g => g.Count() > 1).Select(g => g.Key))
        {
            errors.Add(what + $": текст «{duplicate}» повторюється.");
        }
    }
}
