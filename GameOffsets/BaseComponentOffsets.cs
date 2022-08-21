using System.Runtime.InteropServices;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BaseInfoOffsets
    {
        [FieldOffset(0x10)] public byte ItemCellSizeX;
        [FieldOffset(0x11)] public byte ItemCellSizeY;
        [FieldOffset(0x30)] public long NameKey;
        [FieldOffset(0x78) public string ItemDescription;
        [FieldOffset(0x80) public string PtrEntityPath; //offset 0 from this leads back to "Metadata/..."
        [FieldOffset(0x88) public string ItemType; //offset 0 from this
        [FieldOffset(0x90) public long BaseItemTypesPtr; 
        [FieldOffset(0x98) public string XBoxControllerItemDescription;
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
