using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Lab_Testing;

// Стан файлу результату після перевірки підпису
public enum ResultFileStatus
{
    Valid,       // справжній: підписано ключем цього вчителя
    DefaultKey,  // підписано загальним ключем програми (учень проходив тест без банку вчителя)
    UnknownKey,  // підписано ключем, якого на цьому комп'ютері немає (інший банк або підробка)
    Modified,    // вміст змінено після збереження (підпис не збігається)
    Damaged      // файл не вдалося прочитати або це не файл результату
}

// Відкритий викладачем файл: шлях, чи справжній, дані (null — файл пошкоджений)
public record ResultEntry(string FilePath, ResultFileStatus Status, TestResult? Result);

// Файл результату контрольної: збереження з підписом і перевірка, чи файл не змінювали.
// Підпис — HMAC-SHA256 від усього вмісту файлу. Якщо змінити в Блокноті хоч один символ даних
// (наприклад, оцінку), підпис перестане збігатися.
// Підписують ключем вчителя з банку questions.dat; без банку — загальним ключем програми
public static class ResultFile
{
    public const string FolderName = "Результати тесту C#";
    public const string FormatName = "Lab_Testing.Result/3";
    private const string OldFormatName = "Lab_Testing.Result/1";   // файли першої версії (лише загальний ключ)
    private const string Version2FormatName = "Lab_Testing.Result/2";   // без режиму доброчесності
    public const string DefaultKeyId = "загальний";

    // Загальний ключ програми. Він є в публічному коді, тому файли з ним програма вчителя
    // позначає «загальний ключ» — їхню справжність гарантувати не можна
    private static readonly byte[] DefaultKey = Encoding.UTF8.GetBytes("Lab_Testing|підпис результату|Зівак-471|2026|q7Mz2vR9");

    // Ключ, яким ця програма підписує файли: ключ вчителя з questions.dat (null — загальний ключ)
    public static byte[]? SigningKey { get; set; }

    // Для підпису JSON записується в один рядок, щоб пробіли і переноси не впливали на результат
    private static readonly JsonSerializerOptions CanonicalOptions = new(QuestionBank.JsonOptions) { WriteIndented = false };

    // Тека на робочому столі. Шлях дає система, тому він правильний і коли робочий стіл у хмарній теці
    public static string DesktopFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), FolderName);

    // Запасна тека — якщо на робочий стіл записати не вдалося
    public static string DocumentsFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FolderName);

    // Резервні копії всіх спроб на цьому комп'ютері (за ними рахуємо повторні спроби)
    public static string BackupFolder => Path.Combine(AppPaths.DataFolder, "results");

    // ===================== Збереження =====================

    // Зберігає підписаний файл: резервна копія в AppData + файл на робочому столі (або в Документах).
    // Повертає шлях до файлу, який учень надсилає викладачеві
    public static string Save(TestResult result)
    {
        string json = ToSignedJson(result);
        string fileName = MakeFileName(result);

        TryWrite(BackupFolder, fileName, json);

        return TryWrite(DesktopFolder, fileName, json)
            ?? TryWrite(DocumentsFolder, fileName, json)
            ?? throw new IOException("Не вдалося записати файл ні на робочий стіл, ні в теку «Документи».");
    }

    // Текст файлу: JSON з усіма даними і підписом.
    // Пишемо копію результату, у якій помилки — звичайний текст без розмітки (файл відкривають у Блокноті),
    // а сам результат у програмі не змінюється (там код і далі виділяється)
    public static string ToSignedJson(TestResult result)
    {
        TestResult copy = JsonSerializer.Deserialize<TestResult>(
            JsonSerializer.Serialize(result, QuestionBank.JsonOptions), QuestionBank.JsonOptions)!;

        foreach (QuestionResult question in copy.Questions)
        {
            question.Mistakes = question.Mistakes.Select(PlainText).ToList();
        }

        byte[] key = SigningKey ?? DefaultKey;
        copy.Format = FormatName;
        copy.KeyId = SigningKey == null ? DefaultKeyId : TeacherKey.MakeId(SigningKey);
        copy.Signature = "";

        JsonObject document = JsonSerializer.SerializeToNode(copy, QuestionBank.JsonOptions)!.AsObject();
        document["signature"] = Sign(document, key);
        return document.ToJsonString(QuestionBank.JsonOptions);
    }

    // `код` -> «код», **жирне** -> жирне
    private static string PlainText(string markup)
    {
        return Regex.Replace(markup, "`([^`]*)`", "«$1»").Replace("**", "");
    }

    // Читає файл з диска і перевіряє підпис (для режиму викладача)
    public static ResultEntry Load(string path)
    {
        try
        {
            ResultFileStatus status = Check(File.ReadAllText(path), out TestResult? result);
            return new ResultEntry(path, status, status == ResultFileStatus.Damaged ? null : result);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new ResultEntry(path, ResultFileStatus.Damaged, null);
        }
    }

    // Результат_Прізвище_Ім'я_Група_2026-11-19_14-30.json
    public static string MakeFileName(TestResult result)
    {
        return $"Результат_{Clean(result.StudentName)}_{Clean(result.Group)}_{result.FinishedAt:yyyy-MM-dd_HH-mm}.json";
    }

    // Прибирає символи, недопустимі в назві файлу (\ / : * ? " < > |), пробіли й крапки замінює на «_»
    public static string Clean(string text)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        StringBuilder sb = new();

        foreach (char c in text.Trim())
        {
            sb.Append(invalid.Contains(c) || char.IsWhiteSpace(c) || c == '.' ? '_' : c);
        }

        string cleaned = Regex.Replace(sb.ToString(), "_+", "_").Trim('_');

        if (cleaned.Length > 40)
        {
            cleaned = cleaned[..40];
        }

        return cleaned.Length > 0 ? cleaned : "без_імені";
    }

    // Записує файл у теку; якщо файл з такою назвою вже є — додає _2, _3 ... (null — записати не вдалося)
    private static string? TryWrite(string folder, string fileName, string json)
    {
        try
        {
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, fileName);

            for (int n = 2; File.Exists(path); n++)
            {
                path = Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(fileName)}_{n}.json");
            }

            File.WriteAllText(path, json, new UTF8Encoding(false));
            return path;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    // ===================== Перевірка =====================

    // Читає файл результату і перевіряє підпис
    public static ResultFileStatus Check(string json, out TestResult? result)
    {
        result = null;
        JsonObject? document;

        try
        {
            document = JsonNode.Parse(json) as JsonObject;
            result = document?.Deserialize<TestResult>(QuestionBank.JsonOptions);
        }
        catch (JsonException)
        {
            return ResultFileStatus.Damaged;
        }

        if (document == null || result == null || (result.Format is not (FormatName or Version2FormatName or OldFormatName)))
        {
            result = null;
            return ResultFileStatus.Damaged;
        }

        // Яким ключем підписано: файли першої версії — завжди загальним
        string keyId = result.Format == OldFormatName ? DefaultKeyId : result.KeyId;
        byte[]? key = keyId == DefaultKeyId ? DefaultKey : TeacherKey.Find(keyId);

        if (key == null)
        {
            return ResultFileStatus.UnknownKey;
        }

        byte[] expected = Encoding.ASCII.GetBytes(Sign(document, key));
        byte[] actual = Encoding.ASCII.GetBytes(result.Signature);

        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
        {
            return ResultFileStatus.Modified;
        }

        return keyId == DefaultKeyId ? ResultFileStatus.DefaultKey : ResultFileStatus.Valid;
    }

    // HMAC-SHA256 від усього тексту файлу в один рядок, де поле signature порожнє.
    // Рахуємо від самого JSON (а не від класу C#), тож будь-яка зміна тексту — і навіть додане поле — помітна
    private static string Sign(JsonObject document, byte[] key)
    {
        JsonObject copy = document.DeepClone().AsObject();
        copy["signature"] = "";
        byte[] content = Encoding.UTF8.GetBytes(copy.ToJsonString(CanonicalOptions));

        return Convert.ToBase64String(HMACSHA256.HashData(key, content));
    }

    // ===================== Повторні спроби і Провідник =====================

    // Скільки контрольних цей учень уже проходив на цьому комп'ютері (за резервними копіями)
    public static int CountPreviousAttempts(string studentName, string group)
    {
        if (!Directory.Exists(BackupFolder))
        {
            return 0;
        }

        int count = 0;

        foreach (string file in Directory.GetFiles(BackupFolder, "*.json"))
        {
            try
            {
                if (Check(File.ReadAllText(file), out TestResult? r) != ResultFileStatus.Damaged && r != null &&
                    string.Equals(r.StudentName, studentName, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(r.Group, group, StringComparison.CurrentCultureIgnoreCase))
                {
                    count++;
                }
            }
            catch (IOException)
            {
                // файл зайнятий іншою програмою — пропускаємо
            }
        }

        return count;
    }

    // Відкриває Провідник, у якому виділено файл
    public static void ShowInExplorer(string path)
    {
        Process.Start("explorer.exe", $"/select,\"{path}\"");
    }
}
