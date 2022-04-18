namespace GameOffsets.Components
{
    using System.Runtime.InteropServices;
    using GameOffsets.Native;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct RenderItem
    {
        [FieldOffset(0x0000)] public ComponentHeader Header;
        [FieldOffset(0x0028)] public NativeUnicodeText ResourcePath;
    }
}
