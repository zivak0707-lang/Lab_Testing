namespace Lab_Testing;

// Режим тестування
public enum TestMode
{
    Training,   // «Навчання»: теорія, перевірка, пояснення
    Control     // «Контрольна»: без підказок, з таймером, одна спроба
}

// Одна спроба проходження тесту: хто проходить, у якому режимі і які питання випали
public class TestSession
{
    public string StudentName { get; }
    public string Group { get; }
    public TestMode Mode { get; }
    public BankSettings Settings { get; }

    // Число, з якого «перемішано» варіант. Записується у файл результату,
    // тому той самий варіант можна відтворити
    public int Seed { get; }

    // Генератор випадкових чисел цієї спроби (вибір питань і перемішування карток)
    public Random Random { get; }

    // Обрані питання: по одному кожного типу
    public List<Question> Questions { get; } = new();

    // Унікальний номер спроби
    public Guid AttemptId { get; } = Guid.NewGuid();

    // Коли учень почав відповідати (задається при відкритті вікна тесту)
    public DateTime StartedAt { get; set; } = DateTime.Now;

    // Номер спроби контрольної на цьому комп'ютері (1 — перша)
    public int AttemptNumber { get; set; } = 1;

    public TestSession(QuestionBankData bank, string studentName, string group, TestMode mode, int? seed = null)
    {
        StudentName = studentName;
        Group = group;
        Mode = mode;
        Settings = bank.Settings;
        Seed = seed ?? System.Random.Shared.Next();
        Random = new Random(Seed);

        // По одному випадковому питанню кожного типу, типи йдуть у фіксованому порядку
        foreach (QuestionType type in Enum.GetValues<QuestionType>())
        {
            List<Question> ofType = bank.Questions.Where(q => q.Type == type).ToList();
            Questions.Add(ofType[Random.Next(ofType.Count)]);
        }
    }
}
