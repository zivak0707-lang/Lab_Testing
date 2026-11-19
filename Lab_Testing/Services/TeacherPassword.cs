using System.Security.Cryptography;
using System.Text.Json;

namespace Lab_Testing;

// Пароль режиму викладача.
// Сам пароль ніде не зберігається — лише «сіль» і хеш PBKDF2-SHA256 (з хешу пароль не відновити).
// Звідки береться чинний пароль (у порядку пріоритету):
//   1) змінений на цьому комп'ютері — файл teacher.json у теці даних програми (див. AppPaths);
//   2) пароль вчителя з банку questions.dat (вчитель вкладає його, коли готує банк для учнів);
//   3) типовий пароль (його повідомляє вчителеві автор програми).
// Пароль захищає лише вхід у режим викладача; справжність файлів результатів від пароля не залежить
public static class TeacherPassword
{
    private const int Iterations = 100_000;   // скільки разів «перемішувати» пароль (захист від підбору)
    private const int HashSize = 32;

    // Хеш типового пароля
    private const string DefaultSalt = "f5iW3UfdndRjJ/e09Nf+fg==";
    private const string DefaultHash = "ACmeHlXRiSaUGOM5ujxjl2P6CwQMS3xv3WUBR+oMcCU=";

    private static string FilePath => Path.Combine(AppPaths.DataFolder, "teacher.json");

    // Сіль і хеш пароля (так вони лежать у teacher.json і в questions.dat)
    public record PasswordHash(string Salt, string Hash);

    // Чи правильний пароль
    public static bool Verify(string password)
    {
        PasswordHash current = Current();
        byte[] expected = Convert.FromBase64String(current.Hash);
        return CryptographicOperations.FixedTimeEquals(Hash(password, Convert.FromBase64String(current.Salt)), expected);
    }

    // Чи діє зараз типовий пароль (тоді у вікні викладача з'являється порада змінити його)
    public static bool IsDefault => Current().Hash == DefaultHash;

    // Записує новий пароль (нова випадкова сіль + хеш) у teacher.json
    public static void Change(string newPassword)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        PasswordHash stored = new(Convert.ToBase64String(salt), Convert.ToBase64String(Hash(newPassword, salt)));

        Directory.CreateDirectory(AppPaths.DataFolder);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(stored));
    }

    // Сіль і хеш чинного пароля
    public static PasswordHash Current()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                PasswordHash? stored = JsonSerializer.Deserialize<PasswordHash>(File.ReadAllText(FilePath));

                if (stored != null)
                {
                    return stored;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            // файл пошкоджено — беремо пароль з банку або типовий
        }

        BankPackage? package = QuestionBank.TeacherPackage;

        return package != null
            ? new PasswordHash(package.PasswordSalt, package.PasswordHash)
            : new PasswordHash(DefaultSalt, DefaultHash);
    }

    private static byte[] Hash(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
    }
}
