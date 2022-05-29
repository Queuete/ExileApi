#define DebugKeys
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using ExileCore.Shared;
using ExileCore.Shared.Helpers;
using MoreLinq.Extensions;
using SharpDX;

namespace ExileCore
{
    public class Input
    {
        private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;
        private const int ACTION_DELAY = 1;
        private const int MOVEMENT_DELAY = 7;
        private const int CLICK_DELAY = 5;
        private const int KEY_PRESS_DELAY = 10;
        public const int MOUSEEVENTF_MOVE = 0x0001;
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_MIDDOWN = 0x0020;
        public const int MOUSEEVENTF_MIDUP = 0x0040;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        public const int MOUSE_EVENT_WHEEL = 0x800;
        private static readonly Dictionary<Keys, bool> Keys = new Dictionary<Keys, bool>();
        public static readonly HashSet<Keys> RegisteredKeys = new HashSet<Keys>();
        private static readonly object locker = new object();
        private static readonly WaitTime cursorPositionSmooth = new WaitTime(1);
        private static readonly WaitTime keyPress = new WaitTime(ACTION_DELAY);
        private static readonly Dictionary<Keys, bool> KeysPressed = new Dictionary<Keys, bool>();
        private static readonly Stopwatch sw = Stopwatch.StartNew();

        static Input()
        {
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                KeysPressed[key] = false;
            }
        }

        public static Vector2 ForceMousePosition => WinApi.GetCursorPositionPoint();
        public static Vector2 MousePosition { get; private set; }
        public static System.Numerics.Vector2 MousePositionNum => MousePosition.ToVector2Num();

        public static bool IsKeyDown(int nVirtKey)
        {
            return IsKeyDown((Keys) nVirtKey);
        }

        public static bool IsKeyDown(Keys nVirtKey)
        {
#if DebugKeys
            if (!Keys.ContainsKey(nVirtKey))
                DebugWindow.LogError($"Key '{nVirtKey}' is not registered. Use {nameof(Input)}.{nameof(RegisterKey)}(Settings.MyKey) in Initialize function for registration.", 10);
#endif

            return Keys[nVirtKey];
        }

        public static bool GetKeyState(Keys key)
        {
            return WinApi.GetKeyState(key) < 0;
        }

        /// <summary>
        /// Check if a key was pressed but is now up
        /// </summary>
        public static bool CheckKeyPressed(Keys key)
        {
            return
                WinApi.GetKeyState(key & System.Windows.Forms.Keys.KeyCode) >= 0 &&
                (WinApi.GetAsyncKeyState(key) & 1) == 1;
        }
        public static void ClearAsyncBuffer()
        {
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                WinApi.GetAsyncKeyState(key);
            }
        }
        public static Keys GetModifiers()
        {
            Keys modifiers = System.Windows.Forms.Keys.None;
            if (GetKeyState(System.Windows.Forms.Keys.ShiftKey))
            {
                modifiers |= System.Windows.Forms.Keys.Shift;
            }
            if (GetKeyState(System.Windows.Forms.Keys.ControlKey))
            {
                modifiers |= System.Windows.Forms.Keys.Control;
            }
            if (GetKeyState(System.Windows.Forms.Keys.Menu))
            {
                modifiers |= System.Windows.Forms.Keys.Alt;
            }
            return modifiers;
        }
        public static Keys GetPressedKeys(bool withModifiers = false)
        {
            var key = Enum.GetValues(typeof(Keys))
                            .Cast<Keys>()
                            .Where(k => k != System.Windows.Forms.Keys.None)
                            .Where(k => k != System.Windows.Forms.Keys.KeyCode)
                            .Where(k => (k & ~System.Windows.Forms.Keys.KeyCode) == System.Windows.Forms.Keys.None)
                            .Where(CheckKeyPressed)
                            .LastOrDefault();
            if (withModifiers)
            {
                return key | GetModifiers();
            }
            else
            {
                return key;
            }
        }
        public static bool KeyPressed(Keys key)
        {
            return 
                WinApi.GetKeyState(key) >= 0 &&
                WinApi.GetAsyncKeyState(key) < 0;
        }

        public static void RegisterKey(Keys key)
        {
            if (!Keys.TryGetValue(key, out _))
            {
                lock (locker)
                {
                    Keys[key] = false;
                    RegisteredKeys.Add(key);
                }
            }
        }

        public static event EventHandler<Keys> ReleaseKey;

        public static void Update(IntPtr windowPtr)
        {
            MousePosition = WinApi.GetCursorPosition(windowPtr);

            try
            {
                RegisteredKeys.ForEach(key =>
                {
                    var keyState = GetKeyState(key);
                    if (keyState == false && Keys[key]) ReleaseKey?.Invoke(null, key);

                    Keys[key] = keyState;
                });
            }
            catch (Exception e)
            {
                DebugWindow.LogMsg($"{nameof(Input)} {e}");
            }
        }

        public static IEnumerator SetCursorPositionSmooth(Vector2 vec)
        {
            var step = Math.Max(vec.Distance(ForceMousePosition) / 100, 4);

            if (step > 6)
            {
                for (var i = 0; i < step; i++)
                {
                    var vector2 = Vector2.SmoothStep(ForceMousePosition, vec, i / step);
                    SetCursorPos(vector2);
                    MouseMove();
                    yield return cursorPositionSmooth;
                }
            }
            else
                SetCursorPos(vec);
        }

        public static void VerticalScroll(bool forward, int clicks)
        {
            if (forward)
                WinApi.mouse_event(MOUSE_EVENT_WHEEL, 0, 0, clicks * 120, 0);
            else
                WinApi.mouse_event(MOUSE_EVENT_WHEEL, 0, 0, -(clicks * 120), 0);
        }

        public static void SetCursorPos(Vector2 vec)
        {
            WinApi.SetCursorPos((int) vec.X, (int) vec.Y);
            MouseMove();
        }

        public static void Click(MouseButtons buttons)
        {
            switch (buttons)
            {
                case MouseButtons.Left:
                    MouseMove();
                    WinApi.mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    WinApi.mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case MouseButtons.Right:
                    MouseMove();
                    WinApi.mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    WinApi.mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
            }
        }

        public static void LeftDown()
        {
            WinApi.mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        }

        public static void LeftUp()
        {
            WinApi.mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /*public static IEnumerator LeftClick()
        {
            yield return LeftClick(0, 0);
        }
        public static IEnumerator RightClick()
        {
            yield return RightClick(0, 0);
        }
        static WaitTime leftClick = new WaitTime(CLICK_DELAY);

        public static IEnumerator LeftClick(int x, int y)
        {
            MouseMove();
            WinApi.mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            MouseMove();
            WinApi.mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
            MouseMove();
            yield return CoroutineState.Wait;
        }
        public static IEnumerator RightClick(int x, int y)
        {
            MouseMove();
            WinApi.mouse_event(MOUSEEVENTF_RIGHTDOWN, x, y, 0, 0);
            MouseMove();
            WinApi.mouse_event(MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
            MouseMove();
            yield return null;
        }*/
        public static void MouseMove()
        {
            WinApi.mouse_event(MOUSEEVENTF_MOVE, 0, 0, 0, 0);
        }

        public static IEnumerator KeyPress(Keys key)
        {
            KeyDown(key);
            yield return keyPress;
            KeyUp(key);
        }

        public static void KeyDown(Keys key)
        {
            if (key.HasFlag(System.Windows.Forms.Keys.Shift))
            {
                KeyDown(System.Windows.Forms.Keys.ShiftKey);
            }
            if (key.HasFlag(System.Windows.Forms.Keys.Control))
            {
                KeyDown(System.Windows.Forms.Keys.ControlKey);
            }
            if (key.HasFlag(System.Windows.Forms.Keys.Alt))
            {
                KeyDown(System.Windows.Forms.Keys.Menu);
            }

            WinApi.keybd_event((byte)(key & System.Windows.Forms.Keys.KeyCode), 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
        }

        public static void KeyUp(Keys key)
        {
            WinApi.keybd_event((byte)(key & System.Windows.Forms.Keys.KeyCode), 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            if (key.HasFlag(System.Windows.Forms.Keys.Shift))
            {
                KeyUp(System.Windows.Forms.Keys.ShiftKey);
            }
            if (key.HasFlag(System.Windows.Forms.Keys.Control))
            {
                KeyUp(System.Windows.Forms.Keys.ControlKey);
            }
            if (key.HasFlag(System.Windows.Forms.Keys.Alt))
            {
                KeyUp(System.Windows.Forms.Keys.Menu);
            }
        }

        public static void KeyDown(Keys key, IntPtr handle)
        {
            WinApi.SendMessage(handle, 0x100, (int) key, 0);
        }

        public static void KeyUp(Keys key, IntPtr handle)
        {
            WinApi.SendMessage(handle, 0x101, (int) key, 0);
        }

        public static void KeyPressRelease(Keys key, IntPtr handle)
        {
            if (sw.ElapsedMilliseconds >= KEY_PRESS_DELAY && KeysPressed[key])
            {
                KeyUp(key, handle);

                lock (locker)
                {
                    KeysPressed[key] = false;
                }

                sw.Restart();
            }
            else if (!KeysPressed[key])
            {
                KeyDown(key, handle);

                lock (locker)
                {
                    KeysPressed[key] = true;
                }

                sw.Restart();
            }
        }

        public static void KeyPressRelease(Keys key)
        {
            if (sw.ElapsedMilliseconds >= KEY_PRESS_DELAY && KeysPressed[key])
            {
                KeyUp(key);

                lock (locker)
                {
                    KeysPressed[key] = false;
                }

                sw.Restart();
            }
            else if (!KeysPressed[key])
            {
                KeyDown(key);

                lock (locker)
                {
                    KeysPressed[key] = true;
                }

                sw.Restart();
            }
        }
    }
}
