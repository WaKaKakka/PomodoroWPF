using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace PomodoroWPF.Services
{
    public class HotkeyService : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_NOREPEAT = 0x4000;
        private const int WM_HOTKEY = 0x0312;

        // Hotkey IDs
        public const int HK_START_PAUSE = 1;
        public const int HK_RESET = 2;

        // VK codes
        private const uint VK_P = 0x50;
        private const uint VK_R = 0x52;

        private IntPtr _hwnd;
        private bool _registered;

        public event Action<int>? HotkeyPressed;

        public void Register(IntPtr hwnd)
        {
            _hwnd = hwnd;

            bool ok1 = RegisterHotKey(hwnd, HK_START_PAUSE, MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT, VK_P);
            bool ok2 = RegisterHotKey(hwnd, HK_RESET, MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT, VK_R);
            _registered = ok1 || ok2;

            if (!ok1) Console.WriteLine("[Hotkey] Failed to register Ctrl+Shift+P");
            if (!ok2) Console.WriteLine("[Hotkey] Failed to register Ctrl+Shift+R");

            // Hook into window message loop
            var source = HwndSource.FromHwnd(hwnd);
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                HotkeyPressed?.Invoke(id);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Unregister()
        {
            if (!_registered || _hwnd == IntPtr.Zero) return;
            UnregisterHotKey(_hwnd, HK_START_PAUSE);
            UnregisterHotKey(_hwnd, HK_RESET);
            _registered = false;
        }

        public void Dispose()
        {
            Unregister();
        }
    }
}
