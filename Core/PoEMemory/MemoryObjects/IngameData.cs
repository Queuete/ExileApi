using System;
using System.Collections.Generic;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using GameOffsets;
using GameOffsets.Native;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class IngameData : RemoteMemoryObject
    {
        private static readonly int EntityCountOffset = Extensions.GetOffset<IngameDataOffsets>(nameof(IngameDataOffsets.EntitiesCount));
        private readonly CachedValue<IngameDataOffsets> _IngameData;
        private readonly CachedValue<long> _EntitiesCount;
        private readonly CachedValue<ServerData> _ServerData;
        private readonly CachedValue<Entity> _LocalPlayer;
        private readonly Dictionary<GameStat, int> _MapStats = new Dictionary<GameStat, int>();
        private EntityList _EntityList { get; set; }
        private NativePtrArray _CacheStats { get; set; }

        public IngameData()
        {
            _IngameData = new AreaCache<IngameDataOffsets>(() => M.Read<IngameDataOffsets>(Address));
            _LocalPlayer = new AreaCache<Entity>(() => GetObject<Entity>(_IngameData.Value.LocalPlayer));
            _EntitiesCount = new FrameCache<long>(() => M.Read<long>(Address + EntityCountOffset));
            _ServerData = new AreaCache<ServerData>(() => GetObject<ServerData>(_IngameData.Value.ServerData));
        }

        public IngameDataOffsets DataStruct => _IngameData.Value;
        public long EntitiesCount => _EntitiesCount.Value;
        public int CurrentAreaLevel => _IngameData.Value.CurrentAreaLevel;
        public uint CurrentAreaHash => _IngameData.Value.CurrentAreaHash;
        public Entity LocalPlayer => _LocalPlayer.Value;
        public ServerData ServerData => _ServerData.Value;
        public long EntitiesTest => DataStruct.EntityList;
        public EntityList EntityList => _EntityList = _EntityList ?? GetObject<EntityList>(DataStruct.EntityList);
        private long LabDataPtr => _IngameData.Value.LabDataPtr;
        public LabyrinthData LabyrinthData => LabDataPtr == 0 ? null : GetObject<LabyrinthData>(LabDataPtr);
        public TerrainData Terrain => _IngameData.Value.Terrain;

        public Dictionary<GameStat, int> MapStats
        {
            get
            {
                if (_CacheStats.Equals(_IngameData.Value.MapStats)) return _MapStats;
                _MapStats.Clear();
                var statPtrStart = _IngameData.Value.MapStats.First;
                var statPtrEnd = _IngameData.Value.MapStats.Last;
                var totalStats = (int)(statPtrEnd - statPtrStart);

                if (totalStats / 8 > 200)
                    return null;

                var bytes = M.ReadMem(statPtrStart, totalStats);

                for (var i = 0; i < bytes.Length; i += 8)
                {
                    var key = BitConverter.ToInt32(bytes, i);
                    var value = BitConverter.ToInt32(bytes, i + 0x04);
                    _MapStats[(GameStat)key] = value;
                }

                _CacheStats = _IngameData.Value.MapStats;
                return _MapStats;
            }
        }

        public IList<PortalObject> TownPortals
        {
            get
            {
                return new List<PortalObject>();

                // TODO: Moved/Removed in 3.14.1c, shouldn't read until offsets updated anyway.
                //var statPtrStart = M.Read<long>(Address + 0x4B4);
                //var statPtrEnd = M.Read<long>(Address + 0x4BC);

                //return M.ReadStructsArray<PortalObject>(statPtrStart, statPtrEnd, PortalObject.StructSize, TheGame);
            }
        }

        public class PortalObject : RemoteMemoryObject
        {
            public const int StructSize = 0x38;
            public string PlayerOwner => NativeStringReader.ReadString(Address + 0x08, M);
            public WorldArea Area => TheGame.Files.WorldAreas.GetAreaByAreaId(M.Read<int>(Address + 0x50));

            public override string ToString()
            {
                return $"{PlayerOwner} => {Area.Name}";
            }
        }
    }
}
