using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.FilesInMemory.Atlas;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using GameOffsets;
using GameOffsets.Native;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class ServerPlayerData : RemoteMemoryObject
    {
        private readonly CachedValue<ServerPlayerDataOffsets> _CachedValue;

        public ServerPlayerData()
        {
            _CachedValue = new FrameCache<ServerPlayerDataOffsets>(() => M.Read<ServerPlayerDataOffsets>(Address));
        }
        public ServerPlayerDataOffsets ServerPlayerDataStruct => _CachedValue.Value;

        public CharacterClass Class => (CharacterClass) (ServerPlayerDataStruct.PlayerClass & 0x0F);
        public int Level => ServerPlayerDataStruct.CharacterLevel;
        public int PassiveRefundPointsLeft => ServerPlayerDataStruct.PassiveRefundPointsLeft;
        public int QuestPassiveSkillPoints => ServerPlayerDataStruct.QuestPassiveSkillPoints;
        public int FreePassiveSkillPointsLeft => ServerPlayerDataStruct.FreePassiveSkillPointsLeft;
        public int TotalAscendencyPoints => ServerPlayerDataStruct.TotalAscendencyPoints;
        public int SpentAscendencyPoints => ServerPlayerDataStruct.SpentAscendencyPoints;
        public NativePtrArray AllocatedPassivesIds => ServerPlayerDataStruct.PassiveSkillIds;
    }
    public class ServerData : RemoteMemoryObject
    {
        private readonly CachedValue<ServerDataOffsets> _CachedValue;
        private readonly CachedValue<ServerPlayerData> _PlayerData;
        private readonly List<Player> _NearestPlayers = new List<Player>();

        private const int MaxElementsToScan = 1024;

        public ServerData()
        {
            _CachedValue = new FrameCache<ServerDataOffsets>(() => M.Read<ServerDataOffsets>(Address + ServerDataOffsets.Skip));
            _PlayerData = new FrameCache<ServerPlayerData>(() => GetObject<ServerPlayerData>(_CachedValue.Value.PlayerRelatedData));
        }

        public ServerDataOffsets ServerDataStruct => _CachedValue.Value;
        public ServerPlayerData PlayerInformation => _PlayerData.Value;
        public ushort TradeChatChannel => ServerDataStruct.TradeChatChannel;
        public ushort GlobalChatChannel => ServerDataStruct.GlobalChatChannel;
        public int DialogDepth => ServerDataStruct.DialogDepth;
        public byte MonsterLevel => ServerDataStruct.MonsterLevel;
        public CharacterClass PlayerClass => PlayerInformation.Class;

        //if 51 - more than 50 monsters remaining (no exact number)
        //if 255 - not supported for current map (town or scenery map)
        public byte MonstersRemaining => ServerDataStruct.MonstersRemaining;
        public ushort CurrentSulphiteAmount => _CachedValue.Value.CurrentSulphiteAmount;
        public int CurrentAzuriteAmount => _CachedValue.Value.CurrentAzuriteAmount;

        public IList<Player> NearestPlayers
        {
            get
            {
                if (Address == 0)
                {
                    return _NearestPlayers;
                }

                const int structSize = 0x18;
                var first = ServerDataStruct.NearestPlayers.First;
                var last = ServerDataStruct.NearestPlayers.Last;

                if (first < 0 || last < 0 || (last - first) / structSize > 64)
                {
                    return _NearestPlayers;
                }

                _NearestPlayers.Clear();

                for (var playerAddress = first; playerAddress < last; playerAddress += structSize)
                {
                    _NearestPlayers.Add(ReadObject<Player>(playerAddress));
                }

                return _NearestPlayers;
            }
        }

        public int GetBeastCapturedAmount(BestiaryCapturableMonster monster)
        {
            return M.Read<int>(Address + ServerDataOffsets.BestiaryBeastsCapturedCounts + monster.Id * 4);
        }

        #region PlayerData

        public ushort LastActionId => ServerDataStruct.LastActionId;
        public int CharacterLevel => PlayerInformation.Level;
        public int PassiveRefundPointsLeft => PlayerInformation.PassiveRefundPointsLeft;
        public int FreePassiveSkillPointsLeft => PlayerInformation.FreePassiveSkillPointsLeft;
        public int QuestPassiveSkillPoints => PlayerInformation.QuestPassiveSkillPoints;
        public int TotalAscendencyPoints => PlayerInformation.TotalAscendencyPoints;
        public int SpentAscendencyPoints => PlayerInformation.SpentAscendencyPoints;
        public PartyAllocation PartyAllocationType => (PartyAllocation) ServerDataStruct.PartyAllocationType;
        public string League => ServerDataStruct.League.ToString(M);
        public PartyStatus PartyStatusType => (PartyStatus) ServerDataStruct.PartyStatusType;
        public bool IsInGame => NetworkState == NetworkStateE.Connected;
        public NetworkStateE NetworkState => (NetworkStateE) ServerDataStruct.NetworkState;
        public int Latency => ServerDataStruct.Latency;
        public string Guild => NativeStringReader.ReadString(ServerDataStruct.GuildNameAddress, M);
        public BetrayalData BetrayalData => GetObject<BetrayalData>(M.Read<long>(Address + 0x3C8, 0x718)); // TODO: 3.12.2

        public IList<ushort> SkillBarIds
        {
            get
            {
                if (Address == 0) return new List<ushort>();
                
                var skillBarIds = _CachedValue.Value.SkillBarIds;

                var res = new List<ushort>
                {
                    skillBarIds.SkillBar1,
                    skillBarIds.SkillBar2,
                    skillBarIds.SkillBar3,
                    skillBarIds.SkillBar4,
                    skillBarIds.SkillBar5,
                    skillBarIds.SkillBar6,
                    skillBarIds.SkillBar7,
                    skillBarIds.SkillBar8,
                    skillBarIds.SkillBar9,
                    skillBarIds.SkillBar10,
                    skillBarIds.SkillBar11,
                    skillBarIds.SkillBar12,
                    skillBarIds.SkillBar13
                };

                return res;
            }
        }

        public IList<ushort> PassiveSkillIds
        {
            get
            {
                var passiveSkillIds = new List<ushort>();

                if (Address == 0)
                {
                    return passiveSkillIds;
                }

                var first = PlayerInformation.AllocatedPassivesIds.First;
                var last = PlayerInformation.AllocatedPassivesIds.Last;
                var totalStats = (int) (last - first);
                
                if (totalStats < 0 || totalStats > 500)
                {
                    return passiveSkillIds;
                }

                var bytes = M.ReadMem(first, totalStats);
                for (var i = 0; i < bytes.Length; i += 2)
                {
                    var id = BitConverter.ToUInt16(bytes, i);
                    passiveSkillIds.Add(id);
                }

                return passiveSkillIds;
            }
        }

        #endregion

        #region Stash Tabs

        public IList<ServerStashTab> PlayerStashTabs => GetStashTabs(ServerDataStruct.PlayerStashTabs);
        public IList<ServerStashTab> GuildStashTabs =>  GetStashTabs(ServerDataStruct.GuildStashTabs);

        private IList<ServerStashTab> GetStashTabs(NativePtrArray tabs)
        {
            var first = tabs.First;
            var last = tabs.Last;

            if (first <= 0 || last <= 0)
            {
                return new List<ServerStashTab>();
            }
            
            var tabCount = (last - first) / ServerStashTab.StructSize;
            
            if (tabCount <= 0 || tabCount > MaxElementsToScan) 
            {
                return new List<ServerStashTab>();
            }

            return M.ReadStructsArray<ServerStashTab>(first, last, ServerStashTab.StructSize, TheGame).ToList();
        }

        #endregion

        #region Inventories

        public IList<InventoryHolder> PlayerInventories => GetInventory(ServerDataStruct.PlayerInventories);
        public IList<InventoryHolder> NPCInventories => GetInventory(ServerDataStruct.NPCInventories);
        public IList<InventoryHolder> GuildInventories => GetInventory(ServerDataStruct.GuildInventories);

        private IList<InventoryHolder> GetInventory(NativePtrArray inventories)
        {
            var first = inventories.First;
            var last = inventories.Last;

            if (first <= 0 || last <= 0)
            {
                return new List<InventoryHolder>();
            }

            var inventoryCount = (last - first) / InventoryHolder.StructSize;

            if (inventoryCount < 0 || inventoryCount > MaxElementsToScan)
            {
                return new List<InventoryHolder>();
            }

            return M.ReadStructsArray<InventoryHolder>(first, last, InventoryHolder.StructSize, this).ToList();
        }

        #region Inventory Util functions

        public ServerInventory GetPlayerInventoryBySlot(InventorySlotE slot)
        {
            foreach (var inventory in PlayerInventories)
            {
                if (inventory.Inventory.InventSlot == slot)
                    return inventory.Inventory;
            }

            return null;
        }

        public ServerInventory GetPlayerInventoryByType(InventoryTypeE type)
        {
            foreach (var inventory in PlayerInventories)
            {
                if (inventory.Inventory.InventType == type)
                    return inventory.Inventory;
            }

            return null;
        }

        public ServerInventory GetPlayerInventoryBySlotAndType(InventoryTypeE type, InventorySlotE slot)
        {
            foreach (var inventory in PlayerInventories)
            {
                if (inventory.Inventory.InventType == type && inventory.Inventory.InventSlot == slot)
                    return inventory.Inventory;
            }

            return null;
        }

        #endregion

        #endregion

        #region Atlas

        public byte SocketedWatchstones => ServerDataStruct.SocketedWatchstones;
        public List<WorldArea> CompletedAreas => GetAreas(ServerDataStruct.CompletedMapsList).ToList();
        public List<WorldArea> BonusCompletedAreas => GetAreas(ServerDataStruct.BonusCompletedAreasList).ToList();
        public List<WorldArea> MavenCompletedAreas => GetAreas(ServerDataStruct.MavenMapsList).ToList();

        #region Features removed from 3.17 patch

        [Obsolete("Awakened tracking was removed with the 3.17 Atlas rework.", false)]
        public List<WorldArea> AwakenedCompletedAreas => new List<WorldArea>();
        [Obsolete("Watchtower map tracking was removed with the 3.17 Atlas rework.", false)]
        public Dictionary<AtlasRegionE, WorldArea> WatchtowerMaps => new Dictionary<AtlasRegionE, WorldArea>();

        #endregion


        #region Features removed from 3.9 patch
        
        [ObsoleteAttribute("Shaped maps were removed with the 3.9.0 Atlas Rework. You should not be using this.", false)]
        public IList<WorldArea> ShapedMaps => new List<WorldArea>();
        [ObsoleteAttribute("Elder Guardian Areas were removed with the 3.9.0 Atlas Rework. You should not be using this.", false)]
        public IList<WorldArea> ElderGuardiansAreas => new List<WorldArea>();
        [ObsoleteAttribute("Masters Areas were removed with the 3.9.0 Atlas Rework. You should not be using this.", false)]
        public IList<WorldArea> MasterAreas => new List<WorldArea>();
        [ObsoleteAttribute("Elder Influenced Areas were removed with the 3.9.0 Atlas Rework. You should not be using this.", false)]
        public IList<WorldArea> ShaperElderAreas => new List<WorldArea>();
        
        #endregion

        private IList<WorldArea> GetAreas(long listAddress)
        {
            var worldAreas = new List<WorldArea>();
            var readCount = 0;

            if (Address == 0 || listAddress == 0) return worldAreas;

            var lastAddress = M.Read<long>(listAddress);

            if (lastAddress == 0) return worldAreas;

            var currentAddress = lastAddress;

            for (var nextAddress = M.Read<long>(currentAddress);
                nextAddress != lastAddress && nextAddress != 0;
                nextAddress = M.Read<long>(currentAddress))
            {
                var areaAddress = M.Read<long>(currentAddress + 0x10);
                var byAddress = TheGame.Files.WorldAreas.GetByAddress(areaAddress);
                if (byAddress != null) worldAreas.Add(byAddress);
                currentAddress = nextAddress;

                // Read count shouldn't be higher than the number of maps
                if (++readCount == 256) return new List<WorldArea>();
            }

            return worldAreas;
        }

        [Obsolete("Regions were removed with the 3.17 Atlas rework.", false)]
        public byte GetAtlasRegionUpgradesByRegion(int regionId)
        {
            return SocketedWatchstones;
        }

        [Obsolete("Regions were removed with the 3.17 Atlas rework.", false)]
        public byte GetAtlasRegionUpgradesByRegion(AtlasRegion region)
        {
            return SocketedWatchstones;
        }

        #endregion
    }
}
