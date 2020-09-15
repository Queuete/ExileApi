using System.Runtime.InteropServices;
using SharpDX;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct CameraOffsets
    {
        [FieldOffset(0x8)] public int Width;
        [FieldOffset(0xC)] public int Height;
        [FieldOffset(0x1C4)] public float ZFar;

        //First value is changing when we change the screen size (ratio)
        //4 bytes before the matrix doesn't change
        [FieldOffset(0x80)] public Matrix MatrixBytes;
        [FieldOffset(0xF0)] public Vector3 Position;
    }
}
