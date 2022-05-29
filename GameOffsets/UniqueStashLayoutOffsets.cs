namespace GameOffsets
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct UniqueStashLayoutOffsets
    {
        [FieldOffset(0x00)] public long UniqueNameKey;
        [FieldOffset(0x10)] public long VisualIdentityKey;

        // 3.18.0c Layout
        // [FieldOffset(0x00)] public StdFile UniqueName;
        // [FieldOffset(0x10)] public StdFile ItemVisualIdentity;
        // [FieldOffset(0x20)] public StdFile UniqueStashType;
        // [FieldOffset(0x30)] public StdTuple2<int> InventoryPosition;
        // [FieldOffset(0x38)] public StdTuple2<int> ItemDimensions;
        // [FieldOffset(0x40)] public bool Flag0;
        // [FieldOffset(0x41)] public bool ShowIfEmpty;
        // [FieldOffset(0x42)] public long AlternateVersion;
        // [FieldOffset(0x4A)] public long BaseVersion;
        // [FieldOffset(0x52)] public bool IsAlternateArt;
    }
}

