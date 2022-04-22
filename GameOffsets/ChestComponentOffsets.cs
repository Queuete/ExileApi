using System.Runtime.InteropServices;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ChestComponentOffsets
    {
        [FieldOffset(0x158)] public long StrongboxData;
        [FieldOffset(0x160)] public bool IsOpened;
        [FieldOffset(0x161)] public bool IsLocked;
        [FieldOffset(0x164)] public readonly byte quality;
        [FieldOffset(0x1A0)] public bool IsStrongbox;
    }
}
