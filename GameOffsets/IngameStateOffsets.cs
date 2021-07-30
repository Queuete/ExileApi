using System.Runtime.InteropServices;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct IngameStateOffsets
    {
        [FieldOffset(0x30)] public long Data;
        [FieldOffset(0xC0)] public long EntityLabelMap;
        [FieldOffset(0x98)] public long IngameUi; 
        [FieldOffset(0x498)] public long ServerData;
        [FieldOffset(0x5C0)] public long UIRoot;
        [FieldOffset(0x608)] public long UIHoverElement;
        [FieldOffset(0x608)] public long UIHover; // element which is currently hovered
        [FieldOffset(0x600)] public float UIHoverPosX; // top left corner of currently hovered UI element
        [FieldOffset(0x604)] public float UIHoverPosY; // top left corner of currently hovered UI element
        [FieldOffset(0x644)] public float MousePosX;
        [FieldOffset(0x648)] public float MousePosY;
        [FieldOffset(0x670)] public float TimeInGameF; // time since last frame in seconds
        [FieldOffset(0x674)] public float TimeInGame; // total time in game in seconds
        [FieldOffset(0x618)] public int DiagnosticInfoType;
        [FieldOffset(0x848)] public long LatencyRectangle;
        [FieldOffset(0xCE8)] public long FrameTimeRectangle;
        [FieldOffset(0xF38)] public long FPSRectangle;
        [FieldOffset(0x788)] public int Camera; 
    }
}
