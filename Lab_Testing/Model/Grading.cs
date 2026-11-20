using System.Globalization;
using System.Text.Json.Serialization;

namespace Lab_Testing;

// Результат перевірки одного питання
public class QuestionResult
{
    public int QuestionId { get; set; }
    public QuestionType Type { get; set; }
    public string Title { get; set; } = "";
    public string Topic { get; set; } = "";
    public double Score { get; set; }                       // набрані бали (з частковими)
    public double MaxScore { get; set; } = Grading.QuestionMaxScore;
    public int Correct { get; set; }                        // зараховано правильних розміщень
    public int Total { get; set; }                          // з скількох (Score = 20 × Correct / Total)
    public string Summary { get; set; } = "";               // коротко: «правильно 4 з 6»
    public List<string> Mistakes { get; set; } = new();     // опис кожної помилки разом із правильною відповіддю
    public int Seconds { get; set; }                        // скільки секунд учень провів на цьому питанні (контрольна)

    // Пояснення з банку (для сторінки результату). У файл результату не пишеться:
    // учні пересилають файли, і пояснення з відповідями не повинні «гуляти» між ними
    [JsonIgnore]
    public string Explanation { get; set; } = "";

    // Що виправити — без правильної відповіді (для перевірки в режимі «Навчання»; у файл не зберігається)
    [JsonIgnore]
    public List<string> Issues { get; set; } = new();
}

// Підрахунок балів і оцінки
public static class Grading
{
    // Балів за одне питання (5 питань = 100 балів)
    public const double QuestionMaxScore = 20;

    // Числа показуємо по-українському: 13,3
    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");

    // Частковий бал: частка правильних відповідей від 20 балів, з точністю до 0,1
    public static double Points(int correct, int total)
    {
        if (total <= 0)
        {
            return 0;
        }

        return Math.Round(QuestionMaxScore * Math.Clamp(correct, 0, total) / total, 1);
    }

    // Оцінка за 12-бальною шкалою: найбільша оцінка, мінімальний відсоток якої досягнуто
    public static int Grade(double percent, BankSettings settings)
    {
        int grade = 1;

        for (int g = 1; g <= settings.GradeMinPercent.Count; g++)
        {
            if (percent + 0.0001 >= settings.GradeMinPercent[g - 1])
            {
                grade = g;
            }
        }

        return grade;
    }

    // Рівень навчальних досягнень за оцінкою
    public static string Level(int grade)
    {
        return grade switch
        {
            <= 3 => "початковий рівень",
            <= 6 => "середній рівень",
            <= 9 => "достатній рівень",
            _ => "високий рівень"
        };
    }

    // 13.333 -> "13,3"
    public static string Format(double value)
    {
        return value.ToString("0.#", Ukrainian);
    }

    // 372 с -> "6 хв 12 с"
    public static string FormatDuration(TimeSpan time)
    {
        return $"{(int)time.TotalMinutes} хв {time.Seconds:00} с";
    }
}
