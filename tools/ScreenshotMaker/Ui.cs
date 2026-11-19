using System.Diagnostics;
using System.Drawing.Imaging;
using System.Reflection;
using Lab_Testing;

namespace ScreenshotMaker;

// Керування формами програми (як це робив би користувач) і збереження знімків
internal class Ui
{
    private readonly string outDir;

    public Ui(string outDir)
    {
        this.outDir = outDir;
    }

    // ===================== Пошук і дії з контролами =====================

    public static T Find<T>(Control root, string name) where T : Control
    {
        return root.Controls.Find(name, true).OfType<T>().FirstOrDefault()
            ?? throw new InvalidOperationException($"У формі {root.Name} не знайдено контрол {name}");
    }

    public static void SetText(Control root, string name, string text)
    {
        Find<Control>(root, name).Text = text;
    }

    public static void Check(Control root, string name)
    {
        Find<RadioButton>(root, name).Checked = true;
    }

    public static void Click(Control root, string name)
    {
        Find<Button>(root, name).PerformClick();
    }

    // У LinkLabel немає PerformClick, тому подію LinkClicked викликаємо напряму
    public static void ClickLink(Control root, string name)
    {
        LinkLabel link = Find<LinkLabel>(root, name);
        MethodInfo onClick = typeof(LinkLabel).GetMethod("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance)!;
        onClick.Invoke(link, new object[] { new LinkLabelLinkClickedEventArgs(link.Links[0]) });
    }

    // Подвійний клік по клітинці таблиці (подію CellDoubleClick викликаємо напряму)
    public static void DoubleClickCell(DataGridView grid, int rowIndex)
    {
        MethodInfo onDoubleClick = typeof(DataGridView).GetMethod("OnCellDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance)!;
        onDoubleClick.Invoke(grid, new object[] { new DataGridViewCellEventArgs(0, rowIndex) });
    }

    // ===================== Знімки =====================

    // Знімок відкритої форми
    public async Task ShotAsync(Form form, int number)
    {
        form.Activate();
        form.Refresh();
        await Task.Delay(500);

        using Bitmap bmp = WindowCapture.Capture(form.Handle);
        Save(bmp, number);
    }

    // Знімок MessageBox або діалогу файлів: action відкриває діалог (через BeginInvoke, щоб сценарій
    // не чекав, поки діалог закриється), утиліта чекає на нього, знімає і натискає кнопку closeButtonId.
    // number == null — діалог лише закривається, без знімка
    public async Task DialogShotAsync(Form owner, Action action, int? number, int closeButtonId, int timeoutMs = 8000)
    {
        HashSet<IntPtr> before = WindowCapture.FindDialogs().ToHashSet();
        owner.BeginInvoke(action);

        IntPtr dialog = IntPtr.Zero;
        Stopwatch sw = Stopwatch.StartNew();

        while (dialog == IntPtr.Zero)
        {
            if (sw.ElapsedMilliseconds > timeoutMs)
            {
                throw new TimeoutException($"Рисунок {number}: діалог не з'явився");
            }

            await Task.Delay(100);
            dialog = WindowCapture.FindDialogs().FirstOrDefault(h => !before.Contains(h));
        }

        await Task.Delay(600);

        if (number != null)
        {
            using Bitmap bmp = WindowCapture.Capture(dialog);
            Save(bmp, number.Value);
        }

        WindowCapture.PressButton(dialog, closeButtonId);
        await Task.Delay(400);
    }

    // Знімок повідомлення програми (MessageForm з кнопками «Так/Ні/OK»): action відкриває його,
    // утиліта знімає вікно і натискає потрібну кнопку. number == null — лише натиснути кнопку
    public async Task MessageShotAsync(Form owner, Action action, int? number, Answer answer, int timeoutMs = 8000)
    {
        owner.BeginInvoke(action);
        MessageForm message = await WaitForFormAsync<MessageForm>(number ?? 0, timeoutMs);
        await Task.Delay(500);

        if (number != null)
        {
            using Bitmap bmp = WindowCapture.Capture(message.Handle);
            Save(bmp, number.Value);
        }

        string button = answer switch
        {
            Answer.Yes => "btnYes",
            Answer.No => "btnNo",
            _ => "btnOk"
        };

        Find<Button>(message, button).PerformClick();
        await Task.Delay(400);
    }

    // Знімок модальної форми програми (наприклад, «Про програму»), яку відкриває action
    public async Task FormShotAsync<T>(Form owner, Action action, int number) where T : Form
    {
        owner.BeginInvoke(action);
        T form = await WaitForFormAsync<T>(number);

        await Task.Delay(500);
        using (Bitmap bmp = WindowCapture.Capture(form.Handle))
        {
            Save(bmp, number);
        }

        form.Close();
        await Task.Delay(400);
    }

    public static async Task<T> WaitForFormAsync<T>(int number, int timeoutMs = 8000) where T : Form
    {
        Stopwatch sw = Stopwatch.StartNew();

        while (true)
        {
            T? form = Application.OpenForms.OfType<T>().FirstOrDefault(f => f.Visible);

            if (form != null)
            {
                return form;
            }

            if (sw.ElapsedMilliseconds > timeoutMs)
            {
                throw new TimeoutException($"Рисунок {number}: форма {typeof(T).Name} не з'явилася");
            }

            await Task.Delay(100);
        }
    }

    // Знімок форми в довільний файл (для перегляду всіх питань, не для звіту)
    public static async Task ShotToFileAsync(Form form, string path)
    {
        form.Activate();
        form.Refresh();
        await Task.Delay(400);

        using Bitmap bmp = WindowCapture.Capture(form.Handle);
        bmp.Save(path, ImageFormat.Png);
    }

    public void SaveShot(Bitmap bmp, int number)
    {
        Save(bmp, number);
    }

    private void Save(Bitmap bmp, int number)
    {
        Shot shot = ShotList.Get(number);
        bmp.Save(Path.Combine(outDir, shot.File), ImageFormat.Png);
        Console.WriteLine($"  Рисунок {number}: {shot.File} ({bmp.Width}×{bmp.Height})");
    }
}

// Яку кнопку натиснути в повідомленні програми
internal enum Answer
{
    Ok,
    Yes,
    No
}
