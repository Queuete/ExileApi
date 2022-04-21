using System.Runtime.InteropServices;
using SharpDX;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PositionedComponentOffsets
    {
        [FieldOffset(0x8)] public long OwnerAddress;
        [FieldOffset(0x1D9)] public byte Reaction;
        [FieldOffset(0x260)] public int GridX;
        [FieldOffset(0x264)] public int GridY;
        [FieldOffset(0x268)] public float Rotation;
        [FieldOffset(0x280)] public float Scale;
        [FieldOffset(0x284)] public int Size;
        [FieldOffset(0x28C)] public float WorldX;
        [FieldOffset(0x290)] public float WorldY;
    }
}
