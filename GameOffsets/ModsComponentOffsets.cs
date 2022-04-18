using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public readonly struct ModsComponentDetailsOffsets
    {
        [FieldOffset(0x008)] public readonly NativePtrArray ImplicitStatsArray;
        [FieldOffset(0x048)] public readonly NativePtrArray EnchantedStatsArray;
        [FieldOffset(0x088)] public readonly NativePtrArray ScourgeStatsArray;
        [FieldOffset(0x0C8)] public readonly NativePtrArray ExplicitStatsArray;
        [FieldOffset(0x108)] public readonly NativePtrArray CraftedStatsArray;
        [FieldOffset(0x148)] public readonly NativePtrArray FracturedStatsArray;

        // 3.16 Offsets
        //[FieldOffset(0x008)] public readonly NativePtrArray ImplicitStatsArray;
        //[FieldOffset(0x048)] public readonly NativePtrArray EnchantedStatsArray;
        //[FieldOffset(0x088)] public readonly NativePtrArray ScourgeStatsArray;
        //[FieldOffset(0x0C8)] public readonly NativePtrArray ExplicitStatsArray;
        //[FieldOffset(0x108)] public readonly NativePtrArray CraftedStatsArray;
        //[FieldOffset(0x148)] public readonly NativePtrArray FracturedStatsArray;
        //[FieldOffset(0x190)] public readonly NativePtrArray ImplicitStatsArray2;
        //[FieldOffset(0x1D0)] public readonly NativePtrArray EnchantedStatsArray2;
        //[FieldOffset(0x210)] public readonly NativePtrArray ScourgeStatsArray2;
        //[FieldOffset(0x250)] public readonly NativePtrArray ExplicitStatsArray2;
        //[FieldOffset(0x290)] public readonly NativePtrArray CraftedStatsArray2;
        //[FieldOffset(0x2D0)] public readonly NativePtrArray FracturedStatsArray2;
        //[FieldOffset(0x318)] public readonly NativePtrArray Unknown0;
        //[FieldOffset(0x358)] public readonly NativePtrArray Unknown1;
        //[FieldOffset(0x430)] public readonly NativePtrArray Unknown2;
        //[FieldOffset(0x478)] public readonly NativePtrArray ImplicitDescriptions;
        //[FieldOffset(0x4B8)] public readonly NativePtrArray EnchantDescriptions;
        //[FieldOffset(0x500)] public readonly NativePtrArray BasicPrefixDescriptions;
        //[FieldOffset(0x518)] public readonly NativePtrArray CraftedPrefixDescriptions;
        //[FieldOffset(0x530)] public readonly NativePtrArray FracturedPrefixDescriptions;
        //[FieldOffset(0x548)] public readonly NativePtrArray BasicSuffixDescriptions;
        //[FieldOffset(0x560)] public readonly NativePtrArray CraftedSuffixDescriptions;
        //[FieldOffset(0x578)] public readonly NativePtrArray FracturedSuffixDescriptions;
        //[FieldOffset(0x590)] public readonly NativePtrArray ScourgeDescriptions;
    }
    
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ModsComponentOffsets
    {
        [FieldOffset(0x030)] public readonly NativePtrArray UniqueName;
        [FieldOffset(0x0A8)] public readonly bool Identified;
        [FieldOffset(0x0AC)] public readonly int ItemRarity;
        [FieldOffset(0x0B8)] public readonly NativePtrArray ImplicitModsArray;
        [FieldOffset(0x0D0)] public readonly NativePtrArray ExplicitModsArray;
        [FieldOffset(0x0E8)] public readonly NativePtrArray EnchantedModsArray;
        [FieldOffset(0x100)] public readonly NativePtrArray ScourgeModsArray;
        [FieldOffset(0x1F0)] public readonly long ModsComponentDetailsKey;
        [FieldOffset(0x220)] public readonly int ItemLevel;
        [FieldOffset(0x224)] public readonly int RequiredLevel;
        [FieldOffset(0x228)] public readonly long IncubatorKey;
        [FieldOffset(0x238)] public readonly short IncubatorKillCount;
        [FieldOffset(0x23D)] public readonly byte IsMirrored;
        [FieldOffset(0x23E)] public readonly byte IsSplit;
        [FieldOffset(0x23F)] public readonly byte IsUsable;
        [FieldOffset(0x241)] public readonly byte IsSynthesised;

        public const int ItemModRecordSize = 0x38;
        public const int NameOffset = 0x04;
        public const int NameRecordSize = 0x10;
        public const int StatRecordSize = 0x20;

        // 3.16 Layout
        //[FieldOffset(0x030)] public readonly NativePtrArray UniqueName;
        //[FieldOffset(0x0A8)] public readonly bool Identified;
        //[FieldOffset(0x0AC)] public readonly int ItemRarity;
        //[FieldOffset(0x0B8)] public readonly NativePtrArray ImplicitModsArray;
        //[FieldOffset(0x0D0)] public readonly NativePtrArray ExplicitModsArray;
        //[FieldOffset(0x0E8)] public readonly NativePtrArray EnchantedModsArray;
        //[FieldOffset(0x100)] public readonly NativePtrArray ScourgeModsArray;
        //[FieldOffset(0x160)] public readonly NativePtrArray StatValuesArray;
        //[FieldOffset(0x1D8)] public readonly long AlternateQualityTypeKey;
        //[FieldOffset(0x1E0)] public readonly long AlternateQualityTypeFileKey;
        //[FieldOffset(0x1E8)] public readonly int AlternateQualityAmount;
        //[FieldOffset(0x1F0)] public readonly long ModsDetailsKey;
        //[FieldOffset(0x1F8)] public readonly long StatDescriptionsFile1;
        //[FieldOffset(0x200)] public readonly long StatDescriptionsFile2;
        //[FieldOffset(0x208)] public readonly NativePtrArray Tags;
        //[FieldOffset(0x220)] public readonly int ItemLevel;
        //[FieldOffset(0x224)] public readonly int RequiredLevel;
        //[FieldOffset(0x228)] public readonly long IncubatorKey;
        //[FieldOffset(0x230)] public readonly long IncubatorFile;
        //[FieldOffset(0x238)] public readonly short IncubatorKillCount;
        //[FieldOffset(ox23A)] public readonly short IncubatorItemLevel;
        //[FieldOffset(0x23D)] public readonly byte IsMirrored;
        //[FieldOffset(0x23E)] public readonly byte IsSplit;
        //[FieldOffset(0x23F)] public readonly byte IsUsable;
        //[FieldOffset(0x241)] public readonly byte IsSynthesised;
    }
}
