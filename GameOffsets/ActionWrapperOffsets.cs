using System.Runtime.InteropServices;
using SharpDX;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ActionWrapperOffsets
    {
        [FieldOffset(0xD0)] public long Target;
        [FieldOffset(0x150)] public long Skill;
        [FieldOffset(0x170)] public Vector2 Destination;
    }
}
