using Lab_Testing;

namespace ScreenshotMaker;

// Сценарії знімків для кожного етапу
internal class Scenarios
{
    private const string StudentName = "Іваненко Олена Петрівна";
    private const string StudentGroup = "471";

    private readonly Ui ui;

    public Scenarios(Ui ui)
    {
        this.ui = ui;
    }

    // Самоперевірка Drag-and-Drop справжньою мишею (замість знімків)
    public Task RunDragTestAsync()
    {
        return DragSelfTest.RunAsync(CreateSession(TestMode.Training, MainQuestionIds), ui);
    }

    // Питання, які видно на знімках для звіту: кошик №1, таблиця №6, номерки №9, пропуски №13, кроки №20
    private static readonly int[] MainQuestionIds = { 1, 6, 9, 13, 20 };

    // Знімки всіх питань банку (порожніх і заповнених) у теку dir — для перевірки розмітки
    public static async Task RunAllQuestionsAsync(string dir)
    {
        Directory.CreateDirectory(dir);
        QuestionBankData bank = QuestionBank.Load(out _);

        foreach (Question question in bank.Questions)
        {
            TestForm form = new(CreateSession(TestMode.Training, question.Id));
            form.Show();
            form.ShowQuestion((int)question.Type);   // типи йдуть у тесті по порядку

            await Ui.ShotToFileAsync(form, Path.Combine(dir, $"q{question.Id:00}_empty.png"));
            Answers.FillCorrect(form.CurrentView);
            await Ui.ShotToFileAsync(form, Path.Combine(dir, $"q{question.Id:00}_done.png"));

            await new Ui(dir).DialogShotAsync(form, () => Ui.Click(form, "btnFinish"), null, WindowCapture.IDYES);
            Console.WriteLine($"  питання №{question.Id}");
        }
    }

    public async Task RunAsync(int maxStage)
    {
        if (maxStage >= 1)
        {
            await Stage1Async();
        }

        if (maxStage >= 2)
        {
            await Stage2Async();
        }

        if (maxStage >= 3)
        {
            await Stage3Async();
        }

        if (maxStage >= 4)
        {
            await Stage4Async();
        }

        if (maxStage >= 5)
        {
            await Stage5Async();
        }

        if (maxStage >= 6)
        {
            await Stage6Async();
        }
    }

    // ===================== Етап 6: режим викладача =====================

    private async Task Stage6Async()
    {
        Console.WriteLine("Етап 6: режим викладача");
        QuestionBankData bank = QuestionBank.Load(out string source);

        // Вхід: вікно пароля і неправильний пароль
        using (StartForm start = new(bank, source))
        {
            start.Show();
            await Task.Delay(300);
            start.BeginInvoke(() => Ui.ClickLink(start, "lnkTeacher"));

            PasswordForm password = await Ui.WaitForFormAsync<PasswordForm>(37);
            await ui.ShotAsync(password, 37);
            Ui.SetText(password, "txtPassword", "123456");
            Ui.Click(password, "btnOk");
            await ui.ShotAsync(password, 38);
            password.Close();
            await Task.Delay(300);
            start.Close();
        }

        // Вікно викладача (порожнє) і його діалоги
        TeacherForm teacher = new(bank);
        teacher.Show();
        await Task.Delay(400);
        await ui.ShotAsync(teacher, 39);
        await ui.DialogShotAsync(teacher, () => Ui.Click(teacher, "btnOpenFiles"), 40, WindowCapture.IDCANCEL);
        await ui.FormShotAsync<ChangePasswordForm>(teacher, () => Ui.Click(teacher, "btnChangePassword"), 41);

        // Зразкові файли (у тимчасовій теці): 3 справжні, 1 змінений, 1 зіпсований і сторонній .json
        (string folder, string damaged) = await MakeSampleFilesAsync(bank);
        teacher.LoadFiles(new[] { folder });
        CheckEntries(teacher, valid: 3, modified: 1, damaged: 0, "відкриття теки");

        teacher.LoadFiles(new[] { damaged });
        CheckEntries(teacher, valid: 3, modified: 1, damaged: 1, "відкриття пошкодженого файлу");

        await ui.DialogShotAsync(teacher, () => Ui.Click(teacher, "btnExport"), 42, WindowCapture.IDCANCEL);

        // Подвійний клік по рядку зміненого файлу -> попередження («Ні» — не відкривати)
        DataGridView grid = Ui.Find<DataGridView>(teacher, "dgvResults");
        int modifiedRow = grid.Rows.Cast<DataGridViewRow>()
            .First(r => r.Tag is ResultEntry { Status: ResultFileStatus.Modified }).Index;
        await ui.DialogShotAsync(teacher, () => Ui.DoubleClickCell(grid, modifiedRow), 43, WindowCapture.IDNO);

        CheckCsv(teacher);
        teacher.Close();
    }

    // Складає контрольні трьох учнів і зберігає їхні файли в тимчасову теку (не на ваш робочий стіл)
    private async Task<(string Folder, string Damaged)> MakeSampleFilesAsync(QuestionBankData bank)
    {
        string folder = Path.Combine(Path.GetTempPath(), "Lab_Testing_зразки");

        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, true);
        }

        Directory.CreateDirectory(folder);
        string? tamperedSource = null;

        (string Name, Action<QuestionView> Fill)[] students =
        {
            ("Петренко Андрій", Answers.FillCorrect),
            ("Коваль Марія", Answers.FillWithMistakes),
            ("Шевчук Олег", _ => { })
        };

        for (int s = 0; s < students.Length; s++)
        {
            TestForm form = new(new TestSession(bank, students[s].Name, StudentGroup, TestMode.Control, 100 + s));
            form.Show();

            for (int i = 0; i < 5; i++)
            {
                form.ShowQuestion(i);
                students[s].Fill(form.CurrentView);
            }

            await ui.DialogShotAsync(form, () => Ui.Click(form, "btnFinish"), null, WindowCapture.IDYES);
            TestResult result = form.Result ?? throw new InvalidOperationException("Контрольна не завершилася");

            string json = ResultFile.ToSignedJson(result);
            File.WriteAllText(Path.Combine(folder, ResultFile.MakeFileName(result)), json);
            tamperedSource ??= s == 1 ? json : null;
        }

        // Змінений файл (як після правки оцінки в Блокноті), зіпсований файл і сторонній .json у теці
        string tampered = System.Text.RegularExpressions.Regex.Replace(tamperedSource!, "\"grade\": \\d+", "\"grade\": 12");
        File.WriteAllText(Path.Combine(folder, "Результат_Коваль_Марія_471_змінений.json"), tampered);
        File.WriteAllText(Path.Combine(folder, "questions.json"), QuestionBank.ReadEmbeddedJson());

        string damaged = Path.Combine(Path.GetTempPath(), "Результат_зіпсований.json");
        File.WriteAllText(damaged, "{ це вже не файл результату");

        return (folder, damaged);
    }

    private static void CheckEntries(TeacherForm teacher, int valid, int modified, int damaged, string what)
    {
        int v = teacher.Entries.Count(e => e.Status == ResultFileStatus.Valid);
        int m = teacher.Entries.Count(e => e.Status == ResultFileStatus.Modified);
        int d = teacher.Entries.Count(e => e.Status == ResultFileStatus.Damaged);

        if (v != valid || m != modified || d != damaged)
        {
            throw new InvalidOperationException($"Режим викладача ({what}): справжніх {v}, змінених {m}, пошкоджених {d}");
        }
    }

    // CSV: UTF-8 з BOM, роздільник з регіональних налаштувань, кирилиця, рядок на кожен файл
    private static void CheckCsv(TeacherForm teacher)
    {
        string csv = Path.Combine(Path.GetTempPath(), "Lab_Testing_зразки", "Результати.csv");
        CsvExport.Save(csv, teacher.Entries);

        byte[] bytes = File.ReadAllBytes(csv);
        string text = new System.Text.UTF8Encoding(false).GetString(bytes, 3, bytes.Length - 3);
        string[] lines = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        string separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        bool bom = bytes.Length > 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;

        if (!bom || lines.Length != teacher.Entries.Count + 1 || !lines[0].StartsWith("№" + separator + "ПІБ") ||
            !text.Contains("Коваль Марія") || !text.Contains("НІ — змінено"))
        {
            throw new InvalidOperationException("CSV для Excel сформовано неправильно");
        }

        Console.WriteLine($"  Режим викладача: 3 справжні, 1 змінений, 1 пошкоджений — розпізнано; " +
                          $"CSV: BOM, роздільник «{separator}», кирилиця — правильно");
    }

    // ===================== Етап 5: контрольна, таймер, файл результату =====================

    private async Task Stage5Async()
    {
        Console.WriteLine("Етап 5: контрольна і файл результату");

        TestSession session = CreateSession(TestMode.Control, MainQuestionIds);
        TestForm form = new(session);
        form.Show();
        await Task.Delay(300);

        // Частину питань розв'язано, таймер у шапці
        form.ShowQuestion(0);
        Answers.FillCorrect(form.CurrentView);
        form.ShowQuestion(2);
        Answers.FillWithMistakes(form.CurrentView);
        form.ShowQuestion(1);
        Answers.FillWithMistakes(form.CurrentView);
        await ui.ShotAsync(form, 29);

        // «Завершити контрольну» -> підтвердження -> «Так»
        await ui.DialogShotAsync(form, () => Ui.Click(form, "btnFinish"), 31, WindowCapture.IDYES);
        TestResult result = form.Result ?? throw new InvalidOperationException("Контрольна не завершилася");

        string json = ResultFile.ToSignedJson(result);
        CheckSignature(json);

        // У файлі — звичайний текст без розмітки, немає пояснень, а з вимкненим розбором — і правильних відповідей
        if (json.Contains('`'))
        {
            throw new InvalidOperationException("У файлі результату лишилася розмітка (зворотні лапки)");
        }

        result.RemoveCorrectAnswers();
        string hidden = ResultFile.ToSignedJson(result);

        // Правильна відповідь у помилках завжди записана як «правильно: …» (підсумки «правильно 4 з 6» — це лише кількість)
        if (json.Contains("\"explanation\"") || hidden.Contains("правильно:", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("У файлі результату є правильні відповіді, яких там не має бути");
        }

        Console.WriteLine("  Файл результату: звичайний текст без розмітки, без пояснень; з вимкненим розбором — без правильних відповідей");

        // Таймер і «Час вийшов»: банк з обмеженням 1 хвилина (чекаємо її по-справжньому)
        QuestionBankData shortBank = QuestionBank.Load(out _);
        shortBank.Settings.ControlTimeMinutes = 1;
        TestSession shortSession = new(shortBank, StudentName, StudentGroup, TestMode.Control, session.Seed);
        TestForm shortForm = new(shortSession);
        shortForm.Show();
        shortForm.ShowQuestion(3);
        Answers.FillCorrect(shortForm.CurrentView);
        await Task.Delay(2000);
        await ui.ShotAsync(shortForm, 30);

        Console.WriteLine("  Чекаємо, поки мине 1 хвилина контрольної...");
        await ui.DialogShotAsync(shortForm, () => { }, 32, WindowCapture.IDOK, timeoutMs: 75000);

        if (shortForm.Result is not { FinishedByTimeout: true })
        {
            throw new InvalidOperationException("Після закінчення часу контрольна мала завершитися автоматично");
        }

        Console.WriteLine("  Таймер: після 1 хвилини контрольну завершено автоматично — правильно");
    }

    // Самоперевірка підпису: справжній файл, змінена оцінка, зіпсований файл; очищення назви файлу
    private static void CheckSignature(string json)
    {
        if (ResultFile.Check(json, out _) != ResultFileStatus.Valid)
        {
            throw new InvalidOperationException("Щойно збережений файл не пройшов перевірку підпису");
        }

        string tampered = System.Text.RegularExpressions.Regex.Replace(json, "\"grade\": \\d+", "\"grade\": 12");

        if (tampered == json || ResultFile.Check(tampered, out _) != ResultFileStatus.Modified)
        {
            throw new InvalidOperationException("Файл зі зміненою оцінкою не розпізнано як змінений");
        }

        if (ResultFile.Check("{ це не файл результату", out _) != ResultFileStatus.Damaged)
        {
            throw new InvalidOperationException("Зіпсований файл не розпізнано як пошкоджений");
        }

        if (ResultFile.Clean(" Іваненко  О.П. / \"471\"? ") != "Іваненко_О_П_471")
        {
            throw new InvalidOperationException("Недопустимі символи в назві файлу не очищено: " + ResultFile.Clean(" Іваненко  О.П. / \"471\"? "));
        }

        Console.WriteLine("  Підпис: справжній файл — «справжній», змінена оцінка — «змінено», зіпсований — «пошкоджений»");
    }

    // ===================== Етап 4: оцінювання, режим «Навчання», сторінка результату =====================

    private async Task Stage4Async()
    {
        Console.WriteLine("Етап 4: перевірка в режимі «Навчання» і сторінка результату");
        GradingSelfCheck.Run();

        TestForm form = new(CreateSession(TestMode.Training, MainQuestionIds));
        form.Show();
        await Task.Delay(300);

        // Таблиця: теорія; потім відповідь з двома помилками і перевірка
        form.ShowQuestion(1);
        await ui.ShotAsync(form, 20);
        Answers.FillWithMistakes(form.CurrentView);
        Ui.Click(form, "btnCheck");
        await ui.ShotAsync(form, 21);

        // Пропуски: неправильне слово і порожній пропуск
        form.ShowQuestion(3);
        Answers.FillWithMistakes(form.CurrentView);
        Ui.Click(form, "btnCheck");
        await ui.ShotAsync(form, 22);

        // Номерки: усе правильно
        form.ShowQuestion(2);
        Answers.FillCorrect(form.CurrentView);
        Ui.Click(form, "btnCheck");
        await ui.ShotAsync(form, 23);

        // Питання 1 і 5 без відповіді — попередження при завершенні («Ні» — повертаємося до тесту)
        await ui.DialogShotAsync(form, () => Ui.Click(form, "btnFinish"), 24, WindowCapture.IDNO);

        // Кошик: прибрано лише частину зайвих і одну потрібну — «невидимі» помилки на слайді
        form.ShowQuestion(0);
        Answers.FillBasketPartly(form.CurrentView);
        Ui.Click(form, "btnCheck");
        await ui.ShotAsync(form, 27);

        // Таблиця: друга спроба (картку переклали і повернули) — після неї можна відкрити відповідь
        form.ShowQuestion(1);
        QuestionView table = form.CurrentView;
        CardLabel moved = table.GetZone("row:0").Cards[0];
        table.PlaceCard(moved, table.GetZone("palette"), DragDropEffects.Move);
        table.PlaceCard(moved, table.GetZone("row:0"), DragDropEffects.Move);
        Ui.Click(form, "btnCheck");

        if (!Ui.Find<Button>(form, "btnShowAnswer").Enabled)
        {
            throw new InvalidOperationException("Після двох спроб кнопка «Показати правильну відповідь» мала стати доступною");
        }

        Ui.Click(form, "btnShowAnswer");
        await ui.ShotAsync(form, 28);

        form.ShowQuestion(0);
        Answers.FillWithMistakes(form.CurrentView);
        form.ShowQuestion(4);
        Answers.FillWithMistakes(form.CurrentView);

        await ui.DialogShotAsync(form, () => Ui.Click(form, "btnFinish"), null, WindowCapture.IDYES);

        // Підсумок перевіряємо, але знімок сторінки результату (рисунок 25) автор робить сам:
        // автоматичний прохід триває кілька секунд, і такий час проходження на знімку виглядав би неправдоподібно
        TestResult result = form.Result ?? throw new InvalidOperationException("Тест не завершився");

        if (Math.Abs(result.TotalScore - 70.3) > 0.001 || result.Grade != 9)
        {
            throw new InvalidOperationException($"Очікувалось 70,3 бала і оцінка 9, а вийшло {result.TotalScore} і {result.Grade}");
        }

        Console.WriteLine($"  Підсумок тесту: {Grading.Format(result.TotalScore)} бала, оцінка {result.Grade} — як очікувалось");

        // Попередній перегляд сторінки результату для перевірки розмітки (не для звіту, у тимчасову теку)
        using ResultForm resultForm = new(result, showReview: true);
        resultForm.Show();
        string preview = Path.Combine(Path.GetTempPath(), "lab_testing_result_preview.png");
        await Ui.ShotToFileAsync(resultForm, preview);
        resultForm.Close();
        Console.WriteLine("  Перегляд сторінки результату (не для звіту): " + preview);
    }

    // Спроба тесту, у яку потрапили саме ці питання (підбираємо seed, щоб знімки були відтворювані)
    private static TestSession CreateSession(TestMode mode, params int[] wantedIds)
    {
        QuestionBankData bank = QuestionBank.Load(out _);

        for (int seed = 1; seed < 100000; seed++)
        {
            TestSession session = new(bank, StudentName, StudentGroup, mode, seed);

            if (wantedIds.All(id => session.Questions.Any(q => q.Id == id)))
            {
                return session;
            }
        }

        throw new InvalidOperationException("Не знайдено seed з потрібними питаннями");
    }

    // ===================== Етап 2: вікно тесту, «Кошик» і «Таблиця» =====================

    private async Task Stage2Async()
    {
        Console.WriteLine("Етап 2: вікно тесту, «Кошик» і «Таблиця»");

        await ShowEmptyAndDoneAsync(new[] { (0, 7), (1, 9) });
    }

    // ===================== Етап 3: номерки, пропуски, порядок кроків =====================

    private async Task Stage3Async()
    {
        Console.WriteLine("Етап 3: «Номерки», «Пропуски», «Порядок кроків»");

        await ShowEmptyAndDoneAsync(new[] { (2, 13), (3, 15), (4, 17) });
    }

    // Для кожної пари (номер питання в тесті, номер рисунка): знімок до відповіді (рисунок N)
    // і з правильно розставленими відповідями (рисунок N + 1)
    private async Task ShowEmptyAndDoneAsync((int Index, int Shot)[] questions)
    {
        TestForm form = new(CreateSession(TestMode.Training, MainQuestionIds));
        form.Show();
        await Task.Delay(300);

        foreach ((int index, int shot) in questions)
        {
            form.ShowQuestion(index);
            await ui.ShotAsync(form, shot);
            Answers.FillCorrect(form.CurrentView);
            await ui.ShotAsync(form, shot + 1);
        }

        // Закриваємо тест кнопкою «Завершити тест» -> «Так»
        await ui.DialogShotAsync(form, () => Ui.Click(form, "btnFinish"), null, WindowCapture.IDYES);
    }

    // ===================== Етап 1: стартова сторінка =====================

    private async Task Stage1Async()
    {
        Console.WriteLine("Етап 1: стартова сторінка");

        QuestionBankData bank = QuestionBank.Load(out string source);

        if (bank.Questions.Count != 20)
        {
            throw new InvalidOperationException($"У банку {bank.Questions.Count} питань замість 20");
        }

        CheckBankValidation();

        using StartForm form = new(bank, source);
        form.Show();
        await Task.Delay(300);

        Ui.SetText(form, "txtName", StudentName);
        Ui.SetText(form, "txtGroup", StudentGroup);
        Ui.Check(form, "rbTraining");
        await ui.ShotAsync(form, 1);

        Ui.Check(form, "rbControl");
        await ui.ShotAsync(form, 2);

        // Порожнє ПІБ -> попередження
        Ui.SetText(form, "txtName", "");
        await ui.DialogShotAsync(form, () => Ui.Click(form, "btnStart"), 3, WindowCapture.IDOK);
        Ui.SetText(form, "txtName", StudentName);

        await ui.FormShotAsync<AboutForm>(form, () => Ui.ClickLink(form, "lnkAbout"), 4);

        form.Close();
    }

    // Самоперевірка: банк з навмисними помилками має відхилятися з зрозумілим повідомленням
    private static void CheckBankValidation()
    {
        string json = QuestionBank.ReadEmbeddedJson();

        string badCategory = json.Replace("\"category\": \"Логічний тип\"", "\"category\": \"Логічні типи\"");
        ExpectBankError(badCategory, "Питання №9");

        string badGap = json.Replace("Цикл {for} з лічильником", "Цикл {for з лічильником");
        ExpectBankError(badGap, "Питання №13");

        string badJson = json.Replace("\"id\": 5,", "\"id\": 5");
        ExpectBankError(badJson, "рядку");

        Console.WriteLine("  Перевірка банку: помилки в банку розпізнаються правильно");
    }

    private static void ExpectBankError(string json, string expectedText)
    {
        try
        {
            QuestionBank.Parse(json, "тест");
        }
        catch (BankException ex) when (ex.Message.Contains(expectedText))
        {
            return;
        }

        throw new InvalidOperationException($"Банк з помилкою не відхилено (очікувався текст «{expectedText}»)");
    }
}
