using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PathfindingComponentOffsets
    {
        [FieldOffset(0x2C)] public Vector2i ClickToNextPosition;
        [FieldOffset(0x34)] public Vector2i WasInThisPosition;
        [FieldOffset(0x470)] public byte IsMoving;
        [FieldOffset(0x54C)] public Vector2i WantMoveToPosition;
        [FieldOffset(0x558)] public float StayTime;
    }
}
