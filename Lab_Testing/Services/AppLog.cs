using System.Text;

namespace Lab_Testing;

// Журнал діагностики: %AppData%\Lab_Testing\log.txt.
// Записуються лише події режиму доброчесності, початок і кінець контрольної та помилки програми —
// нічого понад те, що вже є у файлі результату. Шлях до профілю користувача Windows замінюється на %USERPROFILE%.
// Журнал допомагає розібратися у спірній ситуації, але доказом не є: він лежить на комп'ютері учня
// і його можна видалити чи змінити (доказ — підписаний файл результату)
public static class AppLog
{
    private const long MaxSize = 1_000_000;          // понад 1 МБ — старий журнал перейменовуємо в log.old.txt
    private static readonly object Sync = new();     // записують кілька потоків (події Windows)

    public static string FilePath => Path.Combine(AppPaths.DataFolder, "log.txt");

    // «2026-11-20 10:03:15 | доброчесність | вийшов з вікна тесту на 12 с (питання 2)»
    public static void Info(string section, string text)
    {
        Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {section} | {text}");
    }

    // Помилка програми: тип, повідомлення і стек викликів (де саме в коді)
    public static void Error(string where, Exception ex)
    {
        Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | помилка | {where}: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
    }

    // Запис ніколи не зупиняє програму: якщо журнал записати не вдалося — просто пропускаємо
    private static void Write(string line)
    {
        try
        {
            lock (Sync)
            {
                Directory.CreateDirectory(AppPaths.DataFolder);
                FileInfo file = new(FilePath);

                if (file.Exists && file.Length > MaxSize)
                {
                    File.Move(FilePath, Path.Combine(AppPaths.DataFolder, "log.old.txt"), overwrite: true);
                }

                File.AppendAllText(FilePath, HideUserFolder(line) + Environment.NewLine, new UTF8Encoding(true));
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // журнал недоступний (наприклад, тека лише для читання) — програма працює далі
        }
    }

    // Шлях до профілю користувача -> %USERPROFILE%\... (ім'я користувача Windows у журнал не потрапляє)
    private static string HideUserFolder(string text)
    {
        string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return profile.Length > 0 ? text.Replace(profile, "%USERPROFILE%", StringComparison.OrdinalIgnoreCase) : text;
    }
}
