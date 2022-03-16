using System.Runtime.InteropServices;
using GameOffsets.Native;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct IngameDataOffsets
    {
        [FieldOffset(0xB0)] public byte CurrentAreaLevel;
        [FieldOffset(0x114)] public uint CurrentAreaHash;
        [FieldOffset(0x128)] public NativePtrArray MapStats;
        [FieldOffset(0x198)] public long LabDataPtr;
        [FieldOffset(0x680)] public long ServerData;
        [FieldOffset(0x688)] public long LocalPlayer;
        [FieldOffset(0x738)] public long EntityList;
        [FieldOffset(0x740)] public long EntitiesCount;
        [FieldOffset(0x8D0)] public TerrainStruct Terrain;  ///3.17.3  =>8E8 from Game.IngameState.Data
    }
}
