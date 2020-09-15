using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ModsComponentOffsets
    {
        public static readonly int HumanStats = 0x20;
        [FieldOffset(0x30)] public long UniqueName;
        [FieldOffset(0xA8)] public bool Identified;
        [FieldOffset(0xAC)] public int ItemRarity;
        [FieldOffset(0xB0)] public NativePtrArray implicitMods;
        [FieldOffset(0xC8)] public NativePtrArray explicitMods;
        [FieldOffset(0xE0)] public NativePtrArray enchantMods;
        [FieldOffset(0x1A8)] public NativePtrArray GetImplicitStats;
        [FieldOffset(0x1A8)] public NativePtrArray GetStats;
        [FieldOffset(0x1D8)] public NativePtrArray GetCraftedStats;
        [FieldOffset(0x1F0)] public NativePtrArray GetFracturedStats;
        [FieldOffset(0x46C)] public int ItemLevel;
        [FieldOffset(0x470)] public int RequiredLevel;
        [FieldOffset(0x474)] public byte IsUsable;
        [FieldOffset(0x475)] public byte IsMirrored;
    }
}
