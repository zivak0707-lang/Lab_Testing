using System.Runtime.InteropServices;
using Lab_Testing;

namespace ScreenshotMaker;

// Самоперевірка справжнього Drag-and-Drop: курсор миші рухається системними подіями (як рука користувача),
// тож спрацьовує весь ланцюжок DoDragDrop -> DragEnter -> DragOver -> DragDrop.
// Запуск: dotnet run --project tools/ScreenshotMaker -- --drag-test  (курсор рухатиметься кілька секунд)
internal static class DragSelfTest
{
    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern void mouse_event(uint flags, int dx, int dy, uint data, UIntPtr extra);
    [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(Point point);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
    private const uint MOUSEEVENTF_LEFTUP = 0x04;

    public static async Task RunAsync(TestSession session)
    {
        Console.WriteLine("Самоперевірка Drag-and-Drop справжньою мишею:");
        GetCursorPos(out Point originalCursor);

        TestForm form = new(session) { TopMost = true };
        form.Show();
        await Task.Delay(500);

        try
        {
            // --- «Кошик»: картку зі слайда в порожній кошик, потім ще одну — прямо на картку в кошику
            form.ShowQuestion(0);
            await Task.Delay(300);
            QuestionView basketView = form.CurrentView;
            DropZone basket = basketView.GetZone("basket");
            CardLabel first = basketView.GetZone("slide").Cards[0];
            CardLabel second = basketView.GetZone("slide").Cards[1];

            await DragAsync(first, Center(basket));
            Expect(first.Parent == basket, "картка перенеслася в порожній кошик");

            await DragAsync(second, Center(first));
            Expect(second.Parent == basket, "картка, кинута на іншу картку в кошику, теж потрапила в кошик");

            await DragAsync(first, Center(basketView.GetZone("slide")));
            Expect(first.Parent == basketView.GetZone("slide"), "картку повернуто з кошика на слайд");

            // --- «Таблиця»: картку в клітинку, потім іншу картку на зайняту клітинку (стара повертається)
            form.ShowQuestion(1);
            await Task.Delay(300);
            QuestionView tableView = form.CurrentView;
            DropZone palette = tableView.GetZone("palette");
            DropZone cell = tableView.GetZone("row:0");
            CardLabel a = palette.Cards[0];
            CardLabel b = palette.Cards[1];

            await DragAsync(a, Center(cell));
            Expect(a.Parent == cell, "картка лягла в порожню клітинку таблиці");

            await DragAsync(b, Center(a));
            Expect(b.Parent == cell && a.Parent == palette, "нова картка замінила стару, стара повернулася до карток");
        }
        finally
        {
            SetCursorPos(originalCursor.X, originalCursor.Y);
            form.TopMost = false;
            await new Ui("").DialogShotAsync(form, () => Ui.Click(form, "btnFinish"), null, WindowCapture.IDYES);
        }
    }

    // Перетягує картку мишею в точку target (екранні координати)
    private static async Task DragAsync(CardLabel card, Point target)
    {
        Point start = Center(card);

        // Безпека: тиснемо кнопку, лише якщо під курсором саме наша картка
        if (WindowFromPoint(start) != card.Handle)
        {
            throw new InvalidOperationException("Під курсором не картка програми — перетягування скасовано");
        }

        // Рухи миші виконуються у фоновому потоці, а вікно тим часом обробляє перетягування
        await Task.Run(() =>
        {
            SetCursorPos(start.X, start.Y);
            Thread.Sleep(150);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(150);

            const int steps = 25;

            for (int i = 1; i <= steps; i++)
            {
                SetCursorPos(start.X + (target.X - start.X) * i / steps, start.Y + (target.Y - start.Y) * i / steps);
                Thread.Sleep(20);
            }

            Thread.Sleep(250);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        });

        await Task.Delay(400);
    }

    private static Point Center(Control control)
    {
        return control.PointToScreen(new Point(control.Width / 2, control.Height / 2));
    }

    private static void Expect(bool condition, string description)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Drag-and-Drop НЕ працює: " + description);
        }

        Console.WriteLine("  ✔ " + description);
    }
}
