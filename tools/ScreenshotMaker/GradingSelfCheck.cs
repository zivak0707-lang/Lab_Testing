using Lab_Testing;

namespace ScreenshotMaker;

// Самоперевірка оцінювання: кожне з 20 питань у трьох станах + межі 12-бальної шкали
internal static class GradingSelfCheck
{
    public static void Run()
    {
        QuestionBankData bank = QuestionBank.Load(out _);

        foreach (Question q in bank.Questions)
        {
            Check(q, _ => { }, 0, mustHaveMistakes: true, "порожня відповідь");
            Check(q, Answers.FillCorrect, 20, mustHaveMistakes: false, "усе правильно");
            Check(q, Answers.FillWithMistakes, Answers.ExpectedMistakesScore(q), mustHaveMistakes: true, "з помилками");
        }

        // Межі шкали: відсоток -> очікувана оцінка
        (double Percent, int Grade)[] cases =
        {
            (0, 1), (8.9, 1), (9, 2), (50.9, 6), (51, 7), (71.6, 9), (91.9, 11), (92, 12), (100, 12)
        };

        foreach ((double percent, int grade) in cases)
        {
            int actual = Grading.Grade(percent, bank.Settings);

            if (actual != grade)
            {
                throw new InvalidOperationException($"Шкала оцінок: {percent} % дає {actual} замість {grade}");
            }
        }

        Console.WriteLine($"  Перевірка оцінювання: {bank.Questions.Count} питань × 3 варіанти відповіді + шкала оцінок — усе правильно");
    }

    private static void Check(Question q, Action<QuestionView> fill, double expected, bool mustHaveMistakes, string what)
    {
        using QuestionView view = QuestionView.Create(q, new Random(7));
        fill(view);
        QuestionResult result = view.Evaluate(highlight: false);

        bool scoreOk = Math.Abs(result.Score - expected) < 0.001;
        bool mistakesOk = (result.Mistakes.Count > 0) == mustHaveMistakes;

        if (!scoreOk || !mistakesOk)
        {
            throw new InvalidOperationException(
                $"Питання №{q.Id} ({what}): {result.Score} балів і {result.Mistakes.Count} помилок, очікувалось {expected}");
        }

        // «Що виправити» є завжди, коли є помилки, і ніколи не називає правильної відповіді
        bool issuesOk = (result.Issues.Count > 0) == mustHaveMistakes;
        bool noAnswers = result.Issues.All(i => !i.Contains("правильно"));

        if (!issuesOk || !noAnswers)
        {
            throw new InvalidOperationException(
                $"Питання №{q.Id} ({what}): список «що виправити» неповний або підказує відповідь: " +
                string.Join(" | ", result.Issues));
        }
    }
}
