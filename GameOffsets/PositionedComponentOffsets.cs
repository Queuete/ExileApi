using System.Runtime.InteropServices;
using SharpDX;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PositionedComponentOffsets
    {
        [FieldOffset(0x8)] public long OwnerAddress;
        [FieldOffset(0x159)] public byte Reaction; //<== wrong 3.16.2b
        [FieldOffset(0x1B0)] public Vector2 PrevPosition;
        [FieldOffset(0x1C8)] public Vector2 RelativeCoord;
        [FieldOffset(0x268)] public int GridX; // 3.16.2b
        [FieldOffset(0x26C)] public int GridY; // 3.16.2b
        [FieldOffset(0x1F0)] public float Rotation;
        [FieldOffset(0x208)] public float Scale;
        [FieldOffset(0x20C)] public int Size;
        [FieldOffset(0x218)] public Vector2 WorldPosition;
        [FieldOffset(0x22C)] public float WorldX; // 3.16.2b
        [FieldOffset(0x230)] public float WorldY; // 3.16.2b
    }
}
