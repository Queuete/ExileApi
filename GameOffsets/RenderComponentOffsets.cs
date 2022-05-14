using System.Runtime.InteropServices;
using GameOffsets.Native;
using SharpDX;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct RenderComponentOffsets
    {
        [FieldOffset(0x78)] public Vector3 Pos;
        [FieldOffset(0x84)] public Vector3 Bounds;
        [FieldOffset(0x9C)] public NativeStringU Name;
        [FieldOffset(0xBC)] public Vector3 Rotation;
        [FieldOffset(0xE4)] public float Height;
    }
}
