using System.Runtime.InteropServices;
using Lab_Testing;

namespace ScreenshotMaker;

// Діагностика: де в порядку вікон Windows (z-order) опиняються діалоги контрольної на весь екран
internal static class TopmostDiagnostics
{
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")] private static extern bool IsWindowEnabled(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr hwnd, uint cmd);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOPMOST = 0x8;
    private const uint GW_HWNDPREV = 3;
    private const uint GW_OWNER = 4;

    public static async Task RunAsync()
    {
        TestSession session = Scenarios.CreateSession(TestMode.Control, Scenarios.MainQuestionIds);
        TestForm form = new(session);
        form.Show();
        await Task.Delay(1500);

        Report("вікно тесту", form.Handle, form.Handle);

        form.BeginInvoke(() => Ui.Click(form, "btnFinish"));
        MessageForm message = await Ui.WaitForFormAsync<MessageForm>(0);
        await Task.Delay(800);

        Report("діалог «Завершити?»", message.Handle, form.Handle);
        Console.WriteLine($"  вікно тесту доступне для кліків: {IsWindowEnabled(form.Handle)}");
        Console.WriteLine($"  активне вікно — діалог: {GetForegroundWindow() == message.Handle}");

        Ui.Find<Button>(message, "btnNo").PerformClick();
        await Task.Delay(300);
        form.BeginInvoke(() => Ui.Click(form, "btnFinish"));
        message = await Ui.WaitForFormAsync<MessageForm>(0);
        Ui.Find<Button>(message, "btnYes").PerformClick();
        await Task.Delay(500);
    }


    // Діагностика клавіш: чи спрацьовує Ctrl+Shift+F12 (лише Debug) на справжнє натискання —
    // з різним фокусом і різним порядком відпускання клавіш
    public static async Task RunKeysAsync()
    {
        foreach ((bool focusOnForm, bool modifiersFirst) in new[] { (false, true), (false, false), (true, true) })
        {
            TestSession session = Scenarios.CreateSession(TestMode.Control, Scenarios.MainQuestionIds);
            TestForm form = new(session);
            form.Show();
            await Task.Delay(1500);

            if (focusOnForm)
            {
                form.ActiveControl = null;
            }

            string focus = form.ActiveControl?.GetType().Name ?? "вікно тесту (без елемента)";
            bool pressed = await HotKeys.PressCtrlShiftF12Async(form, modifiersFirst);
            MessageForm? message = pressed ? await TryWaitMessageAsync() : null;

            Console.WriteLine($"  фокус: {focus}; відпускання: {(modifiersFirst ? "спершу Ctrl і Shift" : "спершу F12")} — " +
                              $"збій спрацював: {message != null}");

            if (message != null)
            {
                Ui.Find<Button>(message, "btnOk").PerformClick();
            }
            else
            {
                form.BeginInvoke(() => Ui.Click(form, "btnFinish"));
                MessageForm ask = await Ui.WaitForFormAsync<MessageForm>(0);
                Ui.Find<Button>(ask, "btnYes").PerformClick();
            }

            await Task.Delay(500);
        }
    }

    private static async Task<MessageForm?> TryWaitMessageAsync()
    {
        for (int i = 0; i < 30; i++)
        {
            MessageForm? message = Application.OpenForms.OfType<MessageForm>().FirstOrDefault(f => f.Visible);

            if (message != null)
            {
                return message;
            }

            await Task.Delay(100);
        }

        return null;
    }

    private static void Report(string name, IntPtr hwnd, IntPtr testWindow)
    {
        bool topmost = (GetWindowLong(hwnd, GWL_EXSTYLE) & WS_EX_TOPMOST) != 0;
        IntPtr owner = GetWindow(hwnd, GW_OWNER);

        // Чи лежить вікно вище за вікно тесту: ідемо від вікна тесту вгору по z-order
        bool aboveTest = false;

        for (IntPtr w = GetWindow(testWindow, GW_HWNDPREV); w != IntPtr.Zero; w = GetWindow(w, GW_HWNDPREV))
        {
            if (w == hwnd)
            {
                aboveTest = true;
                break;
            }
        }

        Console.WriteLine($"  {name}: topmost={topmost}, власник — вікно тесту: {owner == testWindow}, " +
                          $"вище за вікно тесту: {(hwnd == testWindow ? "—" : aboveTest.ToString())}");
    }
}
