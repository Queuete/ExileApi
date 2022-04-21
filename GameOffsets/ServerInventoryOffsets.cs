using System.Runtime.InteropServices;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ServerInventoryOffsets
    {
        [FieldOffset(0x138)] public byte InventType;
        [FieldOffset(0x13C)] public byte InventSlot;
        [FieldOffset(0x140)] public byte IsRequested;
        [FieldOffset(0x144)] public int Columns;
        [FieldOffset(0x148)] public int Rows;
        [FieldOffset(0x168)] public long InventoryItemsPtr;
        [FieldOffset(0x180)] public long InventorySlotItemsPtr;
        [FieldOffset(0x188)] public long CountItems;
        [FieldOffset(0x188)] public int TotalItemsCount;
        [FieldOffset(0x1C8)] public int ServerRequestCounter;
        [FieldOffset(0x1E8)] public long Hash;
    }
}
