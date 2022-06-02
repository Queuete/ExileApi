namespace ExileCore.PoEMemory.MemoryObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExileCore.PoEMemory.Elements;
    using ExileCore.PoEMemory.MemoryObjects.Metamorph;
    using ExileCore.Shared.Cache;
    using GameOffsets;

    public class IngameUIElements : Element
    {
        private readonly CachedValue<List<QuestState>> _cachedQuestStates;
        private readonly CachedValue<IngameUElementsOffsets> _cachedValue;
        private Element _BetrayalWindow;
        private Element _CraftBench;
        private Cursor _cursor;
        private SubterraneanChart _DelveWindow;
        private Element _haggleWindow;
        private IncursionWindow _IncursionWindow;
        private Map _map;
        private Element _purchaseWindow;
        private Element _SynthesisWindow;
        private Element _UnveilWindow;
        private Element _ZanaMissionChoice;

        public IngameUIElements()
        {
            this._cachedValue = new FrameCache<IngameUElementsOffsets>(() => this.M.Read<IngameUElementsOffsets>(this.Address));
            this._cachedQuestStates = new TimeCache<List<QuestState>>(this.GenerateQuestStates, 1000);
        }

        public WorldMapElement AreaInstanceUi => this.GetObject<WorldMapElement>(this.IngameUIElementsStruct.AreaInstanceUi);

        public AtlasElement Atlas => this.AtlasPanel; // Required to fit with TehCheats Api, Random Feature uses this field.

        public AtlasElement AtlasPanel => this.GetObject<AtlasElement>(this.IngameUIElementsStruct.AtlasPanel);

        public Element AtlasSkillPanel => this.GetObject<Element>(this.IngameUIElementsStruct.AtlasSkillPanel);

        public BanditDialog BanditDialog => this.GetObject<BanditDialog>(this.IngameUIElementsStruct.BanditDialog);

        public Element BetrayalWindow => this._BetrayalWindow
            = this._BetrayalWindow ?? this.GetObject<Element>(this.IngameUIElementsStruct.BetrayalWindow);

        [Obsolete("Use ChatBoxRoot?.MessageBox instead")]
        public Element ChatBox => this.ChatBoxRoot?.MessageBox;

        public ChatElement ChatBoxRoot => this.GetObject<ChatElement>(this.IngameUIElementsStruct.ChatPanel);

        [Obsolete("Use ChatBoxRoot?.MessageBox?.Children.Select(x => x.Text).ToList() instead")]
        public IList<string> ChatMessages => this.ChatBox?.Children.Select(x => x.Text).ToList();

        public Element CraftBench
            => this._CraftBench = this._CraftBench ?? this.GetObject<Element>(this.IngameUIElementsStruct.CraftBenchWindow);

        public Cursor Cursor => this._cursor = this._cursor ?? this.GetObject<Cursor>(this.IngameUIElementsStruct.Mouse);

        public DelveDarknessElement DelveDarkness => this.GetObject<DelveDarknessElement>(this.IngameUIElementsStruct.DelveDarkness);

        public SubterraneanChart DelveWindow => this._DelveWindow
            = this._DelveWindow ?? this.GetObject<SubterraneanChart>(this.IngameUIElementsStruct.DelveWindow);

        public string DndMessage => this.M.ReadStringU(this.M.Read<long>(this.Address + 0xf98));

        public GameUi GameUI => this.GetObject<GameUi>(this.IngameUIElementsStruct.GameUI);

        public GemLvlUpPanel GemLvlUpPanel => this.GetObject<GemLvlUpPanel>(this.IngameUIElementsStruct.GemLvlUpPanel);

        public IEnumerable<QuestState> GetCompletedQuests => new List<QuestState>(); // GetQuestStates.Where(q => q.QuestStateId == 0);

        public List<QuestState> GetQuestStates => new List<QuestState>(); // _cachedQuestStates?.Value;

        // TODO: Was causing crash. Fix for 3.18.
        public IEnumerable<QuestState> GetUncompletedQuests => new List<QuestState>(); // GetQuestStates.Where(q => q.QuestStateId != 0));

        public StashElement GuildStash => this.GetObject<StashElement>(this.IngameUIElementsStruct.GuildStashElement);

        public Element HaggleWindow => this._haggleWindow
            = this._haggleWindow ?? this.GetObject<Element>(this.IngameUIElementsStruct.LeaguePurchaseWindow);

        public HarvestWindow HarvestWindow => this.HorticraftingSacredGrovePanel.IsVisible ? this.HorticraftingSacredGrovePanel
            : this.HorticraftingHideoutPanel;

        public Element HeistAllyEquipmentWindow => this.GetObject<Element>(this.IngameUIElementsStruct.HeistAllyEquipmentPanel);

        public Element HeistBlueprintWindow => this.GetObject<Element>(this.IngameUIElementsStruct.HeistBlueprintPanel);

        public Element HeistContractWindow => this.GetObject<Element>(this.IngameUIElementsStruct.HeistContractPanel);

        public Element HeistLockerWindow => this.GetObject<Element>(this.IngameUIElementsStruct.HeistLockerPanel);

        public Element HeistRevealWindow => this.GetObject<Element>(this.IngameUIElementsStruct.HeistRevealPanel);

        public SkillBarElement HiddenSkillBar => this.GetObject<SkillBarElement>(this.IngameUIElementsStruct.HiddenSkillBar);

        public HarvestWindow HorticraftingHideoutPanel => this.GetObject<HarvestWindow>(this.IngameUIElementsStruct.HorticraftingHideoutPanel);

        public HarvestWindow HorticraftingSacredGrovePanel
            => this.GetObject<HarvestWindow>(this.IngameUIElementsStruct.HorticraftingSacredGrovePanel);

        public IncursionWindow IncursionWindow => this._IncursionWindow
            = this._IncursionWindow ?? this.GetObject<IncursionWindow>(this.IngameUIElementsStruct.IncursionWindow);

        public IngameUElementsOffsets IngameUIElementsStruct => this._cachedValue.Value;

        public InventoryElement InventoryPanel => this.GetObject<InventoryElement>(this.IngameUIElementsStruct.InventoryPanel);

        public Element InvitesPanel => this.GetObject<Element>(this.IngameUIElementsStruct.InvitesPanel);

        public bool IsDndEnabled => this.M.Read<byte>(this.Address + 0xf92) == 1;

        public ItemOnGroundTooltip ItemOnGroundTooltip
            => this.GetObject<ItemOnGroundTooltip>(this.IngameUIElementsStruct.ItemOnGroundTooltip);

        public ItemsOnGroundLabelElement ItemsOnGroundLabelElement
            => this.GetObject<ItemsOnGroundLabelElement>(this.IngameUIElementsStruct.itemsOnGroundLabelRoot);

        public IList<LabelOnGround> ItemsOnGroundLabels => this.ItemsOnGroundLabelElement.LabelsOnGround;

        public IList<LabelOnGround> ItemsOnGroundLabelsVisible
            => this.ItemsOnGroundLabelElement.LabelsOnGround.Where(x => x.Address != 0 && x.IsVisible).ToList();

        public DivineFont LabyrinthDivineFontPanel => this.GetObject<DivineFont>(this.IngameUIElementsStruct.LabyrinthDivineFontPanel);

        public Map Map => this._map = this._map ?? this.GetObject<Map>(this.IngameUIElementsStruct.Map);

        public Element MapDeviceWindow => this.GetObject<Element>(this.IngameUIElementsStruct.MapDeviceWindow);

        public MapStashTabElement MapStashTab => this.ReadObject<MapStashTabElement>(this.IngameUIElementsStruct.MapTabWindowStartPtr);

        public MetamorphWindowElement MetamorphWindow
            => this.GetObject<MetamorphWindowElement>(this.IngameUIElementsStruct.MetamorphWindow);

        public NpcDialog NpcDialog => this.GetObject<NpcDialog>(this.IngameUIElementsStruct.NpcDialog);

        public Element OpenLeftPanel => this.GetObject<Element>(this.IngameUIElementsStruct.OpenLeftPanel);

        public Element OpenRightPanel => this.GetObject<Element>(this.IngameUIElementsStruct.OpenRightPanel);

        public Element PurchaseWindow
        {
            get
            {
                var purchaseWindow = this.GetObject<Element>(this.IngameUIElementsStruct.PurchaseWindow);

                if (purchaseWindow != null && purchaseWindow.IsVisibleLocal)
                {
                    return purchaseWindow;
                }

                return this.GetObject<Element>(this.IngameUIElementsStruct.LeaguePurchaseWindow);
            }
        }

        public Element PVPPanel => this.GetChildAtIndex(26);

        public QuestRewardWindow QuestRewardWindow => this.GetObject<QuestRewardWindow>(this.IngameUIElementsStruct.QuestRewardWindow);

        public Element QuestTracker => this.GetObject<Element>(this.IngameUIElementsStruct.QuestTracker);

        public Element RitualFavourWindow => this.GetObject<Element>(this.IngameUIElementsStruct.RitualFavourPanel);

        public RitualWindow RitualWindow => this.GetObject<RitualWindow>(this.IngameUIElementsStruct.RitualWindow);

        public SellWindow SellWindow
        {
            get
            {
                var sellWindow = this.GetObject<SellWindow>(this.IngameUIElementsStruct.SellWindow);

                if (sellWindow != null && sellWindow.IsVisibleLocal)
                {
                    return sellWindow;
                }

                return this.GetObject<SellWindow>(this.IngameUIElementsStruct.LeagueSellWindow);
            }
        }

        public SkillBarElement SkillBar => this.GetObject<SkillBarElement>(this.IngameUIElementsStruct.SkillBar);

        public StashElement StashElement => this.GetObject<StashElement>(this.IngameUIElementsStruct.StashElement);

        public Element Sulphit => this.GetObject<Element>(this.IngameUIElementsStruct.Map).GetChildAtIndex(3);

        public Element SyndicatePanel => this.BetrayalWindow; // Required for TehCheats Api, BroodyHen uses this.

        public Element SyndicateTree => this.BetrayalWindow[0];

        public Element SynthesisWindow => this._SynthesisWindow
            = this._SynthesisWindow ?? this.GetObject<Element>(this.IngameUIElementsStruct.SynthesisWindow);

        public TradeWindow TradeWindow => this.GetObject<TradeWindow>(this.IngameUIElementsStruct.TradeWindow);

        public Element TreePanel => this.GetChildAtIndex(25);

        public Element UltimatumProgressWindow => this.GetObject<Element>(this.IngameUIElementsStruct.UltimatumProgressPanel);

        public Element UnveilWindow
            => this._UnveilWindow = this._UnveilWindow ?? this.GetObject<Element>(this.IngameUIElementsStruct.UnveilWindow);

        public WorldMapElement WorldMap => this.GetObject<WorldMapElement>(this.IngameUIElementsStruct.WorldMap);

        public Element ZanaMissionChoice => this._ZanaMissionChoice
            = this._ZanaMissionChoice ?? this.GetObject<Element>(this.IngameUIElementsStruct.ZanaMissionChoice);

        private List<QuestState> GenerateQuestStates()
        {
            var result = new Dictionary<string, QuestState>();

            /*
             * This is definitely not the most performant way to get the quest.
             * 9 quests are missing (e.g. a10q2). 
             */
            for (long i = 0; i < 0xffff; i += 0x8)
            {
                var pointerToQuest = this.IngameUIElementsStruct.GetQuests + i;
                var addressOfQuest = this.M.Read<long>(pointerToQuest);

                var questState = this.GetObject<QuestState>(addressOfQuest);
                var quest = questState?.Quest;

                if (quest == null)
                {
                    continue;
                }

                if (!result.ContainsKey(quest.Id))
                {
                    result.Add(quest.Id, questState);
                }
            }

            return result.Values.ToList();
        }
    }
}
