using System.Text;

namespace ScreenshotMaker;

// Один знімок: номер рисунка, файл, підпис, етап; Manual != null — знімок робиться вручну (інструкція)
internal record Shot(int Number, string File, string Caption, int Stage, string? Manual = null);

// Перелік усіх знімків для звіту. Номери сталі: нові знімки лише додаються в кінець
internal static class ShotList
{
    // Останній реалізований етап
    public const int CurrentStage = 1;

    public static readonly List<Shot> All = new()
    {
        // ---------- Етап 1 ----------
        new(1, "01_start_training.png", "Стартова сторінка програми (обрано режим «Навчання»)", 1),
        new(2, "02_start_control.png", "Стартова сторінка програми (обрано режим «Контрольна»)", 1),
        new(3, "03_msg_name_required.png", "Повідомлення про незаповнене поле «Прізвище, ім'я, по батькові»", 1),
        new(4, "04_about.png", "Вікно «Про програму»", 1),
        new(5, "05_designer_startform.png", "Форма StartForm у конструкторі Visual Studio", 1,
            "відкрийте Forms\\StartForm.cs у Visual Studio (Shift+F7 — конструктор) і зніміть вікно VS з формою (Alt+PrtScr)"),
        new(6, "06_solution_explorer.png", "Структура проєкту Lab_Testing у вікні Solution Explorer", 1,
            "розгорніть у Solution Explorer теки Forms, Model, Data і зніміть цю панель"),
    };

    public static Shot Get(int number)
    {
        return All.First(s => s.Number == number);
    }

    // Записує docs/screenshots/README.md — список рисунків для звіту
    public static void WriteReadme(string outDir, int maxStage)
    {
        StringBuilder sb = new();
        sb.AppendLine("# Знімки екрана для звіту");
        sb.AppendLine();
        sb.AppendLine("Автоматичні знімки робить утиліта `tools/ScreenshotMaker` (з кореня репозиторію:");
        sb.AppendLine("`dotnet run --project tools/ScreenshotMaker`). Вона відкриває справжні вікна програми і знімає");
        sb.AppendLine("кожне вікно окремо (без робочого столу).");
        sb.AppendLine();
        sb.AppendLine("Рядки з позначкою **✋ вручну** — місця під ваші власні знімки: зробіть знімок за інструкцією");
        sb.AppendLine("і збережіть його в цю теку під указаною назвою файлу.");
        sb.AppendLine();
        sb.AppendLine("| № | Файл | Підпис до рисунка | Етап | Як знято |");
        sb.AppendLine("|---|------|-------------------|------|----------|");

        foreach (Shot shot in All.Where(s => s.Stage <= maxStage))
        {
            string how = "авто";

            if (shot.Manual != null)
            {
                bool exists = File.Exists(Path.Combine(outDir, shot.File));
                how = $"**✋ вручну**: {shot.Manual}" + (exists ? " — ✔ додано" : " — *ще не додано*");
            }

            sb.AppendLine($"| {shot.Number} | `{shot.File}` | Рисунок {shot.Number} – {shot.Caption} | {shot.Stage} | {how} |");
        }

        File.WriteAllText(Path.Combine(outDir, "README.md"), sb.ToString(), new UTF8Encoding(false));
    }
}
