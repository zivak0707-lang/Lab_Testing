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
