using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects.Metamorph;
using ExileCore.Shared.Cache;
using GameOffsets;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class IngameUIElements : Element
    {
        private Element _BetrayalWindow;
        private readonly CachedValue<IngameUElementsOffsets> _cachedValue;
        private Element _CraftBench;
        private Cursor _cursor;
        private SubterraneanChart _DelveWindow;
        private IncursionWindow _IncursionWindow;
        private Map _map;
        private Element _purchaseWindow;
        private Element _SynthesisWindow;
        private Element _UnveilWindow;
        private Element _ZanaMissionChoice;

        public IngameUIElements()
        {
            _cachedValue = new FrameCache<IngameUElementsOffsets>(() => M.Read<IngameUElementsOffsets>(Address));
        }

        public IngameUElementsOffsets IngameUIElementsStruct => _cachedValue.Value;
        public GameUi GameUI => GetObject<GameUi>(IngameUIElementsStruct.GameUI);
        public SellWindow SellWindow => GetObject<SellWindow>(IngameUIElementsStruct.SellWindow);
        public TradeWindow TradeWindow => GetObject<TradeWindow>(IngameUIElementsStruct.TradeWindow);
        public NpcDialog NpcDialog => GetObject<NpcDialog>(IngameUIElementsStruct.NpcDialog);
        public BanditDialog BanditDialog => GetObject<BanditDialog>(IngameUIElementsStruct.BanditDialog);
        public Element PurchaseWindow => _purchaseWindow ?? (_purchaseWindow = GetObject<Element>(IngameUIElementsStruct.PurchaseWindow));
        public SubterraneanChart DelveWindow =>
            _DelveWindow ?? (_DelveWindow = GetObject<SubterraneanChart>(IngameUIElementsStruct.DelveWindow));
        public SkillBarElement SkillBar => GetObject<SkillBarElement>(IngameUIElementsStruct.SkillBar);
        public SkillBarElement HiddenSkillBar => GetObject<SkillBarElement>(IngameUIElementsStruct.HiddenSkillBar);

        // Structure of Chatbox is
        //Root > [x] ChatBox Root Panel > [0] Toggle Panel     > [0] Channel Bar Panel
        //                                                     > [1] Hide ChatBox Button Panel
        //                              > [1] History Panel    > [0] Feature Placeholder
        //                                                     > [1] Feature Placeholder
        //                                                     > [2] Chat Body Panel  > [0] Feature Placeholder
        //                                                                            > [1] Chat History Panel
        //                                                     > [3] Chat Scroll Panel
        //                              > [2] Target Channel Panel
        //                              > [3] Text Input Panel
        //                              > [4] Autocomplete Panel
        //
        // For backward compability, ChatBox points to the nested Chat Body Panel instead of the root panel. 

        // This offset points to the chat box panel that is a direct child of root.
        public PoeChatElement ChatBoxRoot => GetObject<PoeChatElement>(IngameUIElementsStruct.ChatPanel);

        // This offset points to the chat body panel that is a grandchild of the previous element.
        // Chatbox.Parent.Parent.Parent is equivalent to ChatBoxRoot.
        public PoeChatElement ChatBox => GetObject<PoeChatElement>(ChatBoxRoot?.GetChildAtIndex(1)?.GetChildAtIndex(2)?.GetChildAtIndex(1)?.Address);
        public IList<string> ChatMessages => ChatBox.Children.Select(x => x.Text).ToList();
        public Element QuestTracker => GetObject<Element>(IngameUIElementsStruct.QuestTracker);
        public QuestRewardWindow QuestRewardWindow => GetObject<QuestRewardWindow>(IngameUIElementsStruct.QuestRewardWindow);
        public Element OpenLeftPanel => GetObject<Element>(IngameUIElementsStruct.OpenLeftPanel);
        public Element OpenRightPanel => GetObject<Element>(IngameUIElementsStruct.OpenRightPanel);
        public StashElement StashElement => GetObject<StashElement>(IngameUIElementsStruct.StashElement);
        public InventoryElement InventoryPanel => GetObject<InventoryElement>(IngameUIElementsStruct.InventoryPanel);
        public Element TreePanel => GetObject<Element>(IngameUIElementsStruct.TreePanel);
        public Element AtlasPanel => GetObject<Element>(IngameUIElementsStruct.AtlasPanel);
        public Element Atlas => AtlasPanel; // Required to fit with TehCheats Api, Random Feature uses this field.
        public Map Map => _map ?? (_map = GetObject<Map>(IngameUIElementsStruct.Map));
        public ItemsOnGroundLabelElement ItemsOnGroundLabelElement =>
            GetObject<ItemsOnGroundLabelElement>(IngameUIElementsStruct.itemsOnGroundLabelRoot);
        public IList<LabelOnGround> ItemsOnGroundLabels => ItemsOnGroundLabelElement.LabelsOnGround;

        public IList<LabelOnGround> ItemsOnGroundLabelsVisible =>
            ItemsOnGroundLabelElement.LabelsOnGround.Where(x => x.Address != 0 && x.IsVisible).ToList();
        public GemLvlUpPanel GemLvlUpPanel => GetObject<GemLvlUpPanel>(IngameUIElementsStruct.GemLvlUpPanel);
        public Element InvitesPanel => GetObject<Element>(IngameUIElementsStruct.InvitesPanel);
        public ItemOnGroundTooltip ItemOnGroundTooltip => GetObject<ItemOnGroundTooltip>(IngameUIElementsStruct.ItemOnGroundTooltip);
        public MapStashTabElement MapStashTab => ReadObject<MapStashTabElement>(IngameUIElementsStruct.MapTabWindowStartPtr);
        public Element Sulphit => GetObject<Element>(IngameUIElementsStruct.Map).GetChildAtIndex(0);
        public Cursor Cursor => _cursor ?? (_cursor = GetObject<Cursor>(IngameUIElementsStruct.Mouse));
        public Element BetrayalWindow => _BetrayalWindow ?? (_BetrayalWindow = GetObject<Element>(IngameUIElementsStruct.BetrayalWindow));
        public Element SyndicateTree => GetObject<Element>(M.Read<long>(BetrayalWindow.Address + 0xA50));
        public Element UnveilWindow => _UnveilWindow ?? (_UnveilWindow = GetObject<Element>(IngameUIElementsStruct.UnveilWindow));
        public Element ZanaMissionChoice =>
            _ZanaMissionChoice ?? (_ZanaMissionChoice = GetObject<Element>(IngameUIElementsStruct.ZanaMissionChoice));
        public IncursionWindow IncursionWindow =>
            _IncursionWindow ?? (_IncursionWindow = GetObject<IncursionWindow>(IngameUIElementsStruct.IncursionWindow));
        public Element SynthesisWindow =>
            _SynthesisWindow ?? (_SynthesisWindow = GetObject<Element>(IngameUIElementsStruct.SynthesisWindow));
        public Element CraftBench => _CraftBench ?? (_CraftBench = GetObject<Element>(IngameUIElementsStruct.CraftBenchWindow));
        public bool IsDndEnabled => M.Read<byte>(Address + 0xf92) == 1;
        public string DndMessage => M.ReadStringU(M.Read<long>(Address + 0xf98));
        public WorldMapElement AreaInstanceUi => GetObject<WorldMapElement>(IngameUIElementsStruct.AreaInstanceUi);
        public WorldMapElement WorldMap => GetObject<WorldMapElement>(IngameUIElementsStruct.WorldMap);
        public MetamorphWindowElement MetamorphWindow => GetObject<MetamorphWindowElement>(IngameUIElementsStruct.MetamorphWindow);
        public RitualWindow RitualWindow => GetObject<RitualWindow>(IngameUIElementsStruct.RitualWindow);

        public IList<Tuple<Quest, int>> GetUncompletedQuests
        {
            get
            {
                if (IngameUIElementsStruct.GetQuests == 0) return new List<Tuple<Quest, int>>();
                var stateListPres = M.ReadDoublePointerIntList(IngameUIElementsStruct.GetQuests);

                return stateListPres.Where(x => x.Item2 > 0)
                    .Select(x => new Tuple<Quest, int>(TheGame.Files.Quests.GetByAddress(x.Item1), x.Item2)).ToList();
            }
        }

        public IList<Tuple<Quest, int>> GetCompletedQuests
        {
            get
            {
                if (IngameUIElementsStruct.GetQuests == 0) return new List<Tuple<Quest, int>>();
                var stateListPres = M.ReadDoublePointerIntList(IngameUIElementsStruct.GetQuests);

                return stateListPres.Where(x => x.Item2 == 0)
                    .Select(x => new Tuple<Quest, int>(TheGame.Files.Quests.GetByAddress(x.Item1), x.Item2)).ToList();
            }
        }

        public Dictionary<Quest, QuestState> GetUncompletedQuests2
        {
            get
            {
                var result = new Dictionary<Quest, QuestState>();
                var keyValuePairs = GetQuestStates;

                foreach (var keyValuePair in keyValuePairs.Values)
                {
                    if (keyValuePair.Value.QuestStateId > 0)
                        result[keyValuePair.Key] = keyValuePair.Value;
                }

                return result;
            }
        }

        public Dictionary<string, KeyValuePair<Quest, QuestState>> GetQuestStates
        {
            get
            {
                if (IngameUIElementsStruct.GetQuests == 0) return new Dictionary<string, KeyValuePair<Quest, QuestState>>();
                var dictionary = new Dictionary<string, KeyValuePair<Quest, QuestState>>();

                foreach (var quest in GetQuests)
                {
                    if (quest == null) continue;
                    if (quest.Item1 == null) continue;

                    var value = TheGame.Files.QuestStates.GetQuestState(quest.Item1.Id, quest.Item2);

                    if (value == null) continue;

                    if (!dictionary.ContainsKey(quest.Item1.Id))
                        dictionary.Add(quest.Item1.Id, new KeyValuePair<Quest, QuestState>(quest.Item1, value));
                }

                return dictionary.OrderBy(x => x.Key).ToDictionary(Key => Key.Key, Value => Value.Value);
            }
        }

        private IList<Tuple<Quest, int>> GetQuests
        {
            get
            {
                if (IngameUIElementsStruct.GetQuests == 0) return new List<Tuple<Quest, int>>();
                var stateListPres = M.ReadDoublePointerIntList(IngameUIElementsStruct.GetQuests);
                return stateListPres.Select(x => new Tuple<Quest, int>(TheGame.Files.Quests.GetByAddress(x.Item1), x.Item2)).ToList();
            }
        }
    }
}
