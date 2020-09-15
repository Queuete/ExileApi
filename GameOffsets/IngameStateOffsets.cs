using System.Runtime.InteropServices;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct IngameStateOffsets
    {
		[FieldOffset(0x80)] public long IngameUi;
		[FieldOffset(0xA0)] public long EntityLabelMap;
        [FieldOffset(0x500)] public long Data;
        [FieldOffset(0x508)] public long ServerData;
        [FieldOffset(0x630)] public long UIRoot;
		[FieldOffset(0x668)] public long UIHoverTooltip;
		[FieldOffset(0x670)] public float CurentUElementPosX;
		[FieldOffset(0x674)] public float CurentUElementPosY;
		[FieldOffset(0x678)] public long UIHover;
        [FieldOffset(0x6A0)] public int MouseXGlobal;
        [FieldOffset(0x6A4)] public int MouseYGlobal;
        [FieldOffset(0x6AC)] public float UIHoverX;
        [FieldOffset(0x6B0)] public float UIHoverY;
        [FieldOffset(0x6B4)] public float MouseXInGame;
        [FieldOffset(0x6B8)] public float MouseYInGame;
        [FieldOffset(0x6DC)] public float TimeInGame;
        [FieldOffset(0x6E4)] public float TimeInGameF;
        [FieldOffset(0x6F8)] public int DiagnosticInfoType;
        [FieldOffset(0x928)] public long LatencyRectangle;
        [FieldOffset(0xDC8)] public long FrameTimeRectangle;
        [FieldOffset(0x1018)] public long FPSRectangle;
        [FieldOffset(0x1168)] public int Camera;
    }
}
