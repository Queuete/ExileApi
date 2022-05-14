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
        [FieldOffset(0x6F0)] public long ServerData;
        [FieldOffset(0x6F8)] public long LocalPlayer;
        [FieldOffset(0x7A8)] public long EntityList;
        [FieldOffset(0x7B0)] public long EntitiesCount;
        [FieldOffset(0x940)] public TerrainData Terrain;
    }
}
