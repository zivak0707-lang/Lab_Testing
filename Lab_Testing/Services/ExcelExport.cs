using ClosedXML.Excel;

namespace Lab_Testing;

// Експорт таблиці результатів у книгу Excel (.xlsx) бібліотекою ClosedXML (ліцензія MIT).
// На відміну від CSV, у .xlsx можна задати ширину стовпців, формат дат і чисел, кольори і фільтр,
// тож вчитель відкриває файл і одразу все бачить
public static class ExcelExport
{
    private static readonly XLColor HeaderBack = XLColor.FromArgb(232, 242, 252);
    private static readonly XLColor HeaderText = XLColor.FromArgb(31, 78, 121);
    private static readonly XLColor ModifiedBack = XLColor.FromArgb(252, 230, 228);
    private static readonly XLColor OtherKeyBack = XLColor.FromArgb(255, 238, 214);
    private static readonly XLColor DamagedBack = XLColor.FromArgb(238, 240, 243);

    private const double NoteWidth = 70;     // ширина стовпця «Примітка»
    private const double LineHeight = 15;    // висота рядка тексту (шрифт Calibri 11), пункти

    public static void Save(string path, IReadOnlyList<ResultEntry> entries, BankSettings settings)
    {
        using XLWorkbook book = new();
        WriteResults(book.Worksheets.Add("Результати"), entries, settings);
        WriteLegend(book.Worksheets.Add("Пояснення"), settings);
        book.SaveAs(path);
    }

    // ===================== Аркуш «Результати» =====================

    private static void WriteResults(IXLWorksheet sheet, IReadOnlyList<ResultEntry> entries, BankSettings settings)
    {
        List<string> header = new() { "№", "ПІБ", "Група", "Дата", "Час проходження", "Спроба" };
        header.AddRange(Enum.GetValues<QuestionType>().Select((t, i) => $"П{i + 1} {t.ToUkrainian()}"));
        header.AddRange(new[] { "Разом (з 100)", "%", "Оцінка", "Рівень", "Файл справжній", "Примітка", "Файл" });

        for (int c = 0; c < header.Count; c++)
        {
            sheet.Cell(1, c + 1).Value = header[c];
        }

        int noteColumn = header.IndexOf("Примітка") + 1;
        int statusColumn = header.IndexOf("Файл справжній") + 1;
        int row = 2;

        foreach (ResultEntry entry in entries)
        {
            TestResult? r = entry.Result;
            int col = 1;
            sheet.Cell(row, col++).Value = row - 1;

            if (r != null)
            {
                sheet.Cell(row, col++).Value = r.StudentName;
                // Група з цифр — числом (інакше Excel позначає «число збережено як текст»)
                sheet.Cell(row, col++).Value = int.TryParse(r.Group, out int groupNumber) && !r.Group.StartsWith('0')
                    ? groupNumber
                    : r.Group;
                sheet.Cell(row, col).Value = r.FinishedAt;
                sheet.Cell(row, col++).Style.NumberFormat.Format = "dd.mm.yyyy hh:mm";
                sheet.Cell(row, col).Value = r.Duration;
                sheet.Cell(row, col++).Style.NumberFormat.Format = "[h]:mm:ss";
                sheet.Cell(row, col++).Value = r.AttemptNumber;

                for (int i = 0; i < 5; i++)
                {
                    if (i < r.Questions.Count)
                    {
                        sheet.Cell(row, col).Value = r.Questions[i].Score;
                    }

                    sheet.Cell(row, col++).Style.NumberFormat.Format = "0.0";
                }

                sheet.Cell(row, col).Value = r.TotalScore;
                sheet.Cell(row, col++).Style.NumberFormat.Format = "0.0";
                sheet.Cell(row, col).Value = r.Percent;
                sheet.Cell(row, col++).Style.NumberFormat.Format = "0.0";
                sheet.Cell(row, col).Value = r.Grade;
                sheet.Cell(row, col++).Style.Font.Bold = true;
                sheet.Cell(row, col++).Value = Grading.Level(r.Grade);
            }

            sheet.Cell(row, statusColumn).Value = ResultChecks.StatusText(entry.Status);
            sheet.Cell(row, noteColumn).Value = ResultChecks.Note(entry, settings);
            sheet.Cell(row, noteColumn + 1).Value = Path.GetFileName(entry.FilePath);
            ColorRow(sheet.Range(row, 1, row, header.Count), sheet.Cell(row, statusColumn), entry.Status);
            row++;
        }

        // Заголовки: жирні, з заливкою і переносом; рядок заголовків закріплено, на ньому — фільтр
        IXLRange headerRange = sheet.Range(1, 1, 1, header.Count);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = HeaderText;
        headerRange.Style.Fill.BackgroundColor = HeaderBack;
        headerRange.Style.Alignment.WrapText = true;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        sheet.Row(1).Height = 48;   // до трьох рядків заголовка
        sheet.SheetView.FreezeRows(1);
        sheet.Range(1, 1, Math.Max(row - 1, 1), header.Count).SetAutoFilter();

        // Ширина стовпців — під дані (довгі заголовки переносяться на другий рядок); примітка — з переносом рядків
        sheet.Columns(1, header.Count).AdjustToContents(2, Math.Max(row - 1, 2), 8, 40);
        sheet.Columns(7, 11).Width = 17;            // П1–П5: «відповідностей» — без переносу посеред слова
        sheet.Column(5).Width = Math.Max(sheet.Column(5).Width, 17);   // «проходження» — так само
        sheet.Column(1).Width = 5;
        sheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;   // група — як текст
        sheet.Column(statusColumn).Width += 3;      // стан написано жирним — потрібно трохи більше місця
        sheet.Column(noteColumn).Width = NoteWidth;
        sheet.Column(noteColumn).Style.Alignment.WrapText = true;
        sheet.Range(2, 1, Math.Max(row - 1, 2), header.Count).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

        // Висоту рядків задаємо самі: Excel підбирає її під час відкриття не завжди точно,
        // і текст на межі ширини обрізається
        for (int r = 2; r < row; r++)
        {
            FitRowHeight(sheet.Row(r), sheet.Cell(r, noteColumn).GetString(), NoteWidth);
        }

        // Друк: альбомна сторінка, уся таблиця по ширині на одному аркуші
        sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        sheet.PageSetup.FitToPages(1, 0);
    }

    // Висота рядка під текст з переносом: рахуємо рядки за словами, з запасом по ширині
    // (ширина стовпця в Excel — це кількість цифр «0», а літери в середньому ширші)
    private static void FitRowHeight(IXLRow row, string text, double columnWidth)
    {
        int lineLength = (int)(columnWidth * 0.8);
        int lines = 0;

        foreach (string paragraph in text.Split('\n'))
        {
            int current = 0;
            lines++;

            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (current > 0 && current + 1 + word.Length > lineLength)
                {
                    lines++;
                    current = 0;
                }

                current += (current > 0 ? 1 : 0) + word.Length;
            }
        }

        row.Height = Math.Max(1, lines) * LineHeight + 3;
    }

    // Справжній — зелений стан; не вашим ключем — жовтий рядок; змінений — червоний; пошкоджений — сірий
    private static void ColorRow(IXLRange row, IXLCell status, ResultFileStatus state)
    {
        status.Style.Font.Bold = true;

        switch (state)
        {
            case ResultFileStatus.Valid:
                status.Style.Font.FontColor = XLColor.FromArgb(46, 125, 50);
                break;

            case ResultFileStatus.DefaultKey:
            case ResultFileStatus.UnknownKey:
                row.Style.Fill.BackgroundColor = OtherKeyBack;
                status.Style.Font.FontColor = XLColor.FromArgb(191, 96, 0);
                break;

            case ResultFileStatus.Modified:
                row.Style.Fill.BackgroundColor = ModifiedBack;
                status.Style.Font.FontColor = XLColor.FromArgb(198, 40, 40);
                break;

            default:
                row.Style.Fill.BackgroundColor = DamagedBack;
                break;
        }
    }

    // ===================== Аркуш «Пояснення» =====================

    private static void WriteLegend(IXLWorksheet sheet, BankSettings settings)
    {
        string scale = string.Join(", ", settings.GradeMinPercent.Select((p, i) => $"{i + 1} — від {Grading.Format(p)} %"));
        TeacherKey.KeyInfo? key = TeacherKey.Current;

        (string Name, string Text)[] lines =
        {
            ("Експортовано", DateTime.Now.ToString("dd.MM.yyyy HH:mm")),
            ("П1–П5", string.Join("; ", Enum.GetValues<QuestionType>().Select((t, i) => $"П{i + 1} — {t.ToUkrainian()}")) +
                      ". За кожне питання — до 20 балів, разом до 100."),
            ("Шкала оцінок", scale),
            ("✔ Справжній", "Файл підписано вашим ключем і після збереження не змінювався."),
            ("⚠ Загальний ключ", "Учень проходив тест без вашого банку (questions.dat) — справжність не гарантовано."),
            ("⚠ Інший ключ", "Файл підписано ключем, якого немає на цьому комп'ютері (інший банк або підробка)."),
            ("✗ Змінено!", "Файл змінено після збереження; у стовпці «Примітка» — що саме не сходиться."),
            ("⚠ Пошкоджено", "Файл не вдалося прочитати."),
            ("Ключ вчителя", key != null ? $"№ {key.Id}" : "ще не створено")
        };

        sheet.Cell(1, 1).Value = "Пояснення до таблиці результатів";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;

        for (int i = 0; i < lines.Length; i++)
        {
            sheet.Cell(i + 3, 1).Value = lines[i].Name;
            sheet.Cell(i + 3, 1).Style.Font.Bold = true;
            sheet.Cell(i + 3, 2).Value = lines[i].Text;
            FitRowHeight(sheet.Row(i + 3), lines[i].Text, 100);
        }

        sheet.Column(1).Width = 20;
        sheet.Column(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        sheet.Column(2).Width = 100;
        sheet.Column(2).Style.Alignment.WrapText = true;
    }
}
