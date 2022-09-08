using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using SharpDX;

namespace ExileCore.Shared.Nodes
{
    public class ToggleHotkeyNode : HotkeyNode
    {
        public bool Enabled {
            get { return enabled; }
            set { if ( value != enabled ) { enabled = value; OnValueChanged(); } }
        }
        private bool enabled = false;
        public ToggleHotkeyNode(Keys key) : base(key) { }
        public static implicit operator Keys(ToggleHotkeyNode node) => node.Value;
        public static implicit operator ToggleHotkeyNode(Keys value) => new ToggleHotkeyNode(value);
        public static implicit operator bool(ToggleHotkeyNode node) => node.Enabled;
    }
    public class HotkeyNode
    {
        private bool _pressed;
        private bool _unPressed;
        [JsonIgnore] public Action OnValueChanged = delegate { };
        private Keys value;

        public HotkeyNode()
        {
            value = Keys.Space;
        }

        public HotkeyNode(Keys value)
        {
            Value = value;
        }

        public Keys Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    this.value = value;

                    try
                    {
                        OnValueChanged();
                    }
                    catch
                    {
                        DebugWindow.LogMsg("Error in function that subscribed for: HotkeyNode.OnValueChanged", 10, Color.Red);
                    }
                }
            }
        }

        public static implicit operator Keys(HotkeyNode node)
        {
            return node.Value;
        }

        public static implicit operator HotkeyNode(Keys value)
        {
            return new HotkeyNode(value);
        }

        public bool PressedOnce()
        {
            if (Input.IsKeyDown(value))
            {
                if (_pressed)
                    return false;

                _pressed = true;
                return true;
            }

            _pressed = false;
            return false;
        }

        public bool UnpressedOnce()
        {
            if (Input.GetKeyState(value))
                _unPressed = true;
            else
            {
                if (_unPressed)
                {
                    _unPressed = false;
                    return true;
                }
            }

            return false;
        }
    }
}
