using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Lab_Testing;

namespace ScreenshotMaker;

// Режим доброчесності: правила, контрольна на весь екран, справжні виходи з вікна тесту,
// друга копія програми, дострокове завершення, таблиця вчителя з порушеннями.
// Виходи — справжні: утиліта на кілька секунд відкриває Блокнот (він стає активним вікном),
// а потім закриває його. Якщо Windows не поверне фокус тесту сама, утиліта клацає мишею
// по смузі вгорі вікна тесту (перед кліком перевіряє, що під курсором саме вікно тесту)
internal class IntegrityScenario
{
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);
    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern void mouse_event(uint flags, int dx, int dy, uint data, UIntPtr extra);
    [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(Point point);
    [DllImport("user32.dll")] private static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_SYSCOMMAND = 0x0112;
    private static readonly IntPtr SC_CLOSE = new(0xF060);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
    private const uint MOUSEEVENTF_LEFTUP = 0x04;
    private const uint GA_ROOT = 2;

    // Скільки «думає» над питанням утиліта-учень: довше за поріг «дуже швидко» (8 с), щоб зразки
    // в таблиці вчителя були схожі на справжніх учнів (час на питання у файлах — справжній)
    public const int ThinkMs = 9000;

    // Проблема, помічена під час «виходу» (перевіряється після кожного виходу)
    private static string? awayProblem;

    private readonly Ui ui;
    private readonly string samples;   // тека зі зразками файлів результатів (з етапу 6)
    private readonly string appExe;     // Lab_Testing.exe поруч з утилітою (для перевірки другої копії)

    public IntegrityScenario(Ui ui)
    {
        this.ui = ui;
        samples = Path.Combine(Path.GetTempPath(), "Lab_Testing_зразки");
        appExe = Path.Combine(AppContext.BaseDirectory, "Lab_Testing.exe");
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Етап 7.2б: режим доброчесності");
        IntegrityCheck.Run();
        CheckOldFiles();
        await CheckCopiesAsync();

        QuestionBankData bank = QuestionBank.Load(out string source);
        ResultFile.SigningKey = Convert.FromBase64String(TeacherKey.GetOrCreate().Key);

        await RulesAsync(bank);
        await SecondCopyAsync(bank);
        await ExitsAsync();
        await TeacherAsync(bank, source);

        ResultFile.SigningKey = null;
    }

    // ===================== Правила перед стартом =====================

    private async Task RulesAsync(QuestionBankData bank)
    {
        IntegrityForm rules = new(bank.Settings);
        rules.Show();
        Button start = Ui.Find<Button>(rules, "btnStart");
        Expect(!start.Enabled, "без підтвердження кнопка «Почати контрольну» неактивна");

        await ui.ShotAsync(rules, 54);

        Ui.Find<CheckBox>(rules, "chkHonest").Checked = true;
        Expect(start.Enabled, "після підтвердження кнопка «Почати контрольну» активна");
        rules.Close();
        await Task.Delay(300);
    }

    // ===================== Друга копія програми під час контрольної =====================

    private async Task SecondCopyAsync(QuestionBankData bank)
    {
        if (!File.Exists(appExe))
        {
            Console.WriteLine("  Друга копія: Lab_Testing.exe поруч з утилітою немає — перевірку пропущено");
            return;
        }

        TestSession session = new(bank, "Мельник Софія", Scenarios.StudentGroup, TestMode.Control, 7);
        session.RulesAcceptedAt = DateTime.Now;
        TestForm form = new(session);
        form.Show();
        await Task.Delay(1000);
        Expect(InstanceGuard.ControlIsRunning, "під час контрольної інші копії бачать, що вона триває");

        for (int i = 0; i < 5; i++)
        {
            form.ShowQuestion(i);

            if (i % 2 == 0)
            {
                Answers.FillCorrect(form.CurrentView);
                await Task.Delay(ThinkMs);
            }
        }

        // Запускаємо програму ще раз: вона має не відкритися, а показати повідомлення
        using Process copy = Process.Start(new ProcessStartInfo(appExe) { UseShellExecute = false })!;
        IntPtr message = await WaitForWindowAsync(copy, 20000);
        await Task.Delay(800);

        using (Bitmap bmp = WindowCapture.Capture(message))
        {
            ui.SaveShot(bmp, 57);
        }

        WindowCapture.Close(message);

        if (!copy.WaitForExit(5000))
        {
            copy.Kill();
        }

        // Друга копія забрала фокус — це вихід з вікна тесту; повертаємось і закриваємо попередження
        await ReturnToAsync(form);
        await CloseWarningIfShownAsync();

        // Alt+F4 (Windows надсилає вікну SC_CLOSE): запитання видно; «Ні» — контрольна триває
        await ui.MessageShotAsync(form, () => PostMessage(form.Handle, WM_SYSCOMMAND, SC_CLOSE, IntPtr.Zero), null, Answer.No);
        Expect(form.Visible && form.Result == null, "Alt+F4 і «Ні» — контрольна триває");

        // «Завершити контрольну» і «Ні» — теж триває
        await ui.MessageShotAsync(form, () => Ui.Click(form, "btnFinish"), null, Answer.No);
        Expect(form.Visible && form.Result == null, "«Завершити контрольну» і «Ні» — контрольна триває");

        // «✕» угорі праворуч і «Так» — контрольну завершено, причина — «закрив вікно»
        await ui.MessageShotAsync(form, () => Ui.Click(form, "btnCloseTest"), null, Answer.Yes);
        TestResult result = form.Result ?? throw new InvalidOperationException("Контрольна не завершилася після «✕»");
        Expect(result.Reason == FinishReason.WindowClosed, "«✕» — причина завершення «закрив вікно тесту»");

        Expect(result.Integrity!.Events.Any(e => e.Kind == IntegrityEventKind.SecondCopy), "спробу відкрити другу копію записано");
        Expect(!InstanceGuard.ControlIsRunning, "після контрольної позначку «іде контрольна» знято");
        SaveSample(result);
        Console.WriteLine("  Друга копія: під час контрольної не відкривається, спробу записано; після контрольної — дозволено");
        Console.WriteLine("  Завершення: Alt+F4, «Завершити контрольну» і «✕» — запитання видно поверх тесту, «Ні» — тест триває, «Так» — завершено");
    }

    // ===================== Виходи з вікна і дострокове завершення =====================

    private async Task ExitsAsync()
    {
        TestSession session = Scenarios.CreateSession(TestMode.Control, Scenarios.MainQuestionIds);
        session.RulesAcceptedAt = DateTime.Now;
        TestForm form = new(session);
        form.Show();
        await Task.Delay(1000);

        form.ShowQuestion(0);
        Answers.FillCorrect(form.CurrentView);
        await Task.Delay(ThinkMs);
        form.ShowQuestion(2);
        Answers.FillCorrect(form.CurrentView);
        await Task.Delay(ThinkMs);
        form.ShowQuestion(1);
        Answers.FillWithMistakes(form.CurrentView);
        await Task.Delay(1500);
        await ui.ShotAsync(form, 55);
        Expect(ZOrder.IsTopMost(form.Handle), "поки учень у тесті, вікно тесту — поверх інших вікон");

        // Вихід 1: 3 секунди в іншій програмі -> попередження
        await ui.MessageShotAsync(form, () => _ = LeaveAndReturnAsync(form, 3000), 56, Answer.Ok, 30000);
        ExpectNoAwayProblem();

        // Короткий вихід (частка секунди): записується, але не рахується
        await LeaveAndReturnAsync(form, 100);
        await Task.Delay(1000);
        IntegrityReport report = form.IntegrityReport!;
        bool shortCounted = report.ShortExits == 0;

        if (shortCounted)
        {
            // Windows повертав фокус довше за поріг — вихід зараховано як звичайний (і показано попередження)
            Console.WriteLine("  Увага: короткий вихід тривав довше за поріг і зарахований як звичайний");
            await CloseWarningIfShownAsync();
        }

        form.ShowQuestion(3);
        Answers.FillCorrect(form.CurrentView);
        await Task.Delay(ThinkMs);

        // Вихід 2 (якщо короткий не зарахувався) -> попередження «наступний вихід завершить контрольну»
        if (!shortCounted)
        {
            await ui.MessageShotAsync(form, () => _ = LeaveAndReturnAsync(form, 2500), null, Answer.Ok, 30000);
            ExpectNoAwayProblem();
        }

        // Останній дозволений вихід -> дострокове завершення
        await ui.MessageShotAsync(form, () => _ = LeaveAndReturnAsync(form, 2500), 58, Answer.Ok, 30000);
        ExpectNoAwayProblem();
        await Task.Delay(500);

        TestResult result = form.Result ?? throw new InvalidOperationException("Контрольна не завершилася достроково");
        IntegrityReport r = result.Integrity!;

        Expect(result.Reason == FinishReason.ExitLimit && !result.FinishedByTimeout, "причина завершення — ліміт виходів");
        Expect(r.Exits == 3, $"зараховано 3 виходи (а не {r.Exits})");
        Expect(r.Events.Any(e => e.Kind == IntegrityEventKind.RulesAccepted), "підтвердження правил записано");
        Expect(r.Events.Any(e => e.Kind == IntegrityEventKind.EarlyFinish), "дострокове завершення записано");
        Expect(r.Events.Where(e => e.Kind == IntegrityEventKind.Exit).All(e => e.Seconds >= 2 && e.Question > 0),
            "у виходів є тривалість і номер питання");
        Expect(result.Questions.Sum(q => q.Seconds) > 0, "записано час на питання");

        // Хронологія — під підписом: змінити тривалість виходу непомітно не можна
        string json = ResultFile.ToSignedJson(result);
        Expect(ResultFile.Check(json, out _) == ResultFileStatus.Valid, "файл з хронологією — справжній");
        string changed = Regex.Replace(json, "\"seconds\": (\\d+)", "\"seconds\": 0");
        Expect(changed != json && ResultFile.Check(changed, out _) == ResultFileStatus.Modified, "змінена тривалість виходу — «змінено»");

        SaveSample(result);
        Console.WriteLine($"  Виходи: справжні виходи з вікна записано ({r.Exits} зараховано, коротких {r.ShortExits}), " +
                          "на ліміті контрольну завершено достроково; хронологія захищена підписом");
    }

    // ===================== Таблиця вчителя =====================

    private async Task TeacherAsync(QuestionBankData bank, string source)
    {
        TeacherForm teacher = new(bank, source);
        teacher.Show();
        teacher.LoadFiles(new[] { samples });
        await Task.Delay(500);
        await ui.ShotAsync(teacher, 59);

        DataGridView grid = Ui.Find<DataGridView>(teacher, "dgvResults");
        int row = grid.Rows.Cast<DataGridViewRow>()
            .First(r => r.Tag is ResultEntry { Result.Reason: FinishReason.ExitLimit }).Index;
        string violations = grid.Rows[row].Cells["colViolations"].Value?.ToString() ?? "";
        Expect(violations.StartsWith("вийшов 3 рази") && violations.Contains("завершено достроково"),
            "стовпець «Порушення»: " + violations);

        await ui.FormShotAsync<ResultForm>(teacher, () => Ui.DoubleClickCell(grid, row), 60);
        CheckExcel(teacher.Entries, bank.Settings);
        teacher.Close();
        await Task.Delay(300);
    }

    // Книга Excel: стовпці «Порушення» і «Завершення», аркуш «Події» з хронологією
    private void CheckExcel(IReadOnlyList<ResultEntry> entries, BankSettings settings)
    {
        string path = Path.Combine(samples, "Результати_доброчесність.xlsx");
        ExcelExport.Save(path, entries, settings);

        using ClosedXML.Excel.XLWorkbook book = new(path);
        ClosedXML.Excel.IXLWorksheet sheet = book.Worksheet("Результати");
        ClosedXML.Excel.IXLWorksheet events = book.Worksheet("Події");

        ClosedXML.Excel.IXLRow early = sheet.RowsUsed().First(r => r.Cells().Any(c => c.GetString() == "достроково: ліміт виходів"));
        Expect(early.Cells().Any(c => c.GetString().StartsWith("вийшов 3 рази")), "Excel: стовпець «Порушення»");

        int exitRows = events.RowsUsed().Count(r => r.Cell(5).GetString().StartsWith("вийшов з вікна тесту"));
        Expect(exitRows >= 3, "Excel: на аркуші «Події» є виходи з вікна");
        Expect(events.RowsUsed().Any(r => r.Cell(5).GetString().StartsWith("спроба відкрити ще одну копію")), "Excel: подія «друга копія»");

        Console.WriteLine("  Excel: стовпці «Порушення» і «Завершення», аркуш «Події» з хронологією — правильно");
    }

    // ===================== Допоміжне =====================

    // Учень «виходить» в іншу програму (Блокнот) на milliseconds і повертається в тест
    internal static async Task LeaveAndReturnAsync(Form form, int milliseconds)
    {
        using Process notepad = Process.Start("notepad.exe");
        await Task.Run(() => notepad.WaitForInputIdle(5000));
        IntPtr window = await WaitForWindowAsync(notepad, 10000);

        SetForegroundWindow(window);
        await Task.Delay(milliseconds);

        // Поки учень в іншій програмі, тест не має її перекривати (інакше Alt+Tab «не працює»)
        // і не має забирати фокус назад
        if (milliseconds >= 1000)
        {
            bool covered = ZOrder.IsTopMost(form.Handle) || !ZOrder.IsAbove(window, form.Handle);
            bool stolen = GetForegroundWindow() != window;

            if (covered || stolen)
            {
                awayProblem = covered
                    ? "інша програма опинилася ПІД вікном тесту — Alt+Tab і Win+R були б «невидимими»"
                    : "вікно тесту забрало фокус назад у іншої програми";
            }
        }

        notepad.CloseMainWindow();

        if (!await Task.Run(() => notepad.WaitForExit(3000)))
        {
            notepad.Kill();
        }

        await ReturnToAsync(form);
    }

    // Повертає фокус у вікно тесту: зазвичай Windows робить це сам після закриття іншого вікна,
    // інакше — клік мишею по смузі вгорі вікна тесту
    private static async Task ReturnToAsync(Form form)
    {
        for (int i = 0; i < 6 && !IsOurWindowActive(); i++)
        {
            await Task.Delay(100);
        }

        if (IsOurWindowActive())
        {
            return;
        }

        Point target = form.PointToScreen(new Point(form.ClientSize.Width / 2, 12));
        IntPtr under = GetAncestor(WindowFromPoint(target), GA_ROOT);

        if (under != form.Handle)
        {
            throw new InvalidOperationException("Під курсором не вікно тесту — клік скасовано (нічого не натиснуто)");
        }

        GetCursorPos(out Point saved);
        SetCursorPos(target.X, target.Y);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        await Task.Delay(200);
        SetCursorPos(saved.X, saved.Y);
    }

    private static bool IsOurWindowActive()
    {
        GetWindowThreadProcessId(GetForegroundWindow(), out uint pid);
        return pid == Environment.ProcessId;
    }

    private static async Task<IntPtr> WaitForWindowAsync(Process process, int timeoutMs)
    {
        Stopwatch sw = Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            IntPtr window = WindowCapture.FindAnyProcessWindow(process.Id);

            if (window != IntPtr.Zero)
            {
                return window;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException("Вікно програми не з'явилося: " + process.ProcessName);
    }

    // Якщо після виходу з'явилося попередження — натискаємо «OK»
    internal static async Task CloseWarningIfShownAsync()
    {
        for (int i = 0; i < 30; i++)
        {
            MessageForm? message = Application.OpenForms.OfType<MessageForm>().FirstOrDefault(f => f.Visible);

            if (message != null)
            {
                await Task.Delay(300);
                Ui.Find<Button>(message, "btnOk").PerformClick();
                await Task.Delay(300);
                return;
            }

            await Task.Delay(100);
        }
    }

    private void SaveSample(TestResult result)
    {
        File.WriteAllText(Path.Combine(samples, ResultFile.MakeFileName(result)), ResultFile.ToSignedJson(result));
    }

    // Файли попередньої версії (без режиму доброчесності) з робочого столу мають і далі відкриватися
    private static void CheckOldFiles()
    {
        // Справжній робочий стіл (утиліта перенаправляє ResultFile.DesktopFolder у тимчасову теку)
        string desktop = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), ResultFile.FolderName);

        if (!Directory.Exists(desktop))
        {
            return;
        }

        int count = 0;

        foreach (string file in Directory.EnumerateFiles(desktop, "*.json"))
        {
            string json = File.ReadAllText(file);

            if (json.Contains("Lab_Testing.Result/2"))
            {
                Expect(ResultFile.Check(json, out _) != ResultFileStatus.Damaged, "файл попередньої версії відкривається");
                count++;
            }
        }

        if (count > 0)
        {
            Console.WriteLine($"  Сумісність: {count} файл(ів) попередньої версії з робочого столу відкриваються, як і раніше");
        }
    }

    // Лічильник копій програми: запущена програма помітна, після закриття — ні
    private async Task CheckCopiesAsync()
    {
        if (!File.Exists(appExe))
        {
            return;
        }

        InstanceGuard.Register();
        int before = InstanceGuard.OtherCopies;

        using Process app = Process.Start(new ProcessStartInfo(appExe) { UseShellExecute = false })!;
        await WaitForWindowAsync(app, 20000);
        await Task.Delay(500);
        int during = InstanceGuard.OtherCopies;

        app.CloseMainWindow();

        if (!await Task.Run(() => app.WaitForExit(5000)))
        {
            app.Kill();
        }

        await Task.Delay(300);
        int after = InstanceGuard.OtherCopies;
        InstanceGuard.Unregister();

        Expect(during == before + 1 && after == before, $"лічильник копій програми: {before} → {during} → {after}");
        Console.WriteLine("  Копії програми: відкриту копію помічено, після закриття — ні");
    }

    private static void ExpectNoAwayProblem()
    {
        string? problem = awayProblem;
        awayProblem = null;
        Expect(problem == null, problem ?? "");
    }

    private static void Expect(bool condition, string what)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Режим доброчесності: не виконано — " + what);
        }
    }
}
