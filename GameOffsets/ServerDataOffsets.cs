namespace GameOffsets
{
    using System.Runtime.InteropServices;
    using GameOffsets.Native;

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

        [FieldOffset(0x8D20 - Skip)] public readonly long PlayerRelatedData;
        [FieldOffset(0x8D38 - Skip)] public readonly byte NetworkState;
        [FieldOffset(0x8D50 - Skip)] public readonly NativeStringU League;
        [FieldOffset(0x8D88 - Skip)] public readonly byte PartyAllocationType;
        [FieldOffset(0x8DD0 - Skip)] public readonly int TimeInGame;
        [FieldOffset(0x8DF0 - Skip)] public readonly int Latency;
        [FieldOffset(0x8DF8 - Skip)] public readonly NativePtrArray PlayerStashTabs;
        [FieldOffset(0x8E10 - Skip)] public readonly NativePtrArray GuildStashTabs;
        [FieldOffset(0x8FC8 - Skip)] public readonly byte PartyStatusType;
        [FieldOffset(0x9050 - Skip)] public readonly long GuildNameAddress;
        [FieldOffset(0x9060 - Skip)] public readonly SkillBarIdsStruct SkillBarIds;
        [FieldOffset(0x90C0 - Skip)] public readonly NativePtrArray NearestPlayers;
        [FieldOffset(0x9378 - Skip)] public readonly NativePtrArray PlayerInventories;
        [FieldOffset(0x9608 - Skip)] public readonly NativePtrArray NPCInventories;
        [FieldOffset(0x9898 - Skip)] public readonly NativePtrArray GuildInventories;
        [FieldOffset(0x9A08 - Skip)] public readonly ushort TradeChatChannel;
        [FieldOffset(0x9A10 - Skip)] public readonly ushort GlobalChatChannel;
        [FieldOffset(0x9AB0 - Skip)] public readonly ushort LastActionId;
        [FieldOffset(0x9B08 - Skip)] public readonly long MavenMapsList;
        [FieldOffset(0x9B10 - Skip)] public readonly long MavenMapsCount;
        [FieldOffset(0x9B18 - Skip)] public readonly long MavenMapsArray;
        [FieldOffset(0x9B48 - Skip)] public readonly long CompletedMapsList;
        [FieldOffset(0x9B50 - Skip)] public readonly long CompletedMapsCount;
        [FieldOffset(0x9B58 - Skip)] public readonly long CompletedMapsArray;
        [FieldOffset(0x9B88 - Skip)] public readonly long BonusCompletedAreasList;
        [FieldOffset(0x9B90 - Skip)] public readonly long BonusCompletedAreasCount;
        [FieldOffset(0x9B98 - Skip)] public readonly long BonusCompletedAreasArray;
        [FieldOffset(0x9C00 - Skip)] public readonly long FavouredMapsArray;
        [FieldOffset(0x9C1A - Skip)] public readonly AtlasMissionAmounts LowTierAtlasMissionAmounts;
        [FieldOffset(0x9C28 - Skip)] public readonly AtlasMissionAmounts MidTierAtlasMissionAmounts;
        [FieldOffset(0x9C36 - Skip)] public readonly AtlasMissionAmounts HighTierAtlasMissionAmounts;
        [FieldOffset(0x9C44 - Skip)] public readonly byte SocketedWatchstones;
        [FieldOffset(0x9D00 - Skip)] public readonly long BestiaryCapturedMonsterList;
        [FieldOffset(0xA5A0 - Skip)] public readonly int DialogDepth;
        [FieldOffset(0xA5A4 - Skip)] public readonly byte MonsterLevel;
        [FieldOffset(0xA5A5 - Skip)] public readonly byte MonstersRemaining;
        [FieldOffset(0xA652 - Skip)] public readonly int CurrentAzuriteAmount;
        [FieldOffset(0xA662 - Skip)] public readonly ushort CurrentSulphiteAmount;

        public const int BestiaryBeastsCapturedCounts = 0x9D88;

        // 3.17 Layout
        // [FieldOffset(0x8D20 - Skip)] public readonly long PlayerRelatedData;
        // [FieldOffset(0x8D38 - Skip)] public readonly byte NetworkState;
        // [FieldOffset(0x8D50 - Skip)] public readonly NativeStringU League;
        // [FieldOffset(0x8DD0 - Skip)] public readonly int TimeInGame;
        // [FieldOffset(0x8DF0 - Skip)] public readonly int Latency;
        // [FieldOffset(0x8DF8 - Skip)] public readonly NativePtrArray PlayerStashTabs;
        // [FieldOffset(0x8E10 - Skip)] public readonly NativePtrArray GuildStashTabs;
        // [FieldOffset(0x8EE0 - Skip)] public readonly long FriendsListMap;
        // [FieldOffset(0x8EF8 - Skip)] public readonly long FriendList;
        // [FieldOffset(0x8F08 - Skip)] public readonly NativePtrArray FriendListArray;
        // [FieldOffset(0x8F48 - Skip)] public readonly NativePtrArray PendingInvites;
        // [FieldOffset(0x8F88 - Skip)] public readonly long PartyPublicDescription;
        // [FieldOffset(0x8FA8 - Skip)] public readonly long PartyLeaderKey;
        // [FieldOffset(0x8FC8 - Skip)] public readonly byte PartyStatusType;
        // [FieldOffset(0x8FD0 - Skip)] public readonly NativePtrArray CurrentParty;
        // [FieldOffset(0x8FE8 - Skip)] public readonly byte PartyAllocationType;
        // [FieldOffset(0x8FE9 - Skip)] public readonly bool PartyDownscaleDisabled;
        // [FieldOffset(0x9000 - Skip)] public readonly long GuildList;
        // [FieldOffset(0x9010 - Skip)] public readonly NativePtrArray GuildArray;
        // [FieldOffset(0x9050 - Skip)] public readonly long GuildNameAddress;
        // [FieldOffset(0x9060 - Skip)] public readonly SkillBarIdsStruct SkillBarIds;
        // [FieldOffset(0x907C - Skip)] public readonly float MouseWorldPosX;
        // [FieldOffset(0x9080 - Skip)] public readonly float MouseWorldPosY;
        // [FieldOffset(0x9088 - Skip)] public readonly long SkillBarUiRootKey;
        // [FieldOffset(0x90C0 - Skip)] public readonly NativePtrArray NearestPlayers;
        // [FieldOffset(0x90F0 - Skip)] public readonly NativePtrArray MinimapIcons;
        // [FieldOffset(0x9110 - Skip)] public readonly NativePtrArray LocalPlayer;
        // [FieldOffset(0x9378 - Skip)] public readonly NativePtrArray PlayerInventories;
        // [FieldOffset(0x9608 - Skip)] public readonly NativePtrArray NPCInventories;
        // [FieldOffset(0x9898 - Skip)] public readonly NativePtrArray GuildInventories;
        // [FieldOffset(0x98B8 - Skip)] public readonly NativeStringU SocialMessage;
        // [FieldOffset(0x9A08 - Skip)] public readonly ushort TradeChatChannel;
        // [FieldOffset(0x9A10 - Skip)] public readonly ushort GlobalChatChannel;
        // [FieldOffset(0x9AB0 - Skip)] public readonly ushort LastActionId;
        // [FieldOffset(0x9AE8 - Skip)] public readonly long InstanceLeagueInfo;
        // [FieldOffset(0x9B08 - Skip)] public readonly long MavenMapsList;
        // [FieldOffset(0x9B10 - Skip)] public readonly long MavenMapsCount;
        // [FieldOffset(0x9B18 - Skip)] public readonly long MavenMapsArray;
        // [FieldOffset(0x9B48 - Skip)] public readonly long CompletedMapsList;
        // [FieldOffset(0x9B50 - Skip)] public readonly long CompletedMapsCount;
        // [FieldOffset(0x9B58 - Skip)] public readonly long CompletedMapsArray;
        // [FieldOffset(0x9B88 - Skip)] public readonly long BonusCompletedAreasList;
        // [FieldOffset(0x9B90 - Skip)] public readonly long BonusCompletedAreasCount;
        // [FieldOffset(0x9B98 - Skip)] public readonly long BonusCompletedAreasArray;
        // [FieldOffset(0x9C00 - Skip)] public readonly long FavouredMapsArray;
        // [FieldOffset(0x9C1A - Skip)] public readonly AtlasMissionAmounts LowTierAtlasMissionAmounts;
        // [FieldOffset(0x9C28 - Skip)] public readonly AtlasMissionAmounts MidTierAtlasMissionAmounts;
        // [FieldOffset(0x9C36 - Skip)] public readonly AtlasMissionAmounts HighTierAtlasMissionAmounts;
        // [FieldOffset(0x9C44 - Skip)] public readonly byte SocketedWatchstones;
        // [FieldOffset(0x9D00 - Skip)] public readonly long BestiaryCapturedMonsterList;
        // [FieldOffset(0x9D88 - Skip)] public fixed int BestiaryBeastCaptureCounts[470];
        // [FieldOffset(0x9DF8 - Skip)] public readonly long LeftIncursionArchitectKey;
        // [FieldOffset(0x9E08 - Skip)] public readonly long RightIncursionArchitectKey;
        // [FieldOffset(0xA5A0 - Skip)] public readonly int DialogDepth;
        // [FieldOffset(0xA5A4 - Skip)] public readonly byte MonsterLevel;
        // [FieldOffset(0xA5A5 - Skip)] public readonly byte MonstersRemaining;
        // [FieldOffset(0xA64C - Skip)] public readonly byte DelveFlareCount;
        // [FieldOffset(0xA64D - Skip)] public readonly byte DelveDynamiteCount;
        // [FieldOffset(0xA652 - Skip)] public readonly int CurrentAzuriteAmount;
        // [FieldOffset(0xA659 - Skip)] public readonly byte DelveMaximumFlareTier;
        // [FieldOffset(0xA65A - Skip)] public readonly byte DelveMaximumDynamiteTier;
        // [FieldOffset(0xA65C - Skip)] public readonly byte DelveFlareRadiusTier;
        // [FieldOffset(0xA65D - Skip)] public readonly byte DelveDynamiteRadiusTier;
        // [FieldOffset(0xA65F - Skip)] public readonly byte DelveDynamiteDamageTier;
        // [FieldOffset(0xA661 - Skip)] public readonly byte DelveFlareDurationTier;
        // [FieldOffset(0xA662 - Skip)] public readonly ushort CurrentSulphiteAmount;
        // [FieldOffset(0xA7A8 - Skip)] public readonly NativePtrArray HorticraftingRecipes;
        // [FieldOffset(0xAF38 - Skip)] public readonly long DeliriumRewardInfo;
        // [FieldOffset(0xAF50 - Skip)] public readonly long HeistBlueprintInfo;
        // [FieldOffset(0xAF68 - Skip)] public readonly long HeistContractInfo;
        // [FieldOffset(0xAFB8 - Skip)] public readonly int CrucibleExperience;
    }
}
