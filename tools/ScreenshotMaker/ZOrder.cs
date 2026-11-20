using System.Runtime.InteropServices;

namespace ScreenshotMaker;

// Порядок вікон Windows (z-order): що чим перекрито. Знімок вікна цього не показує
// (PrintWindow знімає і вікно, перекрите іншим), тому видимість перевіряємо окремо
internal static class ZOrder
{
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr hwnd, uint cmd);
    [DllImport("user32.dll")] private static extern bool IsWindowEnabled(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOPMOST = 0x8;
    private const uint GW_HWNDPREV = 3;
    private const uint GW_OWNER = 4;

    // Вікно-власник (для повідомлення — вікно, яке його відкрило)
    public static IntPtr GetOwner(IntPtr hwnd)
    {
        return GetWindow(hwnd, GW_OWNER);
    }

    public static bool IsTopMost(IntPtr hwnd)
    {
        return (GetWindowLong(hwnd, GWL_EXSTYLE) & WS_EX_TOPMOST) != 0;
    }

    public static bool IsVisible(IntPtr hwnd)
    {
        return IsWindowVisible(hwnd);
    }

    public static bool IsEnabled(IntPtr hwnd)
    {
        return IsWindowEnabled(hwnd);
    }

    // Вікно upper лежить вище за вікно lower (ідемо від lower вгору по z-order)
    public static bool IsAbove(IntPtr upper, IntPtr lower)
    {
        for (IntPtr w = GetWindow(lower, GW_HWNDPREV); w != IntPtr.Zero; w = GetWindow(w, GW_HWNDPREV))
        {
            if (w == upper)
            {
                return true;
            }
        }

        return false;
    }

    // Повідомлення видно: воно вище за свого власника (і «поверх усіх», якщо власник «поверх усіх»)
    public static void ExpectDialogVisible(IntPtr dialog, IntPtr owner, string what)
    {
        if (!IsAbove(dialog, owner) || (IsTopMost(owner) && !IsTopMost(dialog)))
        {
            throw new InvalidOperationException(
                $"{what}: повідомлення відкрилося ПІД вікном-власником — його не видно, а вікно чекає на відповідь " +
                $"(власник «поверх усіх»: {IsTopMost(owner)}, повідомлення «поверх усіх»: {IsTopMost(dialog)})");
        }
    }
}
