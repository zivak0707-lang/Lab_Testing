using System.Runtime.InteropServices;

namespace Lab_Testing;

// Функції Windows, яких немає в WinForms (режим доброчесності контрольної).
// Жодна з них не перехоплює клавіатуру і не впливає на інші програми
internal static class WindowsApi
{
    public const int WM_ACTIVATEAPP = 0x001C;       // активною стала інша програма (або знову наша)
    public const int WM_CLIPBOARDUPDATE = 0x031D;   // змінився вміст буфера обміну

    private const uint WDA_MONITOR = 0x01;              // вікно на знімках — чорний прямокутник
    private const uint WDA_EXCLUDEFROMCAPTURE = 0x11;   // вікна немає на знімках (Windows 10 2004 і новіші)
    private const int SM_REMOTECONTROL = 0x2001;        // сеансом керують віддалено

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint affinity);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int index);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);
    private const uint SWP_NOSIZE = 0x0001, SWP_NOMOVE = 0x0002, SWP_NOACTIVATE = 0x0010;
    private const uint KeepPlace = SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE;   // не рухати, не змінювати розмір, не активувати
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOPMOST = 0x0008;

    // Вікно «поверх усіх»
    public static bool IsTopMost(IntPtr window)
    {
        return (GetWindowLong(window, GWL_EXSTYLE) & WS_EX_TOPMOST) != 0;
    }

    // Учень у тесті: вікно тесту — поверх інших вікон (разом з його повідомленнями)
    public static void KeepOnTop(IntPtr window)
    {
        SetWindowPos(window, HWND_TOPMOST, 0, 0, 0, 0, KeepPlace);
    }

    // Учень перейшов в іншу програму: тест перестає бути «поверх усіх» і стає одразу під її вікном,
    // тож ця програма видна і працює як звичайно (Alt+Tab, Win+R, Win+D). Фокус не забираємо
    public static void StepAside(IntPtr window)
    {
        SetWindowPos(window, HWND_NOTOPMOST, 0, 0, 0, 0, KeepPlace);

        IntPtr foreground = GetForegroundWindow();
        GetWindowThreadProcessId(foreground, out uint processId);

        if (foreground != IntPtr.Zero && processId != Environment.ProcessId && !IsTopMost(foreground))
        {
            SetWindowPos(window, foreground, 0, 0, 0, 0, KeepPlace);
        }
    }

    // Ховає вікно від знімків і запису екрана. Повертає, що вдалося
    public static string HideFromCapture(IntPtr window)
    {
        if (SetWindowDisplayAffinity(window, WDA_EXCLUDEFROMCAPTURE))
        {
            return "вікно сховано від знімків";
        }

        return SetWindowDisplayAffinity(window, WDA_MONITOR)
            ? "на знімках — чорний прямокутник"
            : "захист недоступний у цій версії Windows";
    }

    // Вікно отримуватиме WM_CLIPBOARDUPDATE, коли щось потрапляє в буфер обміну
    public static void WatchClipboard(IntPtr window, bool watch)
    {
        if (watch)
        {
            AddClipboardFormatListener(window);
        }
        else
        {
            RemoveClipboardFormatListener(window);
        }
    }

    // Програму запущено через віддалений робочий стіл або сеансом керують віддалено
    public static bool IsRemoteSession =>
        SystemInformation.TerminalServerSession || GetSystemMetrics(SM_REMOTECONTROL) != 0;
}
