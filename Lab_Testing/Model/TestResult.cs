using System.Text.Json.Serialization;

namespace Lab_Testing;

// Підсумок однієї спроби: хто, коли, скільки балів за кожне питання, оцінка
public class TestResult
{
    public string StudentName { get; set; } = "";
    public string Group { get; set; } = "";
    public TestMode Mode { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public int DurationSeconds { get; set; }
    public bool FinishedByTimeout { get; set; }        // контрольну завершено, бо скінчився час
    public List<QuestionResult> Questions { get; set; } = new();
    public double TotalScore { get; set; }
    public double MaxScore { get; set; }
    public double Percent { get; set; }
    public int Grade { get; set; }                     // оцінка за 12-бальною шкалою
    public int Seed { get; set; }                      // з якого числа «перемішано» варіант
    public Guid AttemptId { get; set; }
    public string AppVersion { get; set; } = "";

    [JsonIgnore]
    public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);

    // Складає підсумок із результатів перевірки всіх питань
    public static TestResult Create(TestSession session, List<QuestionResult> results, DateTime finishedAt, bool byTimeout)
    {
        double total = Math.Round(results.Sum(r => r.Score), 1);
        double max = results.Sum(r => r.MaxScore);
        double percent = max > 0 ? Math.Round(total / max * 100, 1) : 0;

        // Час — з точністю до секунди
        DateTime started = TrimToSeconds(session.StartedAt);
        DateTime finished = TrimToSeconds(finishedAt);

        return new TestResult
        {
            StudentName = session.StudentName,
            Group = session.Group,
            Mode = session.Mode,
            StartedAt = started,
            FinishedAt = finished,
            DurationSeconds = (int)(finished - started).TotalSeconds,
            FinishedByTimeout = byTimeout,
            Questions = results,
            TotalScore = total,
            MaxScore = max,
            Percent = percent,
            Grade = Grading.Grade(percent, session.Settings),
            Seed = session.Seed,
            AttemptId = session.AttemptId,
            AppVersion = AppInfo.Version
        };
    }

    private static DateTime TrimToSeconds(DateTime time)
    {
        return new DateTime(time.Ticks - time.Ticks % TimeSpan.TicksPerSecond, time.Kind);
    }
}
