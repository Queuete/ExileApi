using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct IngameDataOffsets
    {
        [FieldOffset(0x0A0)] public byte CurrentAreaLevel;
        [FieldOffset(0x104)] public uint CurrentAreaHash;
        [FieldOffset(0x118)] public NativePtrArray MapStats;
        [FieldOffset(0x188)] public long LabDataPtr;
        [FieldOffset(0x670)] public long ServerData;
        [FieldOffset(0x678)] public long LocalPlayer;
        [FieldOffset(0x728)] public long EntityList;
        [FieldOffset(0x730)] public long EntitiesCount;
        [FieldOffset(0x8C0)] public TerrainData Terrain;
    }
}
