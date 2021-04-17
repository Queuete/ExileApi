using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct LifeComponentOffsets
    {
        [FieldOffset(0x8)] public long Owner;
        [FieldOffset(0x158)] public NativePtrArray Buffs;

        [FieldOffset(0x1B0)] public float ManaRegen;
        [FieldOffset(0x1B4)] public int MaxMana;
        [FieldOffset(0x1B8)] public int CurMana;
        [FieldOffset(0x1C0)] public int ReservedPercentMana;

        [FieldOffset(0x1EC)] public int MaxES;
        [FieldOffset(0x1F0)] public int CurES;
        
        [FieldOffset(0x248)] public float Regen;
        [FieldOffset(0x24C)] public int MaxHP;
        [FieldOffset(0x250)] public int CurHP;
        [FieldOffset(0x258)] public int ReservedPercentHP;
    }
}
