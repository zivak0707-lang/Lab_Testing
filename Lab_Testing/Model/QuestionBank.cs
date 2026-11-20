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

    // Текст чинного банку (для кнопки «Зберегти банк для редагування»)
    public static string CurrentJson { get; private set; } = "";

    // Банк вчителя (questions.dat), якщо програму запущено з ним; null — банк вчителя не підключено
    public static BankPackage? TeacherPackage { get; private set; }

    // Завантажує банк. Порядок пошуку поруч з exe:
    //   questions.dat  — банк, підготовлений вчителем для учнів (з ключем підпису вчителя);
    //   questions.json — відкритий банк (наприклад, вчитель його редагує);
    //   інакше — банк, вшитий у програму.
    // source — звідки взято банк (для підпису на стартовій сторінці)
    public static QuestionBankData Load(out string source)
    {
        string datPath = Path.Combine(ExeFolder, BankFile.FileName);
        string jsonPath = Path.Combine(ExeFolder, JsonFileName);

        if (File.Exists(datPath))
        {
            BankPackage package = BankFile.Read(datPath);
            QuestionBankData bank = Parse(package.BankJson, BankFile.FileName);

            // Файли результатів підписуватимуться ключем вчителя, а режим викладача — паролем вчителя
            TeacherPackage = package;
            ResultFile.SigningKey = Convert.FromBase64String(package.SigningKey);
            CurrentJson = package.BankJson;
            source = $"банк вчителя, підготовлено {package.PreparedAt:dd.MM.yyyy}";
            return bank;
        }

        if (File.Exists(jsonPath))
        {
            CurrentJson = File.ReadAllText(jsonPath);
            source = "файл " + JsonFileName;
            return Parse(CurrentJson, JsonFileName);
        }

        CurrentJson = ReadEmbeddedJson();
        source = "вбудований у програму";
        return Parse(CurrentJson, "вбудований банк");
    }

    // Текст банку, вшитого в exe
    public static string ReadEmbeddedJson()
    {
        return ReadResource(ResourceName);
    }

    // Шаблон банку для вчителя (по одному простому питанню кожного типу)
    public static string ReadTemplateJson()
    {
        return ReadResource("Lab_Testing.bank_template.json");
    }

    private static string ReadResource(string name)
    {
        using Stream stream = typeof(QuestionBank).Assembly.GetManifestResourceStream(name)
            ?? throw new BankException($"У програмі не знайдено ресурсу {name}.");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    // Ті самі послаблення, що й під час читання банку: коментарі // і зайва кома в кінці списку
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    // Перетворює текст JSON на банк і перевіряє його. sourceName — для повідомлень про помилки.
    // Три кроки: запис файлу (лапки, коми, дужки) -> будова (поля і їхній вигляд) -> зміст питань
    public static QuestionBankData Parse(string json, string sourceName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new BankException($"{sourceName}: файл порожній.");
        }

        // 1. Запис файлу. Перевіряємо окремо, до читання в класи, щоб пояснити помилку людською мовою
        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(json, DocumentOptions);
        }
        catch (JsonException ex)
        {
            throw Errors(sourceName, BankDiagnostics.DescribeSyntaxError(json, ex.LineNumber));
        }

        // 2. Будова: потрібні поля є, значення правильного вигляду, невідомих полів немає
        using (document)
        {
            List<string> structure = BankDiagnostics.CheckStructure(document.RootElement, json);

            if (structure.Count > 0)
            {
                throw Errors(sourceName, structure);
            }
        }

        // 3. Читання в класи (після кроків 1–2 тут уже не буває несподіванок) і перевірка змісту
        QuestionBankData? bank;

        try
        {
            bank = JsonSerializer.Deserialize<QuestionBankData>(json, JsonOptions);
        }
        catch (JsonException)
        {
            throw Errors(sourceName, new List<string> { "дані банку не вдалося прочитати: перевірте числа і значення true/false." });
        }

        if (bank == null)
        {
            throw new BankException($"{sourceName}: файл порожній.");
        }

        List<string> errors = Validate(bank, json);

        if (errors.Count > 0)
        {
            throw Errors(sourceName, errors);
        }

        return bank;
    }

    // Помилка зі списком пунктів (не більше MaxErrorsToShow)
    private static BankException Errors(string sourceName, List<string> errors)
    {
        List<string> shown = errors.Take(MaxErrorsToShow).ToList();

        if (errors.Count > MaxErrorsToShow)
        {
            shown.Add($"... і ще {errors.Count - MaxErrorsToShow} помилок.");
        }

        return new BankException($"{sourceName} містить помилки:\n• " + string.Join("\n• ", shown));
    }

    // ===================== Перевірка змісту =====================

    // Повертає список помилок банку (порожній список — банк правильний).
    // json — текст банку, щоб указати рядок, де починається питання
    public static List<string> Validate(QuestionBankData bank, string json = "")
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
            int line = BankDiagnostics.FindQuestionLine(json, q.Id);
            string where = line > 0 ? $", рядок {line}" : "";
            string prefix = $"Питання №{q.Id} («{q.Type.ToUkrainian()}»{where}): ";

            if (string.IsNullOrWhiteSpace(q.Task))
            {
                errors.Add(prefix + "немає тексту завдання (task).");
            }

            // Блоки коду ``` мають і відкриватися, і закриватися
            if (CountCodeFences(q.Theory) % 2 != 0)
            {
                errors.Add(prefix + "у теорії (theory) блок коду не закрито: рядків із трьома зворотними лапками має бути парна кількість.");
            }

            if (CountCodeFences(q.Explanation) % 2 != 0)
            {
                errors.Add(prefix + "у поясненні (explanation) блок коду не закрито: рядків із трьома зворотними лапками має бути парна кількість.");
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

        if (s.Integrity.ExitLimit < 0 || s.Integrity.ExitLimit > 20)
        {
            errors.Add("Налаштування доброчесності: exitLimit має бути від 0 до 20 (0 — виходи лише записуються).");
        }

        if (s.Integrity.ShortExitSeconds < 0 || s.Integrity.ShortExitSeconds > 10)
        {
            errors.Add("Налаштування доброчесності: shortExitSeconds має бути від 0 до 10 секунд.");
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
