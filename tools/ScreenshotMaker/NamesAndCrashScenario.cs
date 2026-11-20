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
        TestSession session = Scenarios.CreateSession(TestMode.Control, Scenarios.MainQuestionIds);
        session.RulesAcceptedAt = DateTime.Now;
        TestForm form = new(session);
        form.Show();
        await Task.Delay(1000);

        form.ShowQuestion(0);
        Answers.FillCorrect(form.CurrentView);
        Expect(TestForm.RunningControl == form && ZOrder.IsTopMost(form.Handle), "контрольна йде на весь екран поверх усіх");

        // Справжній непередбачений виняток у потоці вікон (як помилка в коді програми)
        form.BeginInvoke(new Action(() => throw new InvalidOperationException("Перевірка аварійного завершення (утиліта знімків)")));
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
        Console.WriteLine("  Збій під час контрольної: відповіді збережено (файл справжній), режим на весь екран знято, повідомлення видно");
    }

    // Журнал: є події доброчесності, початок і кінець контрольної, помилка зі стеком — і немає шляху профілю Windows
    private static void CheckLog()
    {
        string log = File.ReadAllText(AppLog.FilePath);
        string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        Expect(log.Contains("| доброчесність |") && log.Contains("підтвердив правила"), "журнал: події доброчесності");
        Expect(log.Contains("| контрольна | почалась") && log.Contains("через помилку програми"), "журнал: початок і кінець контрольної");
        Expect(log.Contains("| помилка |") && log.Contains("InvalidOperationException"), "журнал: помилка програми");
        Expect(!log.Contains(profile, StringComparison.OrdinalIgnoreCase), "журнал: без шляху до профілю користувача");

        Console.WriteLine("  Журнал log.txt: події доброчесності, початок і кінець контрольної, помилка; шлях профілю приховано");
    }

    private static void Expect(bool condition, string what)
    {
        if (!condition)
        {
            throw new InvalidOperationException("ПІБ / журнал / збій: не виконано — " + what);
        }
    }
}
