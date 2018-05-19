using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hook
{
    class hook
    {
        public static class InterceptKeys
        {
            public enum KeyState
            {
                KeyDown,
                KeyUp
            }

            public static KeyState GetState(Keys Key)
            {
                if (_KeysDown.Contains(Key))
                    return KeyState.KeyDown;
                else
                    return KeyState.KeyUp;
            }

            public delegate void KeyboardEvent(Keys Key);
            public delegate void KeyboardStateEvent(Keys Key, InterceptKeys.KeyState State);

            public static event KeyboardEvent KeyDown;
            public static event KeyboardEvent KeyUp;
            public static event KeyboardStateEvent KeyEvent;

            private static List<Keys> _KeysDown = new List<Keys>();
            public static List<Keys> KeysDown
            {
                get
                {
                    return _KeysDown;
                }
            }

            private const int WH_KEYBOARD_LL = 13;
            private const int WM_KEYDOWN = 0x0100;
            private const int WM_KEYUP = 0x0101;
            private static LowLevelKeyboardProc _proc = HookCallback;
            private static IntPtr _hookID = IntPtr.Zero;

            public static void Hook()
            {
                _hookID = SetHook(_proc);
            }

            public static void UnHook()
            {
                UnhookWindowsHookEx(_hookID);
            }

            private static IntPtr SetHook(LowLevelKeyboardProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            private delegate IntPtr LowLevelKeyboardProc(
                int nCode, IntPtr wParam, IntPtr lParam);

            private static IntPtr HookCallback(
                int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    if (wParam == (IntPtr)WM_KEYDOWN)
                    {
                        if (!_KeysDown.Contains((Keys)vkCode))
                        {
                            if (KeyDown != null)
                                KeyDown((Keys)vkCode);

                            if (KeyEvent != null)
                                KeyEvent((Keys)vkCode, KeyState.KeyDown);

                            _KeysDown.Add((Keys)vkCode);
                        }
                    }
                    if (wParam == (IntPtr)WM_KEYUP)
                    {
                        if (KeyUp != null)
                            KeyUp((Keys)vkCode);

                        if (KeyEvent != null)
                            KeyEvent((Keys)vkCode, KeyState.KeyUp);

                        _KeysDown.Remove((Keys)vkCode);
                    }
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook,
                LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);
        }
    }
}
