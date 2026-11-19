using System.Security.Cryptography;
using System.Text.Json;

namespace Lab_Testing;

// Ключ підпису вчителя: випадкові 32 байти, яких немає в коді програми.
// Створюється на комп'ютері, де готують банк для учнів, і вкладається в зашифрований questions.dat.
// Програма учня підписує файл результату цим ключем, а програма вчителя перевіряє підпис тим самим ключем.
// Ключі зберігаються у файлі teacher_keys.json у теці даних програми (див. AppPaths)
public static class TeacherKey
{
    public const string KeyFileFormat = "Lab_Testing.Key/1";
    public const string KeyFileExtension = ".labkey";

    // Один ключ: короткий номер (видно у файлах результатів), сам ключ і дата створення
    public record KeyInfo(string Id, string Key, DateTime Created);

    // Файл копії ключа, який вчитель зберігає або передає
    private record KeyFile(string Format, string Id, string Key, DateTime Created);

    private static string FilePath => Path.Combine(AppPaths.DataFolder, "teacher_keys.json");

    // Усі ключі на цьому комп'ютері (останній — чинний)
    public static List<KeyInfo> All()
    {
        try
        {
            return File.Exists(FilePath)
                ? JsonSerializer.Deserialize<List<KeyInfo>>(File.ReadAllText(FilePath)) ?? new()
                : new();
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return new();
        }
    }

    // Чинний ключ (null — ще не створено)
    public static KeyInfo? Current => All().LastOrDefault();

    // Чинний ключ; якщо його ще немає — створює новий
    public static KeyInfo GetOrCreate()
    {
        KeyInfo? current = Current;

        if (current != null)
        {
            return current;
        }

        byte[] key = RandomNumberGenerator.GetBytes(32);
        KeyInfo created = new(MakeId(key), Convert.ToBase64String(key), DateTime.Now);
        Save(new List<KeyInfo> { created });
        return created;
    }

    // Ключ за номером (null — такого ключа на цьому комп'ютері немає)
    public static byte[]? Find(string id)
    {
        KeyInfo? info = All().FirstOrDefault(k => k.Id == id);
        return info == null ? null : Convert.FromBase64String(info.Key);
    }

    // Короткий номер ключа: перші 8 символів SHA-256 (за номером ключ не відновити)
    public static string MakeId(byte[] key)
    {
        return Convert.ToHexString(SHA256.HashData(key))[..8];
    }

    // Записує копію чинного ключа у файл (для перенесення на інший комп'ютер вчителя)
    public static void Export(string path)
    {
        KeyInfo current = Current ?? throw new InvalidOperationException("Ключ ще не створено.");
        KeyFile file = new(KeyFileFormat, current.Id, current.Key, current.Created);
        File.WriteAllText(path, JsonSerializer.Serialize(file, new JsonSerializerOptions { WriteIndented = true }));
    }

    // Додає ключ з файлу копії; повертає доданий (або вже наявний) ключ
    public static KeyInfo Import(string path)
    {
        KeyFile? file;

        try
        {
            file = JsonSerializer.Deserialize<KeyFile>(File.ReadAllText(path));
        }
        catch (JsonException)
        {
            file = null;
        }

        if (file == null || file.Format != KeyFileFormat || MakeId(Convert.FromBase64String(file.Key)) != file.Id)
        {
            throw new InvalidDataException("Це не файл ключа програми або файл пошкоджено.");
        }

        List<KeyInfo> keys = All();
        KeyInfo info = new(file.Id, file.Key, file.Created);

        // Відновлений ключ стає чинним (останнім у списку), старі ключі лишаються для перевірки старих файлів
        keys.RemoveAll(k => k.Id == info.Id);
        keys.Add(info);
        Save(keys);
        return info;
    }

    private static void Save(List<KeyInfo> keys)
    {
        Directory.CreateDirectory(AppPaths.DataFolder);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(keys));
    }
}
