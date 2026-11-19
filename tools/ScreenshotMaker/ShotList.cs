using System.Text;

namespace ScreenshotMaker;

// Один знімок: номер рисунка, файл, підпис, етап; Manual != null — знімок робиться вручну (інструкція)
internal record Shot(int Number, string File, string Caption, int Stage, string? Manual = null, bool DragTest = false);

// Перелік усіх знімків для звіту. Номери сталі: нові знімки лише додаються в кінець
internal static class ShotList
{
    // Останній реалізований етап
    public const int CurrentStage = 3;

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

        // ---------- Етап 2 ----------
        new(7, "07_basket_empty.png", "Питання типу «Кошик» до відповіді", 2),
        new(8, "08_basket_done.png", "Питання типу «Кошик»: зайві картки перенесено в кошик", 2),
        new(9, "09_table_empty.png", "Питання типу «Таблиця відповідностей» до відповіді", 2),
        new(10, "10_table_done.png", "Питання типу «Таблиця відповідностей» з розставленими відповідями", 2),
        new(11, "11_drag_in_progress.png", "Перетягування картки в кошик: зона під курсором підсвічена, курсор показує переміщення (Move)", 2, DragTest: true),
        new(12, "12_designer_testform.png", "Форма TestForm у конструкторі Visual Studio", 2,
            "відкрийте Forms\\TestForm.cs у конструкторі (Shift+F7) і зніміть вікно VS"),

        // ---------- Етап 3 ----------
        new(13, "13_categories_empty.png", "Питання типу «Номерки на категорії» до відповіді", 3),
        new(14, "14_categories_done.png", "Питання типу «Номерки на категорії»: номерки розкладено по рядках категорій", 3),
        new(15, "15_gaps_empty.png", "Питання типу «Слова в пропуски» до відповіді", 3),
        new(16, "16_gaps_done.png", "Питання типу «Слова в пропуски»: слова скопійовано в пропуски (break — двічі)", 3),
        new(17, "17_order_empty.png", "Питання типу «Порядок кроків» до відповіді", 3),
        new(18, "18_order_done.png", "Питання типу «Порядок кроків»: кроки алгоритму розставлено по порядку", 3),
        new(19, "19_drag_copy.png", "Копіювання слова в пропуск: пропуск підсвічений, курсор зі знаком «+» (ефект Copy)", 3, DragTest: true),
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
        sb.AppendLine("кожне вікно окремо (без робочого столу). Картки в знімках розкладаються тим самим методом `PlaceCard`,");
        sb.AppendLine("який викликає обробник `DragDrop`. Справжнє перетягування мишею перевіряє окремий запуск");
        sb.AppendLine("`dotnet run --project tools/ScreenshotMaker -- --drag-test` (курсор рухатиметься кілька секунд).");
        sb.AppendLine();
        sb.AppendLine("Рядки з позначкою **✋ вручну** — місця під знімки, які автор робить сам за інструкцією");
        sb.AppendLine("і додає до звіту окремо (у цій теці їх немає).");
        sb.AppendLine();
        sb.AppendLine("| № | Файл | Підпис до рисунка | Етап | Як знято |");
        sb.AppendLine("|---|------|-------------------|------|----------|");

        foreach (Shot shot in All.Where(s => s.Stage <= maxStage))
        {
            string how = shot.Manual != null ? $"**✋ вручну**: {shot.Manual}" : shot.DragTest ? "авто (`--drag-test`, справжня миша)" : "авто";

            sb.AppendLine($"| {shot.Number} | `{shot.File}` | Рисунок {shot.Number} – {shot.Caption} | {shot.Stage} | {how} |");
        }

        File.WriteAllText(Path.Combine(outDir, "README.md"), sb.ToString(), new UTF8Encoding(false));
    }
}
