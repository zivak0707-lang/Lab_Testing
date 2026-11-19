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
        return DragSelfTest.RunAsync(CreateSession(TestMode.Training, 1, 6));
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

        TestForm form = new(CreateSession(TestMode.Training, 1, 6));
        form.Show();
        await Task.Delay(300);

        form.ShowQuestion(0);
        await ui.ShotAsync(form, 7);
        Answers.FillCorrect(form.CurrentView);
        await ui.ShotAsync(form, 8);

        form.ShowQuestion(1);
        await ui.ShotAsync(form, 9);
        Answers.FillCorrect(form.CurrentView);
        await ui.ShotAsync(form, 10);

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
