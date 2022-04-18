using System.Runtime.InteropServices;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BaseInfoOffsets
    {
        [FieldOffset(0x10)] public byte ItemCellSizeX;
        [FieldOffset(0x11)] public byte ItemCellSizeY;
        [FieldOffset(0x18)] public long NameKey;
        //[FieldOffset(0x38)] public long BaseItemsTypeKey;
        //[FieldOffset(0x48)] public NativePtrArray TagKeys;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BaseComponentOffsets
    {
        [FieldOffset(0x10)] public long BaseInfoKey;
        //[FieldOffset(0x18)] public long ItemVisualIdentityKey;
        //[FieldOffset(0x38)] public long FlavourTextKey;
        [FieldOffset(0x60)] public long PublicPricePtr;
        [FieldOffset(0xC6)] public byte InfluenceFlag;
        [FieldOffset(0xC7)] public byte isCorrupted;
        [FieldOffset(0xC8)] public int UnspentAbsorbedCorruption;
        [FieldOffset(0xCC)] public int ScourgedTier;
    }
}
