namespace Lab_Testing;

// Номер спроби у вчителя і примітки до неї
public record AttemptInfo(int Number, int Total, List<string> Notes);

// Повторні спроби — з погляду вчителя, за файлами, які він відкрив.
// Лічильник на комп'ютері учня легко обійти (видалити %AppData% чи перейти на інший комп'ютер),
// а тут файли одного учня групуються за ПІБ і групою (див. NameRules), незалежно від того лічильника
public static class AttemptGroups
{
    public static Dictionary<ResultEntry, AttemptInfo> Analyze(IEnumerable<ResultEntry> entries)
    {
        Dictionary<ResultEntry, AttemptInfo> result = new();
        List<ResultEntry> controls = entries.Where(e => e.Result is { Mode: TestMode.Control }).ToList();
        List<ResultEntry> unique = new();
        Dictionary<ResultEntry, string> borrowed = new();

        // Однаковий номер спроби (AttemptId) у кількох файлах:
        //  - той самий учень — копія того самого файлу (можливо, зміненого), окремою спробою не рахується;
        //  - різні учні — один із файлів, схоже, зроблено з чужого результату (рахуються, але з приміткою)
        foreach (IGrouping<Guid, ResultEntry> sameAttempt in controls.GroupBy(e => e.Result!.AttemptId))
        {
            List<IGrouping<string, ResultEntry>> byName = sameAttempt.GroupBy(e => NameRules.Key(e.Result!.StudentName)).ToList();

            foreach (IGrouping<string, ResultEntry> sameName in byName)
            {
                ResultEntry first = sameName.First();
                unique.Add(first);

                foreach (ResultEntry copy in sameName.Skip(1))
                {
                    result[copy] = new AttemptInfo(0, 0, new List<string>
                    {
                        $"копія файлу «{Path.GetFileName(first.FilePath)}» — окремою спробою не рахується"
                    });
                }

                if (byName.Count > 1)
                {
                    string others = string.Join(", ", byName.Where(g => g != sameName).Select(g => $"«{g.First().Result!.StudentName}»"));
                    borrowed[first] = $"той самий номер спроби, що й у файлі {others} — схоже, один файл зроблено з чужого результату";
                }
            }
        }

        var people = unique
            .GroupBy(e => (Name: NameRules.Key(e.Result!.StudentName), Group: NameRules.GroupKey(e.Result!.Group)))
            .ToList();

        foreach (var person in people)
        {
            List<ResultEntry> attempts = person.OrderBy(e => e.Result!.FinishedAt).ThenBy(e => e.Result!.StartedAt).ToList();

            // Різне написання, крім латинських двійників (про них — окрема примітка)
            List<string> spellings = attempts.Select(e => NameRules.NormalizeSpaces(e.Result!.StudentName)).Distinct().ToList();
            bool spelledDifferently = spellings.Select(NameRules.ReplaceLookalikes).Distinct().Count() > 1;

            // Той самий ПІБ в іншій групі і схожі ПІБ у тій самій групі — підказки вчителю
            List<string> otherGroups = people
                .Where(p => p.Key.Name == person.Key.Name && p.Key.Group != person.Key.Group)
                .Select(p => p.First().Result!.Group).Distinct().ToList();
            List<string> similar = people
                .Where(p => p.Key.Group == person.Key.Group && NameRules.Similar(p.Key.Name, person.Key.Name))
                .Select(p => p.First().Result!.StudentName).Distinct().ToList();

            for (int i = 0; i < attempts.Count; i++)
            {
                TestResult r = attempts[i].Result!;
                List<string> notes = new();

                if (attempts.Count > 1)
                {
                    notes.Add(i == 0
                        ? $"перша з {attempts.Count} спроб цього учня"
                        : $"повторна спроба: {i + 1}-а з {attempts.Count} цього учня");
                }

                if (r.AttemptNumber != i + 1)
                {
                    notes.Add($"у файлі — спроба №{r.AttemptNumber} (лічильник на ПК учня)");
                }

                if (spelledDifferently)
                {
                    notes.Add("ПІБ записано по-різному: " + string.Join(" і ", spellings.Select(s => $"«{s}»")));
                }

                if (NameRules.HasLatin(r.StudentName))
                {
                    notes.Add("латинські літери в ПІБ: " + NameRules.LatinHint(r.StudentName));
                }

                if (otherGroups.Count > 0)
                {
                    notes.Add("такий самий ПІБ — у групі " + string.Join(", ", otherGroups.Select(g => $"«{g}»")));
                }

                if (similar.Count > 0)
                {
                    notes.Add("схожий ПІБ: " + string.Join(", ", similar.Select(s => $"«{s}»")));
                }

                if (borrowed.TryGetValue(attempts[i], out string? borrowedNote))
                {
                    notes.Add(borrowedNote);
                }

                result[attempts[i]] = new AttemptInfo(i + 1, attempts.Count, notes);
            }
        }

        return result;
    }

    // Текст приміток для таблиці: «Повторна спроба: 2-а з 2 цього учня; у файлі — спроба №1 (лічильник на ПК учня).»
    public static string Note(AttemptInfo? info)
    {
        return info == null || info.Notes.Count == 0 ? "" : Capitalize(string.Join("; ", info.Notes)) + ".";
    }

    private static string Capitalize(string text)
    {
        return text.Length == 0 ? text : char.ToUpper(text[0]) + text[1..];
    }
}
