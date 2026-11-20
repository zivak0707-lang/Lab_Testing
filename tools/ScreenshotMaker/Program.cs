using System.Text;

namespace ScreenshotMaker;

internal static class Program
{
    // Аргумент (необов'язковий): до якого етапу включно робити знімки
    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Дані програми (пароль, ключ вчителя, резервні копії) — у тимчасовій теці, щоб утиліта
        // нічого не створювала й не змінювала в %AppData% комп'ютера, де її запускають
        string dataFolder = Path.Combine(Path.GetTempPath(), "Lab_Testing_дані_утиліти");

        if (Directory.Exists(dataFolder))
        {
            Directory.Delete(dataFolder, true);
        }

        Environment.SetEnvironmentVariable("LAB_TESTING_DATA", dataFolder);

        // Ті самі налаштування WinForms, що й у Lab_Testing (ApplicationConfiguration)
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        bool dragTest = args.Contains("--drag-test");
        bool diagTopmost = args.Contains("--diag-topmost");

        // --all-questions <тека>: знімки всіх питань банку для перевірки розмітки
        int allIndex = Array.IndexOf(args, "--all-questions");
        string? allQuestionsDir = allIndex >= 0 && allIndex + 1 < args.Length ? args[allIndex + 1] : null;

        string? stageArg = args.Where((a, i) => !a.StartsWith("--") && (allIndex < 0 || i != allIndex + 1)).FirstOrDefault();
        int maxStage = stageArg != null ? int.Parse(stageArg) : ShotList.CurrentStage;
        string outDir = Path.Combine(FindRepoRoot(), "docs", "screenshots");
        Directory.CreateDirectory(outDir);
        Console.WriteLine("Знімки -> " + outDir);

        Scenarios scenarios = new(new Ui(outDir));
        Exception? error = null;

        // Сценарій виконується всередині циклу повідомлень WinForms, щоб форми працювали як у програмі
        ApplicationContext context = new();
        System.Windows.Forms.Timer starter = new() { Interval = 100 };
        starter.Tick += async (_, _) =>
        {
            starter.Stop();

            try
            {
                if (diagTopmost)
                {
                    await TopmostDiagnostics.RunAsync();
                }
                else if (dragTest)
                {
                    await scenarios.RunDragTestAsync();
                }
                else if (allQuestionsDir != null)
                {
                    await Scenarios.RunAllQuestionsAsync(allQuestionsDir);
                }
                else
                {
                    await scenarios.RunAsync(maxStage);
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                context.ExitThread();
            }
        };
        starter.Start();
        Application.Run(context);

        ShotList.WriteReadme(outDir, maxStage);
        BankListWriter.Write(Path.GetDirectoryName(outDir)!);

        try
        {
            BankListWriter.WriteTheory(Path.GetDirectoryName(outDir)!);
        }
        catch (InvalidOperationException ex)
        {
            error ??= ex;
        }

        if (error != null)
        {
            Console.Error.WriteLine("ПОМИЛКА: " + error);
            return 1;
        }

        Console.WriteLine("Готово.");
        return 0;
    }

    // Корінь репозиторію — тека, де лежить Lab_Testing.slnx
    private static string FindRepoRoot()
    {
        // Шукаємо від теки утиліти, а якщо її зібрано в іншу теку — від поточної теки
        DirectoryInfo? dir = Find(new DirectoryInfo(AppContext.BaseDirectory)) ?? Find(new DirectoryInfo(Environment.CurrentDirectory));
        return dir?.FullName ?? throw new DirectoryNotFoundException("Не знайдено Lab_Testing.slnx");
    }

    private static DirectoryInfo? Find(DirectoryInfo? dir)
    {
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Lab_Testing.slnx")))
        {
            dir = dir.Parent;
        }

        return dir;
    }
}
