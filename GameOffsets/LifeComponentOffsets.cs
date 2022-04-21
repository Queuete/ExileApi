using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct LifeComponentOffsets
    {
        [FieldOffset(0x8)] public long Owner;

        [FieldOffset(0x198)] public float ManaRegen;
        [FieldOffset(0x19C)] public int MaxMana;
        [FieldOffset(0x1A0)] public int CurMana;
        [FieldOffset(0x1A4)] public int ReservedFlatMana;
        [FieldOffset(0x1A8)] public int ReservedPercentMana;

        [FieldOffset(0x1D4)] public int MaxES;
        [FieldOffset(0x1D8)] public int CurES;
        
        [FieldOffset(0x230)] public float Regen;
        [FieldOffset(0x234)] public int MaxHP;
        [FieldOffset(0x238)] public int CurHP;
        [FieldOffset(0x23C)] public int ReservedFlatHP;
        [FieldOffset(0x240)] public int ReservedPercentHP;

    }
}
