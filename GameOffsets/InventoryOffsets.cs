using System.Runtime.InteropServices;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InventoryOffsets
    {
        [FieldOffset(0x228)] public int MoveItemHoverState;
        [FieldOffset(0x230)] public long HoverItem;
        [FieldOffset(0x238)] public int XFake;
        [FieldOffset(0x23C)] public int YFake;
        [FieldOffset(0x240)] public int XReal;
        [FieldOffset(0x244)] public int YReal;
        [FieldOffset(0x250)] public int CursorInInventory;
        [FieldOffset(0x3D8)] public long ItemCount;
        [FieldOffset(0x4C0)] public int TotalBoxesInInventoryRow;
    }
}
