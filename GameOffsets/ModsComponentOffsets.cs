using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ModsComponentOffsets
    {
        public static readonly int ItemModRecordSize = 0x28;
        public static readonly int NameRecordSize = 0x10;
        public static readonly int NameOffset = 0x04;
        public static readonly int StatRecordSize = 0x20;

        [FieldOffset(0x30)] public NativePtrArray UniqueName;
        [FieldOffset(0xA8)] public bool Identified;
        [FieldOffset(0xAC)] public int ItemRarity;
        [FieldOffset(0xB0)] public NativePtrArray ImplicitModsArray;
        [FieldOffset(0xD0)] public NativePtrArray ExplicitModsArray;
        [FieldOffset(0xE0)] public NativePtrArray EnchantedModsArray;
        [FieldOffset(0x1C0)] public NativePtrArray ImplicitStatsArray;
        [FieldOffset(0x1D8)] public NativePtrArray EnchantedStatsArray;
        [FieldOffset(0x1F0)] public NativePtrArray ExplicitStatsArray;
        [FieldOffset(0x208)] public NativePtrArray CraftedStatsArray;
        [FieldOffset(0x220)] public NativePtrArray FracturedStatsArray;
        [FieldOffset(0x489)] public byte IsMirrored;
        [FieldOffset(0x48A)] public byte IsSplit;
        [FieldOffset(0x48C)] public int ItemLevel;
        [FieldOffset(0x490)] public int RequiredLevel;
        [FieldOffset(0x498)] public long IncubatorKey;
        [FieldOffset(0x4A8)] public short IncubatorKillCount;
        [FieldOffset(0x4AC)] public byte IsUsable;
    }
}
