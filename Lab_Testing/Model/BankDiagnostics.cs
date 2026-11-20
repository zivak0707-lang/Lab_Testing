using System.Text.Json;
using System.Text.RegularExpressions;

namespace Lab_Testing;

// Зрозумілі вчителеві пояснення помилок у файлі банку: українською, з номером рядка, номером питання
// і назвою поля людською мовою, з порадою, як виправити. Технічний текст винятків не показується
public static class BankDiagnostics
{
    // Якого вигляду має бути значення поля
    private enum Kind
    {
        Number,       // ціле число
        Bool,         // true / false
        Text,         // текст у лапках
        TextOrLines,  // текст або список рядків (теорія, пояснення, підказка)
        TextList,     // список рядків
        NumberList,   // список чисел
        Items,        // список карток { text, keep, category }
        Rows,         // список рядків таблиці { left, answer }
        Integrity     // розділ налаштувань доброчесності { enabled, exitLimit, … }
    }

    // Поле: назва людською мовою і вигляд значення
    private record Field(string Name, Kind Kind);

    private static readonly Dictionary<string, Field> RootFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["settings"] = new("налаштування", Kind.Text),
        ["questions"] = new("список питань", Kind.Text)
    };

    private static readonly Dictionary<string, Field> SettingsFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["controlTimeMinutes"] = new("час контрольної", Kind.Number),
        ["showReviewAfterControl"] = new("розбір після контрольної", Kind.Bool),
        ["gradeMinPercent"] = new("шкала оцінок", Kind.NumberList),
        ["integrity"] = new("режим доброчесності", Kind.Integrity)
    };

    private static readonly Dictionary<string, Field> IntegrityFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["enabled"] = new("увімкнено", Kind.Bool),
        ["exitLimit"] = new("ліміт виходів", Kind.Number),
        ["shortExitSeconds"] = new("поріг короткого виходу", Kind.Number),
        ["hideFromCapture"] = new("захист від знімків екрана", Kind.Bool)
    };

    private static readonly Dictionary<string, Field> QuestionFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = new("номер питання", Kind.Number),
        ["type"] = new("тип завдання", Kind.Text),
        ["topic"] = new("тема", Kind.Text),
        ["title"] = new("коротка назва", Kind.Text),
        ["task"] = new("текст завдання", Kind.Text),
        ["theory"] = new("теорія", Kind.TextOrLines),
        ["explanation"] = new("пояснення", Kind.TextOrLines),
        ["hint"] = new("підказка", Kind.TextOrLines),
        ["items"] = new("картки", Kind.Items),
        ["columns"] = new("заголовки стовпців", Kind.TextList),
        ["rows"] = new("рядки таблиці", Kind.Rows),
        ["extraCards"] = new("зайві картки", Kind.TextList),
        ["categories"] = new("категорії", Kind.TextList),
        ["text"] = new("текст з пропусками", Kind.Text),
        ["extraWords"] = new("зайві слова", Kind.TextList),
        ["steps"] = new("кроки", Kind.TextList)
    };

    private static readonly Dictionary<string, Field> ItemFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["text"] = new("текст картки", Kind.Text),
        ["keep"] = new("лишити на слайді", Kind.Bool),
        ["category"] = new("категорія", Kind.Text)
    };

    private static readonly Dictionary<string, Field> RowFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["left"] = new("ліва клітинка", Kind.Text),
        ["answer"] = new("правильна картка", Kind.Text)
    };

    // Усі відомі назви полів (для підказки «можливо, ви мали на увазі…»)
    private static readonly string[] AllFieldNames = RootFields.Keys.Concat(SettingsFields.Keys).Concat(IntegrityFields.Keys)
        .Concat(QuestionFields.Keys).Concat(ItemFields.Keys).Concat(RowFields.Keys).Distinct().ToArray();

    // Назва поля людською мовою (для повідомлень про зміст)
    public static string FieldLabel(string jsonName)
    {
        return QuestionFields.TryGetValue(jsonName, out Field? field) ? $"«{field.Name}» ({jsonName})" : jsonName;
    }

    // ===================== Синтаксис JSON: лапки, коми, дужки =====================

    // Пояснює помилку запису: що в рядку, що найімовірніше не так і як виправити
    public static List<string> DescribeSyntaxError(string json, long? lineIndex)
    {
        string[] lines = json.Replace("\r", "").Split('\n');
        int index = (int)Math.Clamp(lineIndex ?? lines.Length - 1, 0, lines.Length - 1);
        string line = lines[index].Trim();
        string previous = PreviousMeaningfulLine(lines, index, out int previousIndex);
        string where = DescribeLocation(lines, index);

        // Аналізуємо рядки без коментарів // (у коментарях можуть бути будь-які символи)
        (string problem, string fix) = Diagnose(StripComment(line).Trim(), previous, previousIndex, index, lines);

        List<string> result = new() { $"Рядок {index + 1}{where}: {problem}" };

        if (line.Length > 0)
        {
            result.Add($"Текст рядка {index + 1}: {Shorten(line)}");
        }

        result.Add("Як виправити: " + fix);
        return result;
    }

    private static (string Problem, string Fix) Diagnose(string line, string previous, int previousIndex, int index, string[] lines)
    {
        if (line.Contains('“') || line.Contains('”') || line.Contains('„'))
        {
            return ("текст узято в «друкарські» лапки “ ”, а файл банку розуміє лише звичайні лапки \".",
                    "замініть лапки “ ” навколо тексту на звичайні \" (так їх ставить Блокнот).");
        }

        if (CountQuotes(line) % 2 == 1)
        {
            return ("у цьому рядку не закрито лапку \": текст почався, але не закінчився.",
                    "кожен текст має починатися й закінчуватися звичайною лапкою \". Допишіть пропущену лапку. " +
                    "Якщо лапка потрібна всередині самого тексту, пишіть її як \\\".");
        }

        if (Regex.IsMatch(line, "^\"[^\"]*\"\\s+[\"\\[{0-9tf]"))
        {
            return ("між назвою поля і значенням немає двокрапки.",
                    "поставте двокрапку після назви поля, наприклад: \"title\": \"Типи даних\".");
        }

        bool previousEndsValue = Regex.IsMatch(previous, "([\"}\\]]|\\d|true|false|null)$");
        bool lineStartsValue = Regex.IsMatch(line, "^[\"{\\[]");

        if (previousIndex >= 0 && previousEndsValue && lineStartsValue)
        {
            return ($"у кінці рядка {previousIndex + 1} не вистачає коми.",
                    $"допишіть кому «,» у кінці рядка {previousIndex + 1}: елементи списку і поля мають розділятися комами.");
        }

        if (index >= lines.Length - 1 || lines.Skip(index + 1).All(l => l.Trim().Length == 0))
        {
            return ("файл закінчився, а якусь дужку { або [ так і не закрито.",
                    "перевірте, щоб кожна відкрита фігурна дужка { мала свою закриваючу }, а кожна квадратна [ — свою ].");
        }

        return ("тут порушено запис файлу банку.",
                "перевірте лапки \", коми й дужки { } [ ] у цьому рядку і в рядку вище. " +
                "Часто причина — пропущена кома в кінці попереднього рядка або зайвий символ.");
    }

    // Кількість лапок, не рахуючи \" усередині тексту
    private static int CountQuotes(string line)
    {
        int count = 0;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\\')
            {
                i++;   // пропускаємо символ після зворотної риски (\" чи \\)
            }
            else if (line[i] == '"')
            {
                count++;
            }
        }

        return count;
    }

    // Попередній рядок, який не порожній і не коментар
    private static string PreviousMeaningfulLine(string[] lines, int index, out int previousIndex)
    {
        for (int i = index - 1; i >= 0; i--)
        {
            string text = StripComment(lines[i]).Trim();

            if (text.Length > 0)
            {
                previousIndex = i;
                return text;
            }
        }

        previousIndex = -1;
        return "";
    }

    // Рядок без коментаря // (якщо // стоїть поза лапками)
    private static string StripComment(string line)
    {
        bool inText = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\\')
            {
                i++;
            }
            else if (line[i] == '"')
            {
                inText = !inText;
            }
            else if (!inText && line[i] == '/' && i + 1 < line.Length && line[i + 1] == '/')
            {
                return line[..i];
            }
        }

        return line;
    }

    // «питання №3, поле «теорія»» — де знаходиться рядок (шукаємо вище найближчі "id" і назву поля)
    private static string DescribeLocation(string[] lines, int index)
    {
        string? field = null;
        string? question = null;

        for (int i = index; i >= 0 && (field == null || question == null); i--)
        {
            Match key = Regex.Match(lines[i], "\"(\\w+)\"\\s*:");

            if (field == null && key.Success)
            {
                field = key.Groups[1].Value;
            }

            Match id = Regex.Match(lines[i], "\"id\"\\s*:\\s*(\\d+)");

            if (question == null && id.Success)
            {
                question = id.Groups[1].Value;
            }

            if (question == null && Regex.IsMatch(lines[i], "\"settings\"\\s*:"))
            {
                question = "settings";
            }
        }

        List<string> parts = new();

        if (question == "settings")
        {
            parts.Add("розділ налаштувань");
        }
        else if (question != null)
        {
            parts.Add($"питання №{question}");
        }

        if (field != null && field != "id")
        {
            Field? info = Lookup(field);
            parts.Add(info != null ? $"поле «{info.Name}» ({field})" : $"поле {field}");
        }

        return parts.Count > 0 ? " (" + string.Join(", ", parts) + ")" : "";
    }

    private static string Shorten(string text)
    {
        return text.Length > 90 ? text[..90] + "…" : text;
    }

    // ===================== Будова банку: поля і їхній вигляд =====================

    // Перевіряє, що в банку є потрібні поля правильного вигляду і немає невідомих
    public static List<string> CheckStructure(JsonElement root, string json)
    {
        List<string> errors = new();

        if (root.ValueKind != JsonValueKind.Object)
        {
            errors.Add("Файл банку має починатися з фігурної дужки { і закінчуватися нею ж }.");
            return errors;
        }

        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (!RootFields.ContainsKey(property.Name))
            {
                errors.Add(UnknownField(property.Name, "на початку файлу"));
            }
        }

        if (root.TryGetProperty("settings", out JsonElement settings))
        {
            if (settings.ValueKind != JsonValueKind.Object)
            {
                errors.Add("Налаштування (settings) мають бути у фігурних дужках { }.");
            }
            else
            {
                CheckFields(settings, SettingsFields, "Налаштування", errors);
            }
        }

        if (!root.TryGetProperty("questions", out JsonElement questions) || questions.ValueKind != JsonValueKind.Array)
        {
            errors.Add("У файлі немає списку питань: потрібне поле \"questions\": [ … ] з питаннями у квадратних дужках.");
            return errors;
        }

        int number = 0;

        foreach (JsonElement question in questions.EnumerateArray())
        {
            number++;

            if (question.ValueKind != JsonValueKind.Object)
            {
                errors.Add($"{number}-е питання за порядком: кожне питання має бути у фігурних дужках {{ }}.");
                continue;
            }

            string label = QuestionLabel(question, number, json);
            CheckFields(question, QuestionFields, label, errors);

            foreach (string required in new[] { "id", "type", "task" })
            {
                if (!question.TryGetProperty(required, out _))
                {
                    errors.Add($"{label}: немає обов'язкового поля «{QuestionFields[required].Name}» ({required}).");
                }
            }

            if (question.TryGetProperty("type", out JsonElement type) && type.ValueKind == JsonValueKind.String &&
                !Enum.TryParse<QuestionType>(type.GetString(), ignoreCase: true, out _))
            {
                errors.Add($"{label}: невідомий тип завдання «{type.GetString()}». Можливі: " +
                           string.Join(", ", Enum.GetValues<QuestionType>().Select(t => $"{t} ({t.ToUkrainian()})")) + ".");
            }
        }

        return errors;
    }

    // «Питання №3 (рядок 45)» — щоб вчитель знайшов питання у файлі
    public static string QuestionLabel(JsonElement question, int number, string json)
    {
        if (question.TryGetProperty("id", out JsonElement id) && id.ValueKind == JsonValueKind.Number && id.TryGetInt32(out int value))
        {
            int line = FindQuestionLine(json, value);
            return line > 0 ? $"Питання №{value} (рядок {line})" : $"Питання №{value}";
        }

        return $"{number}-е питання за порядком";
    }

    // Рядок, де в тексті банку записано "id": N (0 — не знайдено)
    public static int FindQuestionLine(string json, int id)
    {
        Match match = Regex.Match(json, $"\"id\"\\s*:\\s*{id}\\b");
        return match.Success ? json[..match.Index].Count(c => c == '\n') + 1 : 0;
    }

    private static void CheckFields(JsonElement element, Dictionary<string, Field> fields, string label, List<string> errors)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!fields.TryGetValue(property.Name, out Field? field))
            {
                errors.Add(UnknownField(property.Name, label));
                continue;
            }

            // Вкладений розділ «integrity»: перевіряємо його поля так само
            if (field.Kind == Kind.Integrity)
            {
                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    CheckFields(property.Value, IntegrityFields, "Налаштування доброчесності", errors);
                }
                else
                {
                    errors.Add($"{label}, поле «{field.Name}» ({property.Name}): потрібні фігурні дужки {{ … }} з полями " +
                               "enabled, exitLimit, shortExitSeconds, hideFromCapture.");
                }

                continue;
            }

            string? problem = CheckKind(property.Value, field.Kind);

            if (problem != null)
            {
                errors.Add($"{label}, поле «{field.Name}» ({property.Name}): {problem}");
            }
        }
    }

    // null — значення правильного вигляду; інакше — що потрібно
    private static string? CheckKind(JsonElement value, Kind kind)
    {
        switch (kind)
        {
            case Kind.Number:
                return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out _)
                    ? null : "потрібне ціле число без лапок, наприклад 15.";

            case Kind.Bool:
                return value.ValueKind is JsonValueKind.True or JsonValueKind.False
                    ? null : "потрібно true або false (без лапок).";

            case Kind.Text:
                return value.ValueKind == JsonValueKind.String ? null : "потрібен текст у лапках \"…\".";

            case Kind.TextOrLines:
                return value.ValueKind == JsonValueKind.String || IsListOf(value, JsonValueKind.String)
                    ? null : "потрібен текст у лапках або список рядків у квадратних дужках [ \"…\", \"…\" ].";

            case Kind.TextList:
                return IsListOf(value, JsonValueKind.String)
                    ? null : "потрібен список рядків у квадратних дужках, наприклад [\"int\", \"double\"].";

            case Kind.NumberList:
                return IsListOf(value, JsonValueKind.Number) ? null : "потрібен список чисел у квадратних дужках.";

            case Kind.Items:
                return CheckObjectList(value, ItemFields, "картка");

            case Kind.Rows:
                return CheckObjectList(value, RowFields, "рядок таблиці");

            default:
                return null;
        }
    }

    // Список об'єктів (карток або рядків таблиці): кожен у { } з відомими полями
    private static string? CheckObjectList(JsonElement value, Dictionary<string, Field> fields, string what)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            return $"потрібен список у квадратних дужках [ ], де кожна {what} записана у фігурних дужках {{ }}.";
        }

        int number = 0;

        foreach (JsonElement item in value.EnumerateArray())
        {
            number++;

            if (item.ValueKind != JsonValueKind.Object)
            {
                return $"{number}-а {what} має бути у фігурних дужках {{ }}.";
            }

            foreach (JsonProperty property in item.EnumerateObject())
            {
                if (!fields.TryGetValue(property.Name, out Field? field))
                {
                    return $"{number}-а {what}: невідоме поле «{property.Name}»{Suggestion(property.Name)}.";
                }

                string? problem = CheckKind(property.Value, field.Kind);

                if (problem != null)
                {
                    return $"{number}-а {what}, поле «{field.Name}» ({property.Name}): {problem}";
                }
            }
        }

        return null;
    }

    private static bool IsListOf(JsonElement value, JsonValueKind kind)
    {
        return value.ValueKind == JsonValueKind.Array && value.EnumerateArray().All(e => e.ValueKind == kind);
    }

    private static string UnknownField(string name, string label)
    {
        return $"{label}: невідоме поле «{name}»{Suggestion(name)}. Програма не знає такого поля — перевірте написання.";
    }

    // «, можливо, ви мали на увазі «title»» — для поля з одруківкою
    private static string Suggestion(string name)
    {
        string? best = AllFieldNames
            .Select(known => (known, distance: Distance(name.ToLowerInvariant(), known.ToLowerInvariant())))
            .Where(x => x.distance <= 2)
            .OrderBy(x => x.distance)
            .Select(x => x.known)
            .FirstOrDefault();

        return best != null ? $" (можливо, ви мали на увазі «{best}»?)" : "";
    }

    private static Field? Lookup(string name)
    {
        foreach (Dictionary<string, Field> fields in new[] { QuestionFields, SettingsFields, ItemFields, RowFields, RootFields })
        {
            if (fields.TryGetValue(name, out Field? field))
            {
                return field;
            }
        }

        return null;
    }

    // Відстань Левенштейна: скільки букв треба змінити, щоб одне слово стало іншим
    private static int Distance(string a, string b)
    {
        int[,] d = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
        {
            d[i, 0] = i;
        }

        for (int j = 0; j <= b.Length; j++)
        {
            d[0, j] = j;
        }

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        return d[a.Length, b.Length];
    }
}
