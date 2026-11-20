using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Lab_Testing;

namespace ScreenshotMaker;

// Картинки для README (docs/images): іконка, п'ять типів завдань і анімація справжнього перетягування.
// Запуск: dotnet run --project tools/ScreenshotMaker -- --readme-media  (курсор рухатиметься ~15 секунд)
internal static class ReadmeMedia
{
    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")] private static extern void mouse_event(uint flags, int dx, int dy, uint data, UIntPtr extra);
    [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(Point point);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
    private const uint MOUSEEVENTF_LEFTUP = 0x04;

    // Область питання у знімку вікна тесту (без шапки з ПІБ, панелі теорії і нижніх кнопок)
    private static readonly Rectangle QuestionArea = new(1, 88, 820, 559);

    public static async Task RunAsync(string repoRoot)
    {
        string images = Path.Combine(repoRoot, "docs", "images");
        string shots = Path.Combine(repoRoot, "docs", "screenshots");
        Directory.CreateDirectory(images);

        SaveIcon(Path.Combine(repoRoot, "Lab_Testing", "app.ico"), Path.Combine(images, "icon.png"));

        (string Shot, string Name)[] types =
        {
            ("08_basket_done.png", "type-basket.png"),
            ("10_table_done.png", "type-table.png"),
            ("14_categories_done.png", "type-categories.png"),
            ("16_gaps_done.png", "type-gaps.png"),
            ("18_order_done.png", "type-order.png")
        };

        foreach ((string shot, string name) in types)
        {
            using Bitmap full = new(Path.Combine(shots, shot));
            using Bitmap part = full.Clone(QuestionArea, PixelFormat.Format24bppRgb);
            part.Save(Path.Combine(images, name), ImageFormat.Png);
            Console.WriteLine($"  {name} ({part.Width}×{part.Height})");
        }

        await RecordDragDemoAsync(Path.Combine(images, "drag-demo.gif"));
    }

    // Найбільший кадр іконки (256×256, PNG) -> 128×128 для шапки README
    private static void SaveIcon(string icoPath, string pngPath)
    {
        byte[] ico = File.ReadAllBytes(icoPath);
        int count = BitConverter.ToUInt16(ico, 4);
        int bestSize = 0, bestOffset = 0, bestLength = 0;

        for (int i = 0; i < count; i++)
        {
            int entry = 6 + 16 * i;
            int size = ico[entry] == 0 ? 256 : ico[entry];

            if (size > bestSize)
            {
                bestSize = size;
                bestLength = BitConverter.ToInt32(ico, entry + 8);
                bestOffset = BitConverter.ToInt32(ico, entry + 12);
            }
        }

        using MemoryStream stream = new(ico, bestOffset, bestLength);
        using Bitmap big = new(stream);
        using Bitmap small = new(128, 128);

        using (Graphics g = Graphics.FromImage(small))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(big, 0, 0, 128, 128);
        }

        small.Save(pngPath, ImageFormat.Png);
        Console.WriteLine("  icon.png (128×128)");
    }

    // ===================== Анімація перетягування =====================

    private sealed record Frame(byte[] Rgb, long Milliseconds);

    // Справжня миша перетягує картки в «Таблиці» (Move) і копіює слова в «Пропусках» (Copy);
    // утиліта тим часом знімає вікно разом із курсором ~11 разів на секунду
    private static async Task RecordDragDemoAsync(string gifPath)
    {
        GetCursorPos(out Point originalCursor);
        TestSession session = Scenarios.CreateSession(TestMode.Training, Scenarios.MainQuestionIds);
        TestForm form = new(session) { TopMost = true };
        form.Show();
        form.ShowQuestion(1);
        await Task.Delay(800);

        List<Frame> frames = new();
        Stopwatch clock = Stopwatch.StartNew();
        using CancellationTokenSource stop = new();
        IntPtr handle = form.Handle;

        Task recorder = Task.Run(() =>
        {
            while (!stop.IsCancellationRequested)
            {
                long started = clock.ElapsedMilliseconds;

                using (Bitmap window = WindowCapture.CaptureWithCursor(handle))
                {
                    lock (frames)
                    {
                        frames.Add(new Frame(ToRgb(window, QuestionArea), started));
                    }
                }

                Thread.Sleep((int)Math.Max(10, 90 - (clock.ElapsedMilliseconds - started)));
            }
        });

        try
        {
            await Task.Delay(1200);

            // «Таблиця відповідностей»: три картки — у свої клітинки
            QuestionView table = form.CurrentView;

            for (int row = 0; row < 3; row++)
            {
                CardLabel card = table.GetZone("palette").Cards.First(c => c.Value == table.Question.Rows[row].Answer);
                await DragAsync(card, Center(table.GetZone("row:" + row)));
            }

            await Task.Delay(1000);

            // «Слова в пропуски»: слово, потрібне двічі, копіюється в обидва пропуски, потім ще одне слово
            form.ShowQuestion(3);
            await Task.Delay(1200);
            QuestionView gaps = form.CurrentView;
            List<string> answers = gaps.Question.GetGapAnswers();
            string repeated = answers.GroupBy(a => a).First(g => g.Count() > 1).Key;
            DropZone words = gaps.GetZone("palette");

            await DragAsync(words.Cards.First(c => c.Value == repeated), Center(gaps.GetZone("gap:" + answers.IndexOf(repeated))));
            await DragAsync(words.Cards.First(c => c.Value == repeated), Center(gaps.GetZone("gap:" + answers.LastIndexOf(repeated))));

            int other = answers.FindIndex(a => a != repeated);
            await DragAsync(words.Cards.First(c => c.Value == answers[other]), Center(gaps.GetZone("gap:" + other)));
            await Task.Delay(1800);
        }
        finally
        {
            stop.Cancel();
            await recorder;
            SetCursorPos(originalCursor.X, originalCursor.Y);
            form.TopMost = false;
            form.BeginInvoke(() => Ui.Click(form, "btnFinish"));
            MessageForm ask = await Ui.WaitForFormAsync<MessageForm>(0);
            Ui.Find<Button>(ask, "btnYes").PerformClick();
            await Task.Delay(500);
        }

        List<int> delays = frames.Select((f, i) =>
            (int)Math.Round(((i + 1 < frames.Count ? frames[i + 1].Milliseconds : f.Milliseconds + 1500) - f.Milliseconds) / 10.0)).ToList();
        GifWriter.Save(gifPath, QuestionArea.Width, QuestionArea.Height, frames.Select(f => f.Rgb).ToList(), delays);
        Console.WriteLine($"  drag-demo.gif ({QuestionArea.Width}×{QuestionArea.Height}, кадрів: {frames.Count}, " +
                          $"{new FileInfo(gifPath).Length / 1024} КБ)");
    }

    // Плавно ведемо курсор до картки, натискаємо, тягнемо до цілі і відпускаємо — як рука людини
    private static async Task DragAsync(CardLabel card, Point target)
    {
        Point start = Center(card);

        await Task.Run(() =>
        {
            GetCursorPos(out Point from);
            Glide(from, start, 12, 18);
        });

        await Task.Delay(150);

        // Безпека: тиснемо кнопку, лише якщо під курсором саме картка програми
        if (WindowFromPoint(start) != card.Handle)
        {
            throw new InvalidOperationException("Під курсором не картка програми — перетягування скасовано");
        }

        // Рухи миші — у фоновому потоці, а вікно тим часом обробляє перетягування
        await Task.Run(() =>
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(150);
            Glide(start, target, 32, 22);
            Thread.Sleep(350);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        });

        await Task.Delay(500);
    }

    // Рух курсора з плавним розгоном і гальмуванням
    private static void Glide(Point from, Point to, int steps, int pauseMs)
    {
        for (int i = 1; i <= steps; i++)
        {
            double t = (double)i / steps;
            double eased = t * t * (3 - 2 * t);
            SetCursorPos(from.X + (int)((to.X - from.X) * eased), from.Y + (int)((to.Y - from.Y) * eased));
            Thread.Sleep(pauseMs);
        }
    }

    private static Point Center(Control control)
    {
        return control.PointToScreen(new Point(control.Width / 2, control.Height / 2));
    }

    // Частина знімка -> байти RGB
    private static byte[] ToRgb(Bitmap window, Rectangle area)
    {
        using Bitmap part = window.Clone(area, PixelFormat.Format24bppRgb);
        BitmapData data = part.LockBits(new Rectangle(0, 0, part.Width, part.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        byte[] rgb = new byte[part.Width * part.Height * 3];

        try
        {
            for (int y = 0; y < part.Height; y++)
            {
                byte[] row = new byte[part.Width * 3];
                Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, row.Length);

                for (int x = 0; x < part.Width; x++)
                {
                    // У пам'яті Windows — BGR
                    rgb[(y * part.Width + x) * 3] = row[x * 3 + 2];
                    rgb[(y * part.Width + x) * 3 + 1] = row[x * 3 + 1];
                    rgb[(y * part.Width + x) * 3 + 2] = row[x * 3];
                }
            }
        }
        finally
        {
            part.UnlockBits(data);
        }

        return rgb;
    }
}
