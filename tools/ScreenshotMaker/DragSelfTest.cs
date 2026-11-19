using System.Runtime.InteropServices;
using Lab_Testing;

namespace ScreenshotMaker;

// Самоперевірка справжнього Drag-and-Drop: курсор миші рухається системними подіями (як рука користувача),
// тож спрацьовує весь ланцюжок DoDragDrop -> DragEnter -> DragOver -> DragDrop.
// Заодно робить знімки 11 і 19 — вікно посеред перетягування разом із курсором.
// Запуск: dotnet run --project tools/ScreenshotMaker -- --drag-test  (курсор рухатиметься кілька секунд)
internal static class DragSelfTest
{
    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern void mouse_event(uint flags, int dx, int dy, uint data, UIntPtr extra);
    [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(Point point);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
    private const uint MOUSEEVENTF_LEFTUP = 0x04;

    public static async Task RunAsync(TestSession session, Ui ui)
    {
        Console.WriteLine("Самоперевірка Drag-and-Drop справжньою мишею:");
        GetCursorPos(out Point originalCursor);

        TestForm form = new(session) { TopMost = true };
        form.Show();
        await Task.Delay(500);

        try
        {
            // --- «Кошик»: зайву картку в порожній кошик (знімок 11 посеред перетягування),
            //     потім ще одну — прямо на картку в кошику, і одну назад на слайд
            form.ShowQuestion(0);
            await Task.Delay(300);
            QuestionView basketView = form.CurrentView;
            DropZone basket = basketView.GetZone("basket");
            DropZone slide = basketView.GetZone("slide");
            string extraText = basketView.Question.Items.First(i => !i.Keep).Text;
            CardLabel first = slide.Cards.First(c => c.Value == extraText);
            CardLabel second = slide.Cards.First(c => c != first);

            await DragAsync(first, Center(basket), form, ui, shotNumber: 11);
            Expect(first.Parent == basket, "картка перенеслася в порожній кошик");

            await DragAsync(second, Center(first));
            Expect(second.Parent == basket, "картка, кинута на іншу картку в кошику, теж потрапила в кошик");

            await DragAsync(second, Center(slide));
            Expect(second.Parent == slide, "картку повернуто з кошика на слайд");

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

            // --- «Номерки»: два кружечки в один рядок категорії
            form.ShowQuestion(2);
            await Task.Delay(300);
            QuestionView catView = form.CurrentView;
            DropZone row = catView.GetZone("cat:0");
            CardLabel c1 = catView.GetZone("palette").Cards[0];
            CardLabel c2 = catView.GetZone("palette").Cards[1];

            await DragAsync(c1, Center(row));
            await DragAsync(c2, Center(c1));
            Expect(c1.Parent == row && c2.Parent == row, "два номерки лежать в одному рядку категорії");

            // --- «Пропуски»: слово, яке в тексті потрібне двічі, копіюємо (Copy) в обидва пропуски
            //     (знімок 19 — під час другого копіювання), потім переміщення і видалення
            form.ShowQuestion(3);
            await Task.Delay(300);
            QuestionView gapsView = form.CurrentView;
            DropZone words = gapsView.GetZone("palette");
            List<string> answers = gapsView.Question.GetGapAnswers();
            string repeated = answers.GroupBy(w => w).First(g => g.Count() > 1).Key;
            int firstIndex = answers.IndexOf(repeated);
            int secondIndex = answers.LastIndexOf(repeated);
            DropZone gapA = gapsView.GetZone("gap:" + firstIndex);
            DropZone gapB = gapsView.GetZone("gap:" + secondIndex);
            CardLabel template = words.Cards.First(c => c.Value == repeated);
            int wordCount = words.Cards.Count;

            await DragAsync(template, Center(gapA));
            Expect(gapA.Cards.Count == 1 && gapA.Cards[0].Value == repeated && words.Cards.Count == wordCount,
                "слово скопійовано в пропуск (Copy), у палітрі воно лишилося");

            await DragAsync(template, Center(gapB), form, ui, shotNumber: 19);
            Expect(gapB.Cards.Count == 1 && gapB.Cards[0].Value == repeated, "те саме слово вставлено в другий пропуск");

            CardLabel copy = gapB.Cards[0];
            await DragAsync(copy, Center(words));
            Expect(gapB.Cards.Count == 0 && !gapsView.Cards.Contains(copy), "слово перетягнуто з пропуску в палітру — пропуск очистився");

            CardLabel moved = gapA.Cards[0];
            await DragAsync(moved, Center(gapB));
            Expect(gapA.Cards.Count == 0 && moved.Parent == gapB, "слово переміщено з одного пропуску в інший (Move)");

            // --- «Порядок кроків»: заміна і обмін місцями
            form.ShowQuestion(4);
            await Task.Delay(300);
            QuestionView orderView = form.CurrentView;
            DropZone steps = orderView.GetZone("palette");
            DropZone slot0 = orderView.GetZone("slot:0");
            DropZone slot1 = orderView.GetZone("slot:1");
            CardLabel s1 = steps.Cards[0];
            CardLabel s2 = steps.Cards[1];

            await DragAsync(s1, Center(slot0));
            await DragAsync(s2, Center(s1));
            Expect(s2.Parent == slot0 && s1.Parent == steps, "крок зі списку замінив крок у клітинці, старий повернувся в список");

            await DragAsync(s1, Center(slot1));
            await DragAsync(s1, Center(s2));
            Expect(s1.Parent == slot0 && s2.Parent == slot1, "кроки в двох клітинках помінялися місцями");
        }
        finally
        {
            SetCursorPos(originalCursor.X, originalCursor.Y);
            form.TopMost = false;
            await ui.MessageShotAsync(form, () => Ui.Click(form, "btnFinish"), null, Answer.Yes);
        }
    }

    // Перетягує картку мишею в точку target (екранні координати).
    // Якщо задано shotNumber — перед тим як відпустити кнопку, знімає вікно разом із курсором
    private static async Task DragAsync(CardLabel card, Point target, Form? form = null, Ui? ui = null, int? shotNumber = null)
    {
        Point start = Center(card);
        IntPtr formHandle = form?.Handle ?? IntPtr.Zero;

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

            // Кнопку ще не відпущено: зона підсвічена, курсор показує ефект Move або Copy
            if (shotNumber != null && ui != null)
            {
                Thread.Sleep(300);
                using Bitmap bmp = WindowCapture.CaptureWithCursor(formHandle);
                ui.SaveShot(bmp, shotNumber.Value);
            }

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
