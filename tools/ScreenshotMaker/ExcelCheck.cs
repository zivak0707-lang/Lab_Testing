using ClosedXML.Excel;
using Lab_Testing;

namespace ScreenshotMaker;

// Самоперевірка експорту в Excel і звірки даних у змінених файлах
internal static class ExcelCheck
{
    public static void Run(IReadOnlyList<ResultEntry> entries, BankSettings settings)
    {
        CheckMismatches(entries, settings);

        string path = Path.Combine(Path.GetTempPath(), "Lab_Testing_зразки", "Результати.xlsx");
        ExcelExport.Save(path, entries, settings);

        using XLWorkbook book = new(path);
        IXLWorksheet sheet = book.Worksheet("Результати");
        IXLRow header = sheet.Row(1);
        int columns = header.LastCellUsed()!.Address.ColumnNumber;

        int Column(string name) => header.Cells(1, columns).First(c => c.GetString() == name).Address.ColumnNumber;

        int dateColumn = Column("Дата");
        int noteColumn = Column("Примітка");
        int statusColumn = Column("Файл справжній");

        Expect(sheet.LastRowUsed()!.RowNumber() == entries.Count + 1, "по рядку на кожен файл");
        Expect(book.Worksheets.Contains("Пояснення"), "аркуш «Пояснення»");
        Expect(sheet.SheetView.SplitRow == 1, "закріплений рядок заголовків");
        Expect(sheet.AutoFilter.IsEnabled, "фільтр на заголовках");
        Expect(sheet.Column(dateColumn).Width >= 15, "ширина стовпця дати (без ########)");

        IXLCell date = sheet.Cell(2, dateColumn);
        Expect(date.DataType == XLDataType.DateTime && date.Style.NumberFormat.Format.Contains("yyyy"), "дата — справжня дата Excel");

        // Рядок зміненого файлу: червоний, у примітці — що саме не сходиться
        IXLRow modified = sheet.RowsUsed().First(r => r.Cell(statusColumn).GetString() == ResultChecks.StatusText(ResultFileStatus.Modified));
        Expect(modified.Cell(1).Style.Fill.BackgroundColor.Color.ToArgb() == System.Drawing.Color.FromArgb(252, 230, 228).ToArgb(),
            "змінений файл — червоний рядок");
        Expect(modified.Cell(noteColumn).GetString().Contains("оцінка у файлі 12"), "примітка про оцінку у зміненому файлі");

        IXLRow defaultKey = sheet.RowsUsed().First(r => r.Cell(statusColumn).GetString() == ResultChecks.StatusText(ResultFileStatus.DefaultKey));
        Expect(defaultKey.Cell(noteColumn).GetString().Contains("без вашого банку"), "примітка для загального ключа");

        Console.WriteLine("  Excel (.xlsx): дати, ширина стовпців, закріплені заголовки, фільтр, кольори і примітки — правильно");
    }

    // Звірка: змінена оцінка і змінена сума помітні; у справжніх файлах нічого не «сходиться не так»
    private static void CheckMismatches(IReadOnlyList<ResultEntry> entries, BankSettings settings)
    {
        foreach (ResultEntry entry in entries.Where(e => e.Status == ResultFileStatus.Valid))
        {
            Expect(ResultChecks.FindMismatches(entry.Result!, settings).Count == 0, "у справжньому файлі все сходиться");
        }

        TestResult changed = entries.First(e => e.Status == ResultFileStatus.Valid).Result!;
        double realTotal = changed.TotalScore;
        int realGrade = changed.Grade;

        changed.TotalScore = realTotal + 10;
        Expect(ResultChecks.FindMismatches(changed, settings).Any(m => m.Contains("сума балів")), "змінена сума");

        changed.TotalScore = realTotal;
        changed.Grade = realGrade == 12 ? 1 : 12;
        Expect(ResultChecks.FindMismatches(changed, settings).Any(m => m.Contains("оцінка у файлі")), "змінена оцінка");
        changed.Grade = realGrade;
    }

    private static void Expect(bool condition, string what)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Експорт у Excel або звірка даних: не виконано — " + what);
        }
    }
}
