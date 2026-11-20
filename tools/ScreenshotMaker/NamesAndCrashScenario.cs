using ClosedXML.Excel;
using Lab_Testing;

namespace ScreenshotMaker;

// ПІБ і повторні спроби, журнал діагностики, збій програми під час контрольної
internal class NamesAndCrashScenario
{
    // Повторна спроба того самого учня, але з латинською «e» у прізвищі (ніби з попередньої версії програми,
    // яка ще не перевіряла ПІБ). Учень пройшов на іншому комп'ютері — у файлі «спроба №1»
    private const string LatinName = "Пeтренко Андрій";

    private readonly Ui ui;
    private readonly string samples = Path.Combine(Path.GetTempPath(), "Lab_Testing_зразки");

    public NamesAndCrashScenario(Ui ui)
    {
        this.ui = ui;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Етап 7.2в: ПІБ і повторні спроби, журнал, збій програми");
        CheckNameRules();
        CheckCounter();
        CheckGroups();

        QuestionBankData bank = QuestionBank.Load(out string source);
        ResultFile.SigningKey = Convert.FromBase64String(TeacherKey.GetOrCreate().Key);

        await StartMessageAsync(bank, source);
        await RepeatAttemptAsync(bank);
        await TeacherAsync(bank, source);
        await CrashAsync();

        ResultFile.SigningKey = null;
    }

    // ===================== Правила для ПІБ =====================

    private static void CheckNameRules()
    {
        string key = NameRules.Key("Зівак Сергій");
        Expect(NameRules.Key("Зiвак Сергій") == key, "латинська «i» у ключі ПІБ = кирилична «і»");
        Expect(NameRules.Key("  зівак   СЕРГІЙ ") == key, "регістр і зайві пробіли не важливі");
        Expect(NameRules.Key("Сергій Зівак") == key, "порядок слів не важливий");
        Expect(NameRules.Key("Зівок Сергій") != key, "інше прізвище — інший ключ");

        // Латиниця записана кодами, щоб у тексті програми її не сплутати з кирилицею: C (U+0043), y (U+0079), k (U+006B)
        Expect(NameRules.Key("Cергій Зівак") == key, "латинська «C» = кирилична «С»");
        Expect(NameRules.Key("Крyк Ігор") == NameRules.Key("Крук Ігор"), "латинська «y» = кирилична «у»");
        Expect(NameRules.Key("Лаkуста Олег") == NameRules.Key("Лакуста Олег"), "латинська «k» = кирилична «к»");
        Expect(NameRules.Key("Мар'яна Коваль") == NameRules.Key("Марʼяна Коваль") &&
               NameRules.Key("Мар’яна Коваль") == NameRules.Key("Мар'яна Коваль"), "різні апострофи — однаково");
        Expect(NameRules.GroupKey("ІПЗ-21") == NameRules.GroupKey("IПЗ - 21") &&
               NameRules.GroupKey("іпз-21") == NameRules.GroupKey("ІПЗ-21"), "ключ групи");

        string? latin = NameRules.InputProblem("Зiвак Сергій");
        Expect(latin != null && latin.Contains("«Зiвак»") && latin.Contains("«i»") && latin.Contains("кирилицею"),
            "повідомлення про латинську літеру: " + latin);
        Expect(NameRules.InputProblem("Зівак Сергій") == null, "кириличне ПІБ проходить");
        Expect(NameRules.InputProblem("Мар’яна Коваль-Петренко") == null, "апостроф і дефіс дозволено");
        Expect(NameRules.InputProblem("Іваненко О. П.") == null, "ініціали з крапками дозволено");
        Expect(NameRules.InputProblem("Іваненко 2")?.Contains("«2»") == true, "цифра в ПІБ — повідомлення");

        Expect(NameRules.GroupProblem("IПЗ-21") != null, "група зі змішаними абетками — повідомлення");
        Expect(NameRules.GroupProblem("471") == null && NameRules.GroupProblem("ІПЗ-21") == null, "звичайна група проходить");

        Expect(NameRules.Similar(key, NameRules.Key("Зівак Сергій Петрович")), "схожий ПІБ: без по батькові");
        Expect(NameRules.Similar(key, NameRules.Key("Зивак Сергій")), "схожий ПІБ: одна літера");
        Expect(!NameRules.Similar(NameRules.Key("Коваль Марія"), NameRules.Key("Шевчук Олег")), "різні ПІБ — не схожі");

        Console.WriteLine("  ПІБ: латинські двійники, апострофи, регістр, пробіли, порядок слів; заборона латиниці на вході — правильно");
    }

    // Лічильник на комп'ютері учня: «Зiвак» з латинською «i» — та сама людина
    private static void CheckCounter()
    {
        Directory.CreateDirectory(ResultFile.BackupFolder);
        List<string> files = new();

        foreach (string name in new[] { "Зівак Сергій", "Зiвак Сергій" })
        {
            TestResult r = new()
            {
                StudentName = name, Group = "471", Mode = TestMode.Control, AttemptId = Guid.NewGuid(),
                StartedAt = new DateTime(2026, 11, 20, 9, 0, 0), FinishedAt = new DateTime(2026, 11, 20, 9, 10, 0)
            };

            string file = Path.Combine(ResultFile.BackupFolder, $"лічильник_{files.Count}.json");
            File.WriteAllText(file, ResultFile.ToSignedJson(r));
            files.Add(file);
        }

        int counted = ResultFile.CountPreviousAttempts("Зівак Сергій", "471");
        int other = ResultFile.CountPreviousAttempts("Іванов Іван", "471");
        files.ForEach(File.Delete);

        Expect(counted == 2 && other == 0, $"лічильник спроб на ПК учня: {counted} і {other}");
        Console.WriteLine("  Лічильник на ПК учня: «Зiвак» з латинською «i» рахується як той самий учень");
    }

    // Повторні спроби у вчителя: копія, повтор з латинською літерою, інша група, схожий ПІБ
    private static void CheckGroups()
    {
        DateTime t = new(2026, 11, 20, 9, 0, 0);
        Guid first = Guid.NewGuid();

        ResultEntry Entry(string file, string name, string group, Guid id, int hours)
        {
            TestResult r = new()
            {
                StudentName = name, Group = group, Mode = TestMode.Control, AttemptId = id, AttemptNumber = 1,
                StartedAt = t.AddHours(hours), FinishedAt = t.AddHours(hours).AddMinutes(10)
            };
            return new ResultEntry(file, ResultFileStatus.Valid, r);
        }

        ResultEntry original = Entry("a.json", "Петренко Андрій", "471", first, 0);
        ResultEntry repeat = Entry("b.json", LatinName, "471", Guid.NewGuid(), 1);
        ResultEntry copy = Entry("копія\\a.json", "Петренко Андрій", "471", first, 0);
        ResultEntry otherGroup = Entry("c.json", "Петренко Андрій", "472", Guid.NewGuid(), 2);
        ResultEntry similar = Entry("d.json", "Петренко Андрій Іванович", "471", Guid.NewGuid(), 3);
        ResultEntry borrowed = Entry("e.json", "Шевчук Олег", "471", first, 4);   // чужий файл, перейменований

        Dictionary<ResultEntry, AttemptInfo> info = AttemptGroups.Analyze(new[] { original, repeat, copy, otherGroup, similar, borrowed });
        string repeatNote = AttemptGroups.Note(info[repeat]);

        Expect(info[original].Number == 1 && info[repeat].Number == 2 && info[repeat].Total == 2, "номер спроби у вчителя");
        Expect(repeatNote.Contains("повторна спроба: 2-а з 2", StringComparison.OrdinalIgnoreCase) && repeatNote.Contains("у файлі — спроба №1") &&
               !repeatNote.Contains("по-різному") && repeatNote.Contains("«e» у «Пeтренко»") &&
               repeatNote.Contains("у групі «472»") && repeatNote.Contains("схожий ПІБ"), "примітки повторної спроби: " + repeatNote);
        Expect(AttemptGroups.Note(info[copy]).Contains("копія", StringComparison.OrdinalIgnoreCase), "копія того самого файлу не рахується спробою");
        Expect(info[borrowed].Number == 1 && AttemptGroups.Note(info[borrowed]).Contains("з чужого результату"),
            "файл з номером спроби іншого учня — примітка «з чужого результату»");

        // Різні апострофи — та сама людина, але написання різне (це видно в примітці)
        ResultEntry apostrophe1 = Entry("f.json", "Коваль Мар'яна", "471", Guid.NewGuid(), 5);
        ResultEntry apostrophe2 = Entry("g.json", "Коваль Мар’яна", "471", Guid.NewGuid(), 6);
        Dictionary<ResultEntry, AttemptInfo> info2 = AttemptGroups.Analyze(new[] { apostrophe1, apostrophe2 });
        Expect(info2[apostrophe2].Number == 2 && AttemptGroups.Note(info2[apostrophe2]).Contains("по-різному"),
            "різні апострофи — повторна спроба з приміткою «по-різному»");

        Console.WriteLine("  Повторні спроби у вчителя: повтор, копія файлу, інша група, схожий ПІБ, лічильник з ПК учня — розпізнано");
    }

    // ===================== Повідомлення на старті =====================

    private async Task StartMessageAsync(QuestionBankData bank, string source)
    {
        using StartForm form = new(bank, source);
        form.Show();
        await Task.Delay(300);

        Ui.SetText(form, "txtName", "Іванeнко Олена Петрівна");   // «e» — латинська
        Ui.SetText(form, "txtGroup", Scenarios.StudentGroup);
        Ui.Check(form, "rbControl");
        await ui.MessageShotAsync(form, () => Ui.Click(form, "btnStart"), 65, Answer.Ok);
        form.Close();
    }

    // ===================== Повторна спроба в таблиці вчителя =====================

    private async Task RepeatAttemptAsync(QuestionBankData bank)
    {
        TestSession session = new(bank, LatinName, Scenarios.StudentGroup, TestMode.Control, 104);
        session.RulesAcceptedAt = DateTime.Now;
        TestForm form = new(session);
        form.Show();
        await Task.Delay(1000);

        for (int i = 0; i < 5; i++)
        {
            form.ShowQuestion(i);
            Answers.FillCorrect(form.CurrentView);
            await Task.Delay(IntegrityScenario.ThinkMs);
        }

        await ui.MessageShotAsync(form, () => Ui.Click(form, "btnFinish"), null, Answer.Yes);
        TestResult result = form.Result ?? throw new InvalidOperationException("Контрольна не завершилася");
        File.WriteAllText(Path.Combine(samples, ResultFile.MakeFileName(result)), ResultFile.ToSignedJson(result));
    }

    private async Task TeacherAsync(QuestionBankData bank, string source)
    {
        TeacherForm teacher = new(bank, source);
        teacher.Show();
        teacher.LoadFiles(new[] { samples });
        await Task.Delay(500);

        DataGridView grid = Ui.Find<DataGridView>(teacher, "dgvResults");
        DataGridViewRow repeat = grid.Rows.Cast<DataGridViewRow>().First(r => ((ResultEntry)r.Tag!).Result?.StudentName == LatinName);
        string note = repeat.Cells["colNote"].Value?.ToString() ?? "";

        Expect(Equals(repeat.Cells["colAttempt"].Value, 2), "у таблиці вчителя повторна спроба — №2");
        Expect(note.Contains("повторна спроба: 2-а з 2", StringComparison.OrdinalIgnoreCase) && note.Contains("латинські літери"), "примітка в таблиці: " + note);

        // Повторна спроба — нагорі таблиці, щоб її було видно на знімку
        grid.Sort(grid.Columns["colAttempt"]!, System.ComponentModel.ListSortDirection.Descending);
        grid.CurrentCell = null;
        grid.ClearSelection();
        await ui.ShotAsync(teacher, 64);

        // Те саме в Excel
        string xlsx = Path.Combine(samples, "Результати_повторні_спроби.xlsx");
        ExcelExport.Save(xlsx, teacher.Entries, bank.Settings);

        using (XLWorkbook book = new(xlsx))
        {
            Expect(book.Worksheet("Результати").RowsUsed().Any(r => r.Cells().Any(c => c.GetString().Contains("повторна спроба: 2-а з 2", StringComparison.OrdinalIgnoreCase))),
                "Excel: примітка про повторну спробу");
        }

        teacher.Close();
        await Task.Delay(300);
        Console.WriteLine("  Таблиця вчителя і Excel: повторна спроба (№2) і латинська літера в ПІБ — у «Спробі» і «Примітці»");
    }

    // ===================== Збій програми під час контрольної =====================

    private async Task CrashAsync()
    {
        // Ctrl+Shift+F12 спрацьовує і тоді, коли фокус на самому вікні (не на кнопці)
        await HotKeyWithoutFocusAsync();

        // Журнал утиліти (у її тимчасовій теці) — лише для цієї контрольної, щоб на рисунку 67 була вона одна
        File.Delete(AppLog.FilePath);

        TestSession session = Scenarios.CreateSession(TestMode.Control, Scenarios.MainQuestionIds);
        session.RulesAcceptedAt = DateTime.Now;
        TestForm form = new(session);
        form.Show();
        await Task.Delay(1000);

        form.ShowQuestion(0);
        Answers.FillCorrect(form.CurrentView);
        Expect(TestForm.RunningControl == form && ZOrder.IsTopMost(form.Handle), "контрольна йде на весь екран поверх усіх");

        // Один справжній вихід з вікна (Блокнот на 3 с) — щоб у журналі була подія доброчесності
        await IntegrityScenario.LeaveAndReturnAsync(form, 3000);
        await IntegrityScenario.CloseWarningIfShownAsync();

        // Справжнє натискання Ctrl+Shift+F12 (збірка Debug): спершу відпускаємо Ctrl і Shift — найважчий випадок
        Expect(await HotKeys.PressCtrlShiftF12Async(form, modifiersFirst: true), "клавіші надіслано у вікно контрольної");
        MessageForm message = await Ui.WaitForFormAsync<MessageForm>(66, 15000);
        await Task.Delay(600);

        Expect(!ZOrder.IsTopMost(form.Handle) && form.FormBorderStyle == FormBorderStyle.Sizable,
            "після збою вікно тесту — звичайне, не на весь екран і не поверх усіх");
        ZOrder.ExpectDialogVisible(message.Handle, form.Handle, "Повідомлення про збій");

        using (Bitmap bmp = WindowCapture.Capture(message.Handle))
        {
            ui.SaveShot(bmp, 66);
        }

        Ui.Find<Button>(message, "btnOk").PerformClick();
        await Task.Delay(800);

        TestResult result = form.Result ?? throw new InvalidOperationException("Після збою немає результату");
        Expect(result.Reason == FinishReason.ProgramError, "причина завершення — помилка програми");
        Expect(result.Integrity!.Events.Any(e => e.Kind == IntegrityEventKind.ProgramError), "подію збою записано");
        Expect(form.SavedPath != null && File.Exists(form.SavedPath) &&
               ResultFile.Check(File.ReadAllText(form.SavedPath), out _) == ResultFileStatus.Valid, "файл після збою збережено і він справжній");
        Expect(TestForm.RunningControl == null, "після збою контрольна вже не «йде»");

        CheckLog();
        await LogShotAsync();
        await CaptureProtectionShotAsync();
        Console.WriteLine("  Збій під час контрольної: відповіді збережено (файл справжній), режим на весь екран знято, повідомлення видно");
    }

    // Журнал: є події доброчесності, початок і кінець контрольної, помилка зі стеком — і немає шляху профілю Windows
    private static void CheckLog()
    {
        string log = File.ReadAllText(AppLog.FilePath);
        string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        Expect(log.Contains("| доброчесність |") && log.Contains("підтвердив правила") && log.Contains("вийшов з вікна тесту"),
            "журнал: події доброчесності");
        Expect(log.Contains("| контрольна | почалась") && log.Contains("через помилку програми"), "журнал: початок і кінець контрольної");
        Expect(log.Contains("| помилка |") && log.Contains("InvalidOperationException") && log.Contains("Ctrl+Shift+F12"),
            "журнал: помилка програми (від Ctrl+Shift+F12)");
        Expect(!log.Contains(profile, StringComparison.OrdinalIgnoreCase), "журнал: без шляху до профілю користувача");

        Console.WriteLine("  Журнал log.txt: події доброчесності, початок і кінець контрольної, помилка; шлях профілю приховано");
    }

    // Ctrl+Shift+F12, коли фокус на самому вікні тесту (а не на кнопці), і F12 відпущено першою
    private static async Task HotKeyWithoutFocusAsync()
    {
        TestForm form = new(Scenarios.CreateSession(TestMode.Control, Scenarios.MainQuestionIds));
        form.Show();
        await Task.Delay(1000);
        form.ActiveControl = null;

        Expect(await HotKeys.PressCtrlShiftF12Async(form, modifiersFirst: false), "клавіші надіслано у вікно контрольної");
        MessageForm message = await Ui.WaitForFormAsync<MessageForm>(0, 10000);
        Ui.Find<Button>(message, "btnOk").PerformClick();
        await Task.Delay(500);

        Expect(form.Result?.Reason == FinishReason.ProgramError, "Ctrl+Shift+F12 без фокуса на елементі — аварійне завершення");
        Console.WriteLine("  Ctrl+Shift+F12 (Debug): спрацьовує за будь-якого фокуса і порядку відпускання клавіш");
    }

    // ===================== Рисунок 67: журнал у Блокноті =====================

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int width, int height, bool repaint);

    // log.txt у Блокноті з перенесенням рядків. Блокнот запам'ятовує перенесення і розмір вікна в реєстрі —
    // після знімка повертаємо ваші налаштування як були
    private async Task LogShotAsync()
    {
        using Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Notepad");
        string[] names = { "fWrap", "iWindowPosX", "iWindowPosY", "iWindowPosDX", "iWindowPosDY" };
        Dictionary<string, object?> saved = names.ToDictionary(n => n, n => key.GetValue(n));
        key.SetValue("fWrap", 1, Microsoft.Win32.RegistryValueKind.DWord);

        try
        {
            using System.Diagnostics.Process notepad = System.Diagnostics.Process.Start("notepad.exe", $"\"{AppLog.FilePath}\"");
            await Task.Run(() => notepad.WaitForInputIdle(5000));
            IntPtr window = IntPtr.Zero;

            for (int i = 0; i < 50 && window == IntPtr.Zero; i++)
            {
                await Task.Delay(100);
                window = WindowCapture.FindProcessWindow(notepad.Id, "Notepad");
            }

            Expect(window != IntPtr.Zero, "Блокнот відкрився");
            Rectangle area = Screen.PrimaryScreen!.WorkingArea;
            MoveWindow(window, area.X + 40, area.Y + 40, Math.Min(1200, area.Width - 80), Math.Min(760, area.Height - 80), true);
            await Task.Delay(800);

            using (Bitmap bmp = WindowCapture.Capture(window))
            {
                ui.SaveShot(bmp, 67);
            }

            notepad.CloseMainWindow();

            if (!await Task.Run(() => notepad.WaitForExit(3000)))
            {
                notepad.Kill();
            }
        }
        finally
        {
            foreach ((string name, object? value) in saved)
            {
                if (value == null)
                {
                    key.DeleteValue(name, throwOnMissingValue: false);
                }
                else
                {
                    key.SetValue(name, value, Microsoft.Win32.RegistryValueKind.DWord);
                }
            }
        }
    }

    // ===================== Рисунок 61: захист від знімків екрана =====================

    // Знімок УСЬОГО екрана звичайним способом (як Print Screen) під час контрольної: без захисту і з захистом.
    // Під вікном тесту утиліта кладе порожнє вікно-фон — щоб на знімку був не ваш робочий стіл
    private async Task CaptureProtectionShotAsync()
    {
        Rectangle screen = Screen.FromPoint(Cursor.Position).Bounds;
        Color backColor = Color.FromArgb(58, 96, 135);

        using Form backdrop = new()
        {
            FormBorderStyle = FormBorderStyle.None, StartPosition = FormStartPosition.Manual, Bounds = screen,
            BackColor = backColor, ShowInTaskbar = false
        };
        backdrop.Controls.Add(new Label
        {
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White,
            Font = new Font("Segoe UI", 20F), Text = "Тут — інші вікна Windows під вікном тесту\n(для знімка — порожнє вікно утиліти)"
        });
        backdrop.Show();
        await Task.Delay(500);

        using Bitmap visible = await ScreenDuringControlAsync(hide: false, screen);
        using Bitmap hidden = await ScreenDuringControlAsync(hide: true, screen);
        backdrop.Close();

        // Перевірка: посередині смуги режиму доброчесності без захисту — смуга, із захистом — фон під тестом
        Point probe = new(screen.Width / 2, 30);
        Expect(Near(visible.GetPixel(probe.X, probe.Y), Theme(33, 43, 54)), "без захисту вікно тесту є на знімку");
        Expect(Near(hidden.GetPixel(probe.X, probe.Y), backColor), "із захистом вікна тесту на знімку немає");

        using Bitmap combined = SideBySide(visible, "hideFromCapture: false — вікно тесту на знімку видно",
            hidden, "hideFromCapture: true — вікна тесту на знімку немає");
        ui.SaveShot(combined, 61);
        Console.WriteLine("  Захист від знімків: на звичайному знімку екрана без захисту тест є, із захистом — його немає");
    }

    private async Task<Bitmap> ScreenDuringControlAsync(bool hide, Rectangle screen)
    {
        TestSession session = Scenarios.CreateSession(TestMode.Control, Scenarios.MainQuestionIds);
        session.Settings.Integrity.HideFromCapture = hide;
        session.RulesAcceptedAt = DateTime.Now;
        TestForm form = new(session);
        form.Show();
        await Task.Delay(1500);

        form.ShowQuestion(0);
        Answers.FillCorrect(form.CurrentView);
        await Task.Delay(800);
        Expect(form.IntegrityReport!.CaptureProtection == (hide ? "вікно сховано від знімків" : "вимкнено"),
            "стан захисту від знімків: " + form.IntegrityReport.CaptureProtection);

        Bitmap bmp = new(screen.Width, screen.Height);

        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(screen.Location, Point.Empty, screen.Size);   // звичайний знімок екрана (GDI)
        }

        await ui.MessageShotAsync(form, () => Ui.Click(form, "btnFinish"), null, Answer.Yes);
        return bmp;
    }

    // Два знімки поруч (кожен — удвічі менший) з підписами над ними
    private static Bitmap SideBySide(Bitmap left, string leftCaption, Bitmap right, string rightCaption)
    {
        const int gap = 12, caption = 40;
        int w = left.Width / 2, h = left.Height / 2;
        Bitmap result = new(w * 2 + gap, h + caption);

        using Graphics g = Graphics.FromImage(result);
        using Font font = new("Segoe UI", 13F, FontStyle.Bold);
        g.Clear(Color.White);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        TextRenderer.DrawText(g, leftCaption, font, new Rectangle(0, 0, w, caption), Color.FromArgb(31, 78, 121), Color.White);
        TextRenderer.DrawText(g, rightCaption, font, new Rectangle(w + gap, 0, w, caption), Color.FromArgb(31, 78, 121), Color.White);
        g.DrawImage(left, new Rectangle(0, caption, w, h));
        g.DrawImage(right, new Rectangle(w + gap, caption, w, h));
        return result;
    }

    private static Color Theme(int r, int g, int b)
    {
        return Color.FromArgb(r, g, b);
    }

    private static bool Near(Color a, Color b)
    {
        return Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B) < 30;
    }

    private static void Expect(bool condition, string what)
    {
        if (!condition)
        {
            throw new InvalidOperationException("ПІБ / журнал / збій: не виконано — " + what);
        }
    }
}
