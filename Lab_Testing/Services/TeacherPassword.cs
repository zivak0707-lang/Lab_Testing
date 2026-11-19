using System.Security.Cryptography;
using System.Text.Json;

namespace Lab_Testing;

// Пароль режиму викладача.
// Сам пароль ніде не зберігається — лише «сіль» і хеш PBKDF2-SHA256 (з хешу пароль не відновити).
// Змінений пароль записується у файл %AppData%\Lab_Testing\teacher.json — це працює і для exe з одного файлу
public static class TeacherPassword
{
    private const int Iterations = 100_000;   // скільки разів «перемішувати» пароль (захист від підбору)
    private const int HashSize = 32;

    // Хеш типового пароля (сам типовий пароль повідомляє викладачеві автор програми)
    private const string DefaultSalt = "f5iW3UfdndRjJ/e09Nf+fg==";
    private const string DefaultHash = "ACmeHlXRiSaUGOM5ujxjl2P6CwQMS3xv3WUBR+oMcCU=";

    private static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lab_Testing", "teacher.json");

    // Дані файлу teacher.json
    private record StoredPassword(string Salt, string Hash);

    // Чи правильний пароль
    public static bool Verify(string password)
    {
        (byte[] salt, byte[] hash) = Current();
        return CryptographicOperations.FixedTimeEquals(Hash(password, salt), hash);
    }

    // Записує новий пароль (нова випадкова сіль + хеш) у teacher.json
    public static void Change(string newPassword)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        StoredPassword stored = new(Convert.ToBase64String(salt), Convert.ToBase64String(Hash(newPassword, salt)));

        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(stored));
    }

    // Сіль і хеш чинного пароля: змінений (з файлу) або типовий
    private static (byte[] Salt, byte[] Hash) Current()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                StoredPassword? stored = JsonSerializer.Deserialize<StoredPassword>(File.ReadAllText(FilePath));

                if (stored != null)
                {
                    return (Convert.FromBase64String(stored.Salt), Convert.FromBase64String(stored.Hash));
                }
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or FormatException)
        {
            // файл пошкоджено — користуємося типовим паролем
        }

        return (Convert.FromBase64String(DefaultSalt), Convert.FromBase64String(DefaultHash));
    }

    private static byte[] Hash(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
    }
}
