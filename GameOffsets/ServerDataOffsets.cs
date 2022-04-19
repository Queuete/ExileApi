using System.Runtime.InteropServices;
using System.Security.Policy;
using GameOffsets.Native;

namespace GameOffsets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SkillBarIdsStruct
    {
        public readonly ushort SkillBar1;
        public readonly ushort SkillBar2;
        public readonly ushort SkillBar3;
        public readonly ushort SkillBar4;
        public readonly ushort SkillBar5;
        public readonly ushort SkillBar6;
        public readonly ushort SkillBar7;
        public readonly ushort SkillBar8;
        public readonly ushort SkillBar9;
        public readonly ushort SkillBar10;
        public readonly ushort SkillBar11;
        public readonly ushort SkillBar12;
        public readonly ushort SkillBar13;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AtlasMissionAmounts
    {
        public readonly ushort Einhar;
        public readonly ushort Alva;
        public readonly ushort Unknown;
        public readonly ushort Niko;
        public readonly ushort Jun;
        public readonly ushort Kirac;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ServerPlayerDataOffsets
    {
        [FieldOffset(0x180)] public readonly NativePtrArray PassiveSkillIds;
        [FieldOffset(0x238)] public readonly byte PlayerClass;
        [FieldOffset(0x23C)] public readonly int CharacterLevel;
        [FieldOffset(0x240)] public readonly int PassiveRefundPointsLeft;
        [FieldOffset(0x244)] public readonly int QuestPassiveSkillPoints;
        [FieldOffset(0x248)] public readonly int FreePassiveSkillPointsLeft;
        [FieldOffset(0x24C)] public readonly int TotalAscendencyPoints;
        [FieldOffset(0x250)] public readonly int SpentAscendencyPoints;

        // 3.17 Layout
        // [FieldOffset(0x180)] public readonly NativePtrArray PassiveSkillIds;
        // [FieldOffset(0x198)] public readonly NativePtrArray PassiveBasicJewelNodeIds;
        // [FieldOffset(0x1C8)] public readonly NativePtrArray PassiveClusterJewelNodeIds;
        // [FieldOffset(0x1E0)] public readonly NativePtrArray PassiveMasteryNodeIds;
        // [FieldOffset(0x238)] public readonly byte PlayerClass;
        // [FieldOffset(0x23C)] public readonly int CharacterLevel;
        // [FieldOffset(0x240)] public readonly int PassiveRefundPointsLeft;
        // [FieldOffset(0x244)] public readonly int QuestPassiveSkillPoints;
        // [FieldOffset(0x248)] public readonly int FreePassiveSkillPointsLeft;
        // [FieldOffset(0x24C)] public readonly int TotalAscendencyPoints;
        // [FieldOffset(0x250)] public readonly int SpentAscendencyPoints;
}

  [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ServerDataOffsets
    {
        public const int Skip = 0x8000;

        [FieldOffset(0x89A0 - Skip)] public readonly long PlayerRelatedData;
        [FieldOffset(0x89B8 - Skip)] public readonly byte NetworkState;
        [FieldOffset(0x89D0 - Skip)] public readonly NativeStringU League;
        [FieldOffset(0x8A50 - Skip)] public readonly int TimeInGame;
        [FieldOffset(0x8A70 - Skip)] public readonly int Latency;
        [FieldOffset(0x8A78 - Skip)] public readonly NativePtrArray PlayerStashTabs;
        [FieldOffset(0x8A90 - Skip)] public readonly NativePtrArray GuildStashTabs;
        [FieldOffset(0x8C48 - Skip)] public readonly byte PartyStatusType;
        [FieldOffset(0x8C68 - Skip)] public readonly byte PartyAllocationType;
        [FieldOffset(0x8CD0 - Skip)] public readonly long GuildNameAddress;
        [FieldOffset(0x8CE0 - Skip)] public readonly SkillBarIdsStruct SkillBarIds;
        [FieldOffset(0x8D40 - Skip)] public readonly NativePtrArray NearestPlayers;
        [FieldOffset(0x8EB8 - Skip)] public readonly NativePtrArray PlayerInventories;
        [FieldOffset(0x9008 - Skip)] public readonly NativePtrArray NPCInventories;
        [FieldOffset(0x9158 - Skip)] public readonly NativePtrArray GuildInventories;
        [FieldOffset(0x92C8 - Skip)] public readonly ushort TradeChatChannel;
        [FieldOffset(0x9CD0 - Skip)] public readonly ushort GlobalChatChannel;
        [FieldOffset(0x9370 - Skip)] public readonly ushort LastActionId;
        [FieldOffset(0x93C8 - Skip)] public readonly long MavenMapsList;
        [FieldOffset(0x93D0 - Skip)] public readonly long MavenMapsCount;
        [FieldOffset(0x93D8 - Skip)] public readonly long MavenMapsArray;
        [FieldOffset(0x9408 - Skip)] public readonly long CompletedMapsList;
        [FieldOffset(0x9410 - Skip)] public readonly long CompletedMapsCount;
        [FieldOffset(0x9418 - Skip)] public readonly long CompletedMapsArray;
        [FieldOffset(0x9448 - Skip)] public readonly long BonusCompletedAreasList;
        [FieldOffset(0x9450 - Skip)] public readonly long BonusCompletedAreasCount;
        [FieldOffset(0x9458 - Skip)] public readonly long BonusCompletedAreasArray;
        [FieldOffset(0x9488 - Skip)] public readonly long FavouredMapsArray;
        [FieldOffset(0x94A2 - Skip)] public readonly AtlasMissionAmounts LowTierAtlasMissionAmounts;
        [FieldOffset(0x94B0 - Skip)] public readonly AtlasMissionAmounts MidTierAtlasMissionAmounts;
        [FieldOffset(0x94BE - Skip)] public readonly AtlasMissionAmounts HighTierAtlasMissionAmounts;
        [FieldOffset(0x94CC - Skip)] public readonly byte SocketedWatchstones;
        [FieldOffset(0x8A88 - Skip)] public readonly long BestiaryCapturedMonsterList; // TODO
        [FieldOffset(0x9E28 - Skip)] public readonly int DialogDepth;
        [FieldOffset(0x9E2C - Skip)] public readonly byte MonsterLevel;
        [FieldOffset(0x9E2D - Skip)] public readonly byte MonstersRemaining;
        [FieldOffset(0x9EDA - Skip)] public readonly int CurrentAzuriteAmount;
        [FieldOffset(0x9EEA - Skip)] public readonly ushort CurrentSulphiteAmount;
        
        public const int BestiaryBeastsCapturedCounts = 0x8AE8;
    
        // 3.17 Layout
        // [FieldOffset(0x89A0 - Skip)] public readonly long PlayerRelatedData;
        // [FieldOffset(0x89B8 - Skip)] public readonly byte NetworkState;
        // [FieldOffset(0x89D0 - Skip)] public readonly NativeStringU League;
        // [FieldOffset(0x8A50 - Skip)] public readonly int TimeInGame;
        // [FieldOffset(0x8A70 - Skip)] public readonly int Latency;
        // [FieldOffset(0x8A78 - Skip)] public readonly NativePtrArray PlayerStashTabs;
        // [FieldOffset(0x8A90 - Skip)] public readonly NativePtrArray GuildStashTabs;
        // [FieldOffset(0x8B60 - Skip)] public readonly long FriendsListMap;
        // [FieldOffset(0x8B78 - Skip)] public readonly long FriendList;
        // [FieldOffset(0x8B88 - Skip)] public readonly NativePtrArray FriendListArray;
        // [FieldOffset(0x8BC8 - Skip)] public readonly NativePtrArray PendingInvites;
        // [FieldOffset(0x8C08 - Skip)] public readonly long PartyPublicDescription;
        // [FieldOffset(0x8C28 - Skip)] public readonly long PartyLeaderKey;
        // [FieldOffset(0x8C48 - Skip)] public readonly byte PartyStatusType;
        // [FieldOffset(0x8C50 - Skip)] public readonly NativePtrArray CurrentParty;
        // [FieldOffset(0x8C68 - Skip)] public readonly byte PartyAllocationType;
        // [FieldOffset(0x8C69 - Skip)] public readonly bool PartyDownscaleDisabled;
        // [FieldOffset(0x8C80 - Skip)] public readonly long GuildList;
        // [FieldOffset(0x8C90 - Skip)] public readonly NativePtrArray GuildArray;
        // [FieldOffset(0x8CD0 - Skip)] public readonly long GuildNameAddress;
        // [FieldOffset(0x8CE0 - Skip)] public readonly SkillBarIdsStruct SkillBarIds;
        // [FieldOffset(0x8CFC - Skip)] public readonly float MouseWorldPosX;
        // [FieldOffset(0x8D00 - Skip)] public readonly float MouseWorldPosY;
        // [FieldOffset(0x8D10 - Skip)] public readonly long SkillBarUiRootKey;
        // [FieldOffset(0x8D40 - Skip)] public readonly NativePtrArray NearestPlayers;
        // [FieldOffset(0x8D70 - Skip)] public readonly NativePtrArray MinimapIcons;
        // [FieldOffset(0x8D90 - Skip)] public readonly NativePtrArray LocalPlayer;
        // [FieldOffset(0x8EB8 - Skip)] public readonly NativePtrArray PlayerInventories;
        // [FieldOffset(0x9008 - Skip)] public readonly NativePtrArray NPCInventories;
        // [FieldOffset(0x9158 - Skip)] public readonly NativePtrArray GuildInventories;
        // [FieldOffset(0x9178 - Skip)] public readonly NativeStringU SocialMessage;
        // [FieldOffset(0x92C8 - Skip)] public readonly ushort TradeChatChannel;
        // [FieldOffset(0x9CD0 - Skip)] public readonly ushort GlobalChatChannel;
        // [FieldOffset(0x9370 - Skip)] public readonly ushort LastActionId;
        // [FieldOffset(0x93A8 - Skip)] public readonly long InstanceLeagueInfo;
        // [FieldOffset(0x93C8 - Skip)] public readonly long MavenMapsList;
        // [FieldOffset(0x93D0 - Skip)] public readonly long MavenMapsCount;
        // [FieldOffset(0x93D8 - Skip)] public readonly long MavenMapsArray;
        // [FieldOffset(0x9408 - Skip)] public readonly long CompletedMapsList;
        // [FieldOffset(0x9410 - Skip)] public readonly long CompletedMapsCount;
        // [FieldOffset(0x9418 - Skip)] public readonly long CompletedMapsArray;
        // [FieldOffset(0x9448 - Skip)] public readonly long BonusCompletedAreasList;
        // [FieldOffset(0x9450 - Skip)] public readonly long BonusCompletedAreasCount;
        // [FieldOffset(0x9458 - Skip)] public readonly long BonusCompletedAreasArray;
        // [FieldOffset(0x9488 - Skip)] public readonly long FavouredMapsArray;
        // [FieldOffset(0x94A2 - Skip)] public readonly AtlasMissionAmounts LowTierAtlasMissionAmounts;
        // [FieldOffset(0x94B0 - Skip)] public readonly AtlasMissionAmounts MidTierAtlasMissionAmounts;
        // [FieldOffset(0x94BE - Skip)] public readonly AtlasMissionAmounts HighTierAtlasMissionAmounts;
        // [FieldOffset(0x94CC - Skip)] public readonly byte SocketedWatchstones;
        // [FieldOffset(0x8A88 - Skip)] public readonly long BestiaryCapturedMonsterList;
        // [FieldOffset(0x9DF8 - Skip)] public readonly long LeftIncursionArchitectKey;
        // [FieldOffset(0x9E08 - Skip)] public readonly long RightIncursionArchitectKey;
        // [FieldOffset(0x9E28 - Skip)] public readonly int DialogDepth;
        // [FieldOffset(0x9E2C - Skip)] public readonly byte MonsterLevel;
        // [FieldOffset(0x9E2D - Skip)] public readonly byte MonstersRemaining;
        // [FieldOffset(0x9EDA - Skip)] public readonly int CurrentAzuriteAmount;
        // [FieldOffset(0x9EEA - Skip)] public readonly ushort CurrentSulphiteAmount;
        // [FieldOffset(0x9F38 - Skip)] public readonly long DeliriumRewardInfo;
        // [FieldOffset(0x9F50 - Skip)] public readonly long HeistBlueprintInfo;
        // [FieldOffset(0x9F68 - Skip)] public readonly long HeistContractInfo;
        // [FieldOffset(0x9FB8 - Skip)] public readonly int CrucibleExperience;

        // public const int AtlasRegionUpgrades = 0x94CC;
        // public const int BestiaryBeastsCapturedCounts = 0x8AE8;
    }
}
