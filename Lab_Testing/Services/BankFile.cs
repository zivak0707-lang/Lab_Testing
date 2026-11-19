using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Lab_Testing;

// Вміст questions.dat — «банку для учнів», який готує вчитель
public record BankPackage(
    string BankJson,          // текст банку питань (JSON)
    string KeyId,             // номер ключа підпису вчителя
    string SigningKey,        // ключ підпису вчителя (Base64)
    string PasswordSalt,      // сіль і хеш пароля режиму викладача
    string PasswordHash,
    DateTime PreparedAt,
    string AppVersion);

// Зашифрований файл банку для учнів questions.dat.
// Шифрування AES-GCM не дає прочитати відповіді в Блокноті і помічає пошкоджений файл.
// Обмеження: ключ шифрування вшитий у програму (код публічний), тож учень, який уміє програмувати,
// може розшифрувати файл — це описано в README в розділі «Відомі обмеження»
public static class BankFile
{
    public const string FileName = "questions.dat";
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("LABTEST1");
    private static readonly byte[] CipherKey = SHA256.HashData(Encoding.UTF8.GetBytes("Lab_Testing|банк для учнів|AES-GCM|2026"));

    private const int NonceSize = 12;
    private const int TagSize = 16;

    // Шифрує і записує банк
    public static void Write(string path, BankPackage package)
    {
        byte[] plain = JsonSerializer.SerializeToUtf8Bytes(package);
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] cipher = new byte[plain.Length];
        byte[] tag = new byte[TagSize];

        using (AesGcm aes = new(CipherKey, TagSize))
        {
            aes.Encrypt(nonce, plain, cipher, tag);
        }

        using FileStream file = File.Create(path);
        file.Write(Magic);
        file.Write(nonce);
        file.Write(tag);
        file.Write(cipher);
    }

    // Читає і розшифровує банк; пошкоджений або чужий файл — BankException
    public static BankPackage Read(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        int header = Magic.Length + NonceSize + TagSize;

        if (data.Length <= header || !data.AsSpan(0, Magic.Length).SequenceEqual(Magic))
        {
            throw new BankException($"{FileName}: це не файл банку програми.");
        }

        ReadOnlySpan<byte> nonce = data.AsSpan(Magic.Length, NonceSize);
        ReadOnlySpan<byte> tag = data.AsSpan(Magic.Length + NonceSize, TagSize);
        ReadOnlySpan<byte> cipher = data.AsSpan(header);
        byte[] plain = new byte[cipher.Length];

        try
        {
            using AesGcm aes = new(CipherKey, TagSize);
            aes.Decrypt(nonce, cipher, tag, plain);
            return JsonSerializer.Deserialize<BankPackage>(plain) ?? throw new BankException($"{FileName}: файл порожній.");
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException)
        {
            throw new BankException($"{FileName}: файл пошкоджено. Попросіть у вчителя новий.");
        }
    }
}
