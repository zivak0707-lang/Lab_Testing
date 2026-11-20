using System.Globalization;
using System.Text;

namespace Lab_Testing;

// Правила для ПІБ і групи: як порівнювати (щоб «Зiвак» з латинською «i» не ставав «новою людиною»)
// і що дозволено вводити (ПІБ — лише кирилицею)
public static class NameRules
{
    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");

    // Латинські літери, які на екрані не відрізнити від кириличних
    private static readonly Dictionary<char, char> Lookalikes = new()
    {
        ['A'] = 'А', ['B'] = 'В', ['C'] = 'С', ['E'] = 'Е', ['H'] = 'Н', ['I'] = 'І', ['K'] = 'К', ['M'] = 'М',
        ['O'] = 'О', ['P'] = 'Р', ['T'] = 'Т', ['X'] = 'Х', ['Y'] = 'У',
        ['a'] = 'а', ['c'] = 'с', ['e'] = 'е', ['i'] = 'і', ['o'] = 'о', ['p'] = 'р', ['x'] = 'х', ['y'] = 'у', ['k'] = 'к',
        ['ï'] = 'ї', ['Ï'] = 'Ї'
    };

    private const string Apostrophes = "'’ʼ‘`´′";   // різні апострофи — це той самий апостроф
    private const string Dashes = "‐‑‒–—";          // різні дефіси й тире — це дефіс

    // Пробіли: обрізати по краях, кілька підряд — один
    public static string NormalizeSpaces(string text)
    {
        return string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    // Ключ ПІБ для порівняння: однаковий для «Зівак Сергій», «Зiвак  сергій», «Сергій Зівак», «Мар'яна» / «Марʼяна»
    public static string Key(string name)
    {
        string[] words = Canonical(name).ToLower(Ukrainian).Replace('ґ', 'г').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Array.Sort(words, StringComparer.Ordinal);   // порядок слів не важливий
        return string.Join(' ', words);
    }

    // Ключ групи: без пробілів, великими літерами, латинські двійники — кирилицею («ІПЗ-21» = «IПЗ - 21»)
    public static string GroupKey(string group)
    {
        return Canonical(group).Replace(" ", "").ToUpper(Ukrainian).Replace('Ґ', 'Г');
    }

    // Латинські двійники замінено кириличними літерами (решта тексту — як є)
    public static string ReplaceLookalikes(string text)
    {
        return string.Concat(text.Select(c => Lookalikes.TryGetValue(c, out char cyrillic) ? cyrillic : c));
    }

    // Чи є в тексті латинські літери
    public static bool HasLatin(string text)
    {
        return text.Any(IsLatin);
    }

    // Що не так з ПІБ, яке вводить учень (null — усе гаразд)
    public static string? InputProblem(string name)
    {
        foreach (string word in NormalizeSpaces(name).Split(' '))
        {
            string latin = string.Concat(word.Where(IsLatin).Distinct());

            if (latin.Length > 0)
            {
                string letters = string.Join(", ", latin.Select(c => $"«{c}»"));
                return "Пишіть прізвище, ім'я та по батькові українською — кирилицею.\n\n" +
                       $"У слові «{word}» є латинські літери: {letters}. Так буває, коли ввімкнено англійську " +
                       "розкладку: літери «і», «а», «о», «е», «с», «р», «х» виглядають однаково, але це різні символи. " +
                       "Перемкніть мову на українську і введіть це слово ще раз.";
            }
        }

        char? wrong = name.Select(c => (char?)c).FirstOrDefault(c => !IsAllowedInName(c!.Value));

        return wrong != null
            ? $"У прізвищі, імені чи по батькові не може бути символу «{wrong}». Можна лише українські літери, " +
              "пробіл, дефіс (подвійне прізвище), апостроф і крапку (ініціали)."
            : null;
    }

    // Що не так з назвою групи (null — усе гаразд). Цифри з однією абеткою — можна
    public static string? GroupProblem(string group)
    {
        bool cyrillic = group.Any(IsCyrillic);
        bool latin = group.Any(IsLatin);

        return cyrillic && latin
            ? "У назві групи змішано українські й латинські літери (наприклад, латинська «I» замість «І»). " +
              "Введіть назву групи українською розкладкою."
            : null;
    }

    // Латинські літери в ПІБ — для примітки вчителю: «e» у «Пeтренко»
    public static string LatinHint(string name)
    {
        return string.Join(", ", NormalizeSpaces(name).Split(' ')
            .Where(HasLatin)
            .Select(w => $"{string.Join(", ", w.Where(IsLatin).Distinct().Select(c => $"«{c}»"))} у «{w}»"));
    }

    // «Схожий ПІБ» (лише підказка, не та сама людина): одне — частина іншого (без по батькові)
    // або вони відрізняються однією літерою
    public static bool Similar(string key1, string key2)
    {
        if (key1 == key2)
        {
            return false;
        }

        string[] words1 = key1.Split(' ');
        string[] words2 = key2.Split(' ');
        bool part = words1.Length >= 2 && words2.Length >= 2 &&
                    (words1.All(words2.Contains) || words2.All(words1.Contains));

        return part || (Math.Min(key1.Length, key2.Length) >= 8 && TextDistance.Levenshtein(key1, key2) == 1);
    }

    // Спільне для ПІБ і групи: пробіли, латинські двійники, апострофи, дефіси
    private static string Canonical(string text)
    {
        StringBuilder sb = new();

        foreach (char c in NormalizeSpaces(text))
        {
            if (Lookalikes.TryGetValue(c, out char cyrillic))
            {
                sb.Append(cyrillic);
            }
            else if (Apostrophes.Contains(c))
            {
                sb.Append('\'');
            }
            else if (Dashes.Contains(c))
            {
                sb.Append('-');
            }
            else
            {
                sb.Append(c);
            }
        }

        // «Петренко - Коваль» = «Петренко-Коваль»
        return sb.ToString().Replace(" - ", "-").Replace(" -", "-").Replace("- ", "-");
    }

    private static bool IsLatin(char c)
    {
        return c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') || (c >= 'À' && c <= 'ɏ' && char.IsLetter(c));
    }

    private static bool IsCyrillic(char c)
    {
        return c >= 'Ѐ' && c <= 'ӿ';
    }

    private static bool IsAllowedInName(char c)
    {
        return IsCyrillic(c) || c == ' ' || c == '-' || c == '.' || Apostrophes.Contains(c) || Dashes.Contains(c);
    }
}
