using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct IngameDataOffsets
    {
        [FieldOffset(0x78)] public long CurrentArea;
        [FieldOffset(0x98)] public byte CurrentAreaLevel;
        [FieldOffset(0xFC)] public uint CurrentAreaHash;
        [FieldOffset(0xF8)] public NativePtrArray MapStats;
        [FieldOffset(0x470)] public long LocalPlayer;
        [FieldOffset(0x11C)] public long LabDataPtr;
        [FieldOffset(0x518)] public long EntityList;
        [FieldOffset(0x520)] public long EntitiesCount;
        [FieldOffset(0x698 + 0x8)] public TerrainData Terrain; //3.14=>3.14.C
    }
}
