using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace ScreenshotMaker;

// Знімок окремого вікна через WinAPI (справжній вигляд вікна Windows разом із заголовком, без робочого столу)
internal static class WindowCapture
{
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
        public readonly int Width => Right - Left;
        public readonly int Height => Bottom - Top;
    }

    private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    [DllImport("user32.dll")] private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("dwmapi.dll")] private static extern int DwmGetWindowAttribute(IntPtr hwnd, int attr, out RECT rect, int size);
    [DllImport("user32.dll")] private static extern bool EnumThreadWindows(uint threadId, EnumWindowsProc callback, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr hwnd, StringBuilder name, int max);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hwnd);

    private const uint PW_RENDERFULLCONTENT = 2;       // малювати вміст так, як його показує DWM
    private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;  // видима рамка вікна без невидимих країв
    private const uint WM_COMMAND = 0x0111;
    private const uint WM_CLOSE = 0x0010;

    public const int IDOK = 1;
    public const int IDCANCEL = 2;
    public const int IDYES = 6;
    public const int IDNO = 7;

    // Знімок вікна, обрізаний по видимій рамці
    public static Bitmap Capture(IntPtr hwnd)
    {
        GetWindowRect(hwnd, out RECT window);

        if (DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS, out RECT frame, Marshal.SizeOf<RECT>()) != 0)
        {
            frame = window;
        }

        using Bitmap full = new(window.Width, window.Height, PixelFormat.Format32bppArgb);

        using (Graphics g = Graphics.FromImage(full))
        {
            IntPtr hdc = g.GetHdc();

            try
            {
                PrintWindow(hwnd, hdc, PW_RENDERFULLCONTENT);
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }

        Rectangle crop = new(frame.Left - window.Left, frame.Top - window.Top, frame.Width, frame.Height);
        crop.Intersect(new Rectangle(0, 0, full.Width, full.Height));
        return full.Clone(crop, PixelFormat.Format24bppRgb);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CURSORINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public Point ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ICONINFO
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    [DllImport("user32.dll")] private static extern bool GetCursorInfo(ref CURSORINFO info);
    [DllImport("user32.dll")] private static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO info);
    [DllImport("user32.dll")] private static extern bool DrawIconEx(IntPtr hdc, int x, int y, IntPtr hIcon, int w, int h, int step, IntPtr brush, int flags);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr handle);

    private const int DI_NORMAL = 0x0003;

    // Знімок вікна разом із поточним курсором миші (під час перетягування це курсор Move або Copy «+»)
    public static Bitmap CaptureWithCursor(IntPtr hwnd)
    {
        Bitmap bmp = Capture(hwnd);

        if (DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS, out RECT frame, Marshal.SizeOf<RECT>()) != 0)
        {
            GetWindowRect(hwnd, out frame);
        }

        CURSORINFO cursor = new() { cbSize = Marshal.SizeOf<CURSORINFO>() };

        if (!GetCursorInfo(ref cursor) || cursor.hCursor == IntPtr.Zero)
        {
            return bmp;
        }

        GetIconInfo(cursor.hCursor, out ICONINFO icon);
        int x = cursor.ptScreenPos.X - icon.xHotspot - frame.Left;
        int y = cursor.ptScreenPos.Y - icon.yHotspot - frame.Top;

        using (Graphics g = Graphics.FromImage(bmp))
        {
            IntPtr hdc = g.GetHdc();

            try
            {
                DrawIconEx(hdc, x, y, cursor.hCursor, 0, 0, 0, IntPtr.Zero, DI_NORMAL);
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }

        if (icon.hbmMask != IntPtr.Zero)
        {
            DeleteObject(icon.hbmMask);
        }

        if (icon.hbmColor != IntPtr.Zero)
        {
            DeleteObject(icon.hbmColor);
        }

        return bmp;
    }

    // Видимі діалогові вікна (#32770) поточного потоку: MessageBox, діалоги відкриття/збереження файлів
    public static List<IntPtr> FindDialogs()
    {
        List<IntPtr> result = new();

        EnumThreadWindows(GetCurrentThreadId(), (hwnd, _) =>
        {
            if (IsWindowVisible(hwnd) && GetClassName(hwnd) == "#32770")
            {
                result.Add(hwnd);
            }

            return true;
        }, IntPtr.Zero);

        return result;
    }

    // Головне видиме вікно чужого процесу з указаним класом (Блокнот, Excel)
    public static IntPtr FindProcessWindow(int processId, string className)
    {
        IntPtr found = IntPtr.Zero;

        EnumWindows((hwnd, _) =>
        {
            GetWindowThreadProcessId(hwnd, out uint pid);

            if (pid == processId && IsWindowVisible(hwnd) && GetClassName(hwnd) == className)
            {
                found = hwnd;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return found;
    }

    // Натискає кнопку діалогу (OK, Так, Ні, Скасувати)
    public static void PressButton(IntPtr dialog, int buttonId)
    {
        PostMessage(dialog, WM_COMMAND, buttonId, IntPtr.Zero);
    }

    public static void Close(IntPtr hwnd)
    {
        PostMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }

    public static void BringToFront(IntPtr hwnd)
    {
        SetForegroundWindow(hwnd);
    }

    private static string GetClassName(IntPtr hwnd)
    {
        StringBuilder name = new(256);
        GetClassName(hwnd, name, name.Capacity);
        return name.ToString();
    }
}
