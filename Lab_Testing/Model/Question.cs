using System.Text.RegularExpressions;

namespace Lab_Testing;

// Тип завдання Drag-and-Drop
public enum QuestionType
{
    Basket,      // «Кошик»: прибрати зайві картки зі слайда
    Table,       // таблиця відповідностей
    Categories,  // номерки на рядки категорій
    Gaps,        // слова в пропуски (слово копіюється)
    Order        // кроки алгоритму в правильному порядку
}

// Увесь банк питань: налаштування + список питань (так влаштований файл questions.json)
public class QuestionBankData
{
    public BankSettings Settings { get; set; } = new();
    public List<Question> Questions { get; set; } = new();
}

// Налаштування тесту, які викладач може змінити у файлі банку
public class BankSettings
{
    // Час на контрольну, хвилин
    public int ControlTimeMinutes { get; set; } = 15;

    // Чи показувати розбір помилок після контрольної (бали й оцінку видно завжди)
    public bool ShowReviewAfterControl { get; set; } = true;

    // Мінімальний відсоток для оцінок 1, 2, ..., 12
    public List<double> GradeMinPercent { get; set; } = new() { 0, 9, 17, 26, 34, 42, 51, 59, 67, 76, 84, 92 };
}

// Одне питання банку. Для кожного типу заповнюються лише потрібні поля
public class Question
{
    public int Id { get; set; }                    // номер питання в банку
    public QuestionType Type { get; set; }         // тип завдання
    public string Topic { get; set; } = "";        // тема: «Типи даних», «Оператори» ...
    public string Title { get; set; } = "";        // коротка назва (для таблиць результатів)
    public string Task { get; set; } = "";         // текст завдання для учня
    public string Theory { get; set; } = "";       // коротка теорія (режим «Навчання»)
    public string Explanation { get; set; } = "";  // пояснення правильної відповіді

    // «Кошик» і «Номерки»: елементи-картки
    public List<QuestionItem> Items { get; set; } = new();

    // «Таблиця»: заголовки двох стовпців, рядки і зайві картки
    public List<string> Columns { get; set; } = new();
    public List<TableRow> Rows { get; set; } = new();
    public List<string> ExtraCards { get; set; } = new();

    // «Номерки»: назви категорій (рядків, на які кладуть номерки)
    public List<string> Categories { get; set; } = new();

    // «Пропуски»: текст, де пропуски записані у фігурних дужках, напр. "{int} a = 5;"
    public string Text { get; set; } = "";
    public List<string> ExtraWords { get; set; } = new();

    // «Порядок кроків»: кроки у ПРАВИЛЬНОМУ порядку (у тесті вони перемішуються)
    public List<string> Steps { get; set; } = new();

    // Шукає в тексті пропуски виду {слово}
    private static readonly Regex GapRegex = new(@"\{([^{}]+)\}");

    // Правильні слова для пропусків у порядку їх появи в тексті
    public List<string> GetGapAnswers()
    {
        return GapRegex.Matches(Text).Select(m => m.Groups[1].Value.Trim()).ToList();
    }
}

// Картка для «Кошика» або «Номерків»
public class QuestionItem
{
    public string Text { get; set; } = "";       // текст картки
    public bool Keep { get; set; }               // «Кошик»: true — картка потрібна, лишається на слайді
    public string Category { get; set; } = "";   // «Номерки»: правильна категорія
}

// Рядок «Таблиці відповідностей»
public class TableRow
{
    public string Left { get; set; } = "";       // текст лівої клітинки (опис або значення)
    public string Answer { get; set; } = "";     // картка, яку треба покласти в праву клітинку
}

// Українські назви типів завдань і режимів
public static class UkrainianNames
{
    public static string ToUkrainian(this QuestionType type)
    {
        return type switch
        {
            QuestionType.Basket => "Кошик",
            QuestionType.Table => "Таблиця відповідностей",
            QuestionType.Categories => "Номерки на категорії",
            QuestionType.Gaps => "Слова в пропуски",
            QuestionType.Order => "Порядок кроків",
            _ => type.ToString()
        };
    }

    public static string ToUkrainian(this TestMode mode)
    {
        return mode == TestMode.Training ? "Навчання" : "Контрольна";
    }
}
