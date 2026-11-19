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

    public async Task RunAsync(int maxStage)
    {
        if (maxStage >= 1)
        {
            await Stage1Async();
        }
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

        string badGap = json.Replace("Цикл {for} зручно", "Цикл {for зручно");
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
