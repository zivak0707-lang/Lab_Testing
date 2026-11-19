using System.Text;

namespace ScreenshotMaker;

internal static class Program
{
    // Аргумент (необов'язковий): до якого етапу включно робити знімки
    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Ті самі налаштування WinForms, що й у Lab_Testing (ApplicationConfiguration)
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        bool dragTest = args.Contains("--drag-test");

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
                if (dragTest)
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
        DirectoryInfo? dir = new(AppContext.BaseDirectory);

        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Lab_Testing.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Не знайдено Lab_Testing.slnx");
    }
}
