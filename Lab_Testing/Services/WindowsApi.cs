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
