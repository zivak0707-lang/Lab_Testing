using System.Runtime.InteropServices;

namespace ScreenshotMaker;

// Справжні натискання клавіш (як людина з клавіатури) — лише у вікно утиліти, яке зараз активне
internal static class HotKeys
{
    [DllImport("user32.dll")] private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);

    private const byte VK_CONTROL = 0x11, VK_SHIFT = 0x10, VK_F12 = 0x7B;
    private const uint KEYUP = 0x2;

    // Ctrl+Shift+F12 (перевірка аварійного завершення у збірці Debug). modifiersFirst — спершу відпустити Ctrl і Shift,
    // потім F12 (так часто натискають люди). false — клавіші не надіслано: активне вікно не утиліти
    public static async Task<bool> PressCtrlShiftF12Async(Form target, bool modifiersFirst)
    {
        target.Activate();
        await Task.Delay(300);
        GetWindowThreadProcessId(GetForegroundWindow(), out uint pid);

        if (pid != Environment.ProcessId)
        {
            return false;
        }

        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
        keybd_event(VK_SHIFT, 0, 0, UIntPtr.Zero);
        keybd_event(VK_F12, 0, 0, UIntPtr.Zero);
        await Task.Delay(80);

        if (modifiersFirst)
        {
            keybd_event(VK_SHIFT, 0, KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYUP, UIntPtr.Zero);
            await Task.Delay(40);
            keybd_event(VK_F12, 0, KEYUP, UIntPtr.Zero);
        }
        else
        {
            keybd_event(VK_F12, 0, KEYUP, UIntPtr.Zero);
            keybd_event(VK_SHIFT, 0, KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYUP, UIntPtr.Zero);
        }

        return true;
    }
}
