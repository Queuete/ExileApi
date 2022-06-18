using System;
using System.Collections.Generic;
using ExileCore.Shared;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace ExileCore
{
    public class DebugWindow
    {
        private static readonly object locker = new object();
        private static readonly Dictionary<string, DebugMsgDescription> Messages;
        private static readonly List<DebugMsgDescription> MessagesList;
        private static readonly Queue<string> toDelete;
        private static readonly CircularBuffer<DebugMsgDescription> History;
        private readonly Graphics _graphics;
        private readonly CoreSettings _coreSettings;
        private Vector2 position;

        static DebugWindow()
        {
            Messages = new Dictionary<string, DebugMsgDescription>(24);
            MessagesList = new List<DebugMsgDescription>(24);
            toDelete = new Queue<string>(24);
            History = new CircularBuffer<DebugMsgDescription>(4096);
        }

        public DebugWindow(Graphics graphics, CoreSettings coreSettings)
        {
            this._graphics = graphics;
            this._coreSettings = coreSettings;
            graphics.InitImage("menu-background.png");
        }

        public void Render()
        {
            try
            {
                if (_coreSettings.ShowDebugLog)
                {
                    unsafe
                    {
                        ImGui.PushFont(_graphics.Font.Atlas);
                    }

                    ImGui.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.Appearing);
                    ImGui.SetNextWindowSize(new Vector2(600, 1000), ImGuiCond.Appearing);
                    var openedWindow = _coreSettings.ShowDebugLog.Value;
                    ImGui.Begin("Debug log",ref openedWindow);
                    _coreSettings.ShowDebugLog.Value = openedWindow;
                    foreach (var msg in History)
                    {
                        if (msg == null) continue;
                        if (msg.MsgType == MsgType.Debug && !_coreSettings.ShowDebugMessages.Value) continue;
                        if (msg.MsgType == MsgType.Error && !_coreSettings.ShowErrorMessages.Value) continue;
                        if (msg.MsgType == MsgType.Message && !_coreSettings.ShowInformationMessages.Value) continue;
                        ImGui.PushStyleColor(ImGuiCol.Text, msg.ColorV4);
                        ImGui.TextUnformatted($"{msg.Time.ToLongTimeString()}: {msg.Msg}");
                        ImGui.PopStyleColor();
                    }
                    if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                    {
                        ImGui.SetScrollHereY(1.0f);
                    }

                    ImGui.PopFont();
                    ImGui.End();
                }

                if (MessagesList.Count == 0) return;

                position = new Vector2(10, 35);

                for (var index = 0; index < MessagesList.Count; index++)
                {
                    var message = MessagesList[index];
                    if (message == null) continue;

                    if (message.Time < DateTime.UtcNow.AddSeconds(message.Duration))
                    {
                        toDelete.Enqueue(message.Msg);
                        continue;
                    }

                    if (message.MsgType == MsgType.Debug && !_coreSettings.ShowDebugMessages.Value) continue;
                    var draw = message.Msg;
                    if (message.Count > 1) draw = $"({message.Count}){draw}";

                    var currentPosition = _graphics.DrawText(draw, position, message.Color);

                    _graphics.DrawImage("menu-background.png",
                        new RectangleF(position.X - 5, position.Y, currentPosition.X + 20, currentPosition.Y));

                    position = new Vector2(position.X, position.Y + currentPosition.Y);
                }

                while (toDelete.Count > 0)
                {
                    var delete = toDelete.Dequeue();

                    if (Messages.TryGetValue(delete, out var debugMsgDescription))
                    {
                        MessagesList.Remove(debugMsgDescription);

                        switch (debugMsgDescription.MsgType)
                        {
                            case MsgType.Message:
                                Core.Logger.Information($"{debugMsgDescription.Msg}");
                                break;
                            case MsgType.Error:
                                Core.Logger.Error($"{debugMsgDescription.Msg}");
                                break;
                            case MsgType.Debug:
                                Core.Logger.Verbose($"{debugMsgDescription.Msg}");
                                break;
                            default:
                                Core.Logger.Verbose($"THIS SHOULD NOT BE LOGGED! {debugMsgDescription.Msg}");
                                break;
                        }
                    }

                    Messages.Remove(delete);
                }
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }
        }

        public static void LogMsg(string msg)
        {
            LogMsg(msg, 1f);
        }

        public static void LogMsg(string msg, float time)
        {
            LogMsg(msg, time, Color.White);
        }

        public static void LogMsg(string msg, float time, Color color)
        {
            LogMsg(msg, time, color, MsgType.Message);
        }

        public static void LogError(string msg, float time = 2f)
        {
            LogMsg(msg, time, Color.Red, MsgType.Error);
        }

        public static void LogDebug(string msg, float time = 2f)
        {
            LogMsg(msg, time, Color.LightBlue, MsgType.Debug);
        }

        public static void LogMsg(string msg, float time, Color color, MsgType msgType)
        {
            try
            {
                if (Messages.TryGetValue(msg, out var result))
                {
                    result.Count++;
                    var newMessage = result.Copy();
                    newMessage.Time = DateTime.UtcNow;
                    newMessage.Duration = time;
                    newMessage.Color = color;
                    History.PushBack(result);
                }
                else
                {
                    result = new DebugMsgDescription
                    {
                        MsgType = msgType,
                        Msg = msg,
                        Time = DateTime.UtcNow,
                        Duration = time,
                        ColorV4 = color.ToImguiVec4(),
                        Color = color,
                        Count = 1
                    };

                    lock (locker)
                    {
                        Messages[msg] = result;
                        MessagesList.Add(result);
                        History.PushBack(result);
                    }
                }
            }
            catch (Exception e)
            {
                Core.Logger.Error($"{nameof(DebugWindow)} -> {e}");
            }
        }
    }

    public class DebugMsgDescription
    {
        public MsgType MsgType { get; set; }
        public string Msg { get; set; }
        public DateTime Time { get; set; }
        public float Duration { get; set; }
        public Vector4 ColorV4 { get; set; }
        public Color Color { get; set; }
        public int Count { get; set; }

        public DebugMsgDescription Copy()
        {
            return new DebugMsgDescription
            {
                MsgType = MsgType,
                Msg = Msg,
                Time = Time,
                Duration = Duration,
                ColorV4 = ColorV4,
                Color = Color,
                Count = Count
            };
        }
    }

    public enum MsgType
    {
        Message,
        Error,
        Debug
    }
}
