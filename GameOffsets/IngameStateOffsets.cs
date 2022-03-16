using System.Runtime.InteropServices;
using GameOffsets.Native;
using SharpDX;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct IngameStateOffsets
    {
        [FieldOffset(0x18)] public long Data;
        [FieldOffset(0x78)] public long WorldData;
        [FieldOffset(0x98)] public long EntityLabelMap;
        [FieldOffset(0x1A8)] public long UIRoot; //3.16.3
        [FieldOffset(0x440)] public long IngameUi; //3.17.3
        [FieldOffset(0x1E0)] public long UIHover; // element which is currently hovered
        [FieldOffset(0x1E8)] public Vector2 UIHoverPos; // top left corner of currently hovered UI element
        [FieldOffset(0x1F0)] public long UIHoverTooltip;
        [FieldOffset(0x218)] public Vector2i MousePos;
        [FieldOffset(0x22C)] public Vector2 MousePosFloat;
        [FieldOffset(0x224)] public Vector2 UIHoverOffset; // mouse position offset in hovered UI element
        [FieldOffset(0x3F8)] public float TimeInGameF; // time since last frame in seconds
        [FieldOffset(0x3FC)] public float TimeInGame; // total time in game in seconds
        [FieldOffset(0x618)] public int DiagnosticInfoType;
        [FieldOffset(0x848)] public long LatencyRectangle;
        [FieldOffset(0xCE8)] public long FrameTimeRectangle;
        [FieldOffset(0xF38)] public long FPSRectangle;
    }
}
