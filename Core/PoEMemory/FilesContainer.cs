using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ExileCore.PoEMemory.FilesInMemory;
using ExileCore.PoEMemory.FilesInMemory.Atlas;
using ExileCore.PoEMemory.FilesInMemory.Metamorph;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.PoEMemory.MemoryObjects.Heist;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Static;

namespace ExileCore.PoEMemory
{
    using System.Threading.Tasks;
    using ExileCore.Shared.Enums;

    public class FilesContainer
    {
        private readonly IMemory _Memory;
        public FilesFromMemory FilesFromMemory;

        private BaseItemTypes _BaseItemTypes;
        private UniversalFileWrapper<AtlasNode> _AtlasNodes;
        private UniversalFileWrapper<BetrayalChoiceAction> _BetrayalChoiceActions;
        private UniversalFileWrapper<BetrayalChoice> _BetrayalChoices;
        private UniversalFileWrapper<BetrayalDialogue> _BetrayalDialogue;
        private UniversalFileWrapper<BetrayalJob> _BetrayalJobs;
        private UniversalFileWrapper<BetrayalRank> _BetrayalRanks;
        private UniversalFileWrapper<BetrayalReward> _BetrayalRewards;
        private UniversalFileWrapper<BetrayalTarget> _BetrayalTargets;
        private UniversalFileWrapper<HeistJobRecord> _HeistJobs;
        private UniversalFileWrapper<HeistChestRewardTypeRecord> _HeistChestRewardTypes;
        private UniversalFileWrapper<HeistNpcRecord> _HeistNpcs;
        private LabyrinthTrials _LabyrinthTrials;
        private ModsDat _Mods;
        private MonsterVarieties _MonsterVarieties;
        private PassiveSkills _PassiveSkills;
        private PropheciesDat _Prophecies;
        private Quests _Quests;
        private QuestStates _QuestStates;
        private StatsDat _Stats;
        private TagsDat _Tags;
        private WorldAreas _WorldAreas;
        private UniqueStashLayoutsDat _UniqueItemDescriptions;
        private ItemVisualIdentitiesDat _ItemVisualIdentities;
        private GroundEffectsDat _GroundEffects;
        private GroundEffectTypesDat _GroundEffectTypes;

        public FilesContainer(IMemory memory)
        {
            _Memory = memory;
            ItemClasses = new ItemClasses();
            FilesFromMemory = new FilesFromMemory(_Memory);

            ReloadFiles();

            Task.Run(() =>
            {
                using (new PerformanceTimer("Preload stats and mods"))
                {
                    var _ = this.Stats.records.Count;
                    var __ = this.Mods.records.Count;
                    this.ParseFiles(this.AllFiles);
                }
            });
        }

        #region Misc

        public UniqueStashLayoutsDat UniqueItemDescriptions
        {
            get
            {
                if (this._UniqueItemDescriptions != null && this._UniqueItemDescriptions.Address != 0)
                {
                    return this._UniqueItemDescriptions;
                }

                var inventories = RemoteMemoryObject.pTheGame?.IngameState?.IngameUi?.StashElement?.AllInventories?.ToList();
                if (inventories != null && inventories.Any(
                    inv => inv != null && inv.InvType == InventoryType.UniqueStash))
                {
                    this.ReloadFiles();
                }

                return this._UniqueItemDescriptions = new UniqueStashLayoutsDat(this._Memory, () => this.FindFile("Data/UniqueStashLayout.dat"));
            }
        }

        public ItemVisualIdentitiesDat ItemVisualIdentities => this._ItemVisualIdentities
         ?? (_ItemVisualIdentities = new ItemVisualIdentitiesDat(this._Memory, () => this.FindFile("Data/ItemVisualIdentity.dat")));

        public ItemClasses ItemClasses { get; }

        public BaseItemTypes BaseItemTypes =>
            _BaseItemTypes = _BaseItemTypes ?? new BaseItemTypes(_Memory, () => FindFile("Data/BaseItemTypes.dat"));

        public ModsDat Mods => _Mods = _Mods ?? new ModsDat(_Memory, () => FindFile("Data/Mods.dat"), Stats, Tags);
        public StatsDat Stats => _Stats = _Stats ?? new StatsDat(_Memory, () => FindFile("Data/Stats.dat"));
        public TagsDat Tags => _Tags = _Tags ?? new TagsDat(_Memory, () => FindFile("Data/Tags.dat"));
        public WorldAreas WorldAreas => _WorldAreas = _WorldAreas ?? new WorldAreas(_Memory, () => FindFile("Data/WorldAreas.dat"));

        public GroundEffectsDat GroundEffects => this._GroundEffects ?? (this._GroundEffects = new GroundEffectsDat(
            this._Memory,
            () => this.FindFile("Data/GroundEffects.dat")));

        public GroundEffectTypesDat GroundEffectTypes => this._GroundEffectTypes
         ?? (this._GroundEffectTypes = new GroundEffectTypesDat(this._Memory, () => this.FindFile("Data/GroundEffectTypes.dat")));

        public PassiveSkills PassiveSkills =>
            _PassiveSkills = _PassiveSkills ?? new PassiveSkills(_Memory, () => FindFile("Data/PassiveSkills.dat"));

        public LabyrinthTrials LabyrinthTrials =>
            _LabyrinthTrials = _LabyrinthTrials ?? new LabyrinthTrials(_Memory, () => FindFile("Data/LabyrinthTrials.dat"));

        public Quests Quests => _Quests = _Quests ?? new Quests(_Memory, () => FindFile("Data/Quest.dat"));

        public QuestStates QuestStates =>
            _QuestStates = _QuestStates ?? new QuestStates(_Memory, () => FindFile("Data/QuestStates.dat"));

        public MonsterVarieties MonsterVarieties =>
            _MonsterVarieties = _MonsterVarieties ?? new MonsterVarieties(_Memory, () => FindFile("Data/MonsterVarieties.dat"));

        public PropheciesDat Prophecies =>
            _Prophecies = _Prophecies ?? new PropheciesDat(_Memory, () => FindFile("Data/Prophecies.dat"));

        public UniversalFileWrapper<AtlasNode> AtlasNodes =>
            _AtlasNodes = _AtlasNodes ?? new AtlasNodes(_Memory, () => FindFile("Data/AtlasNode.dat"));

        #endregion Misc

        #region Betrayal

        public UniversalFileWrapper<BetrayalTarget> BetrayalTargets =>
            _BetrayalTargets = _BetrayalTargets ?? 
                new UniversalFileWrapper<BetrayalTarget>(_Memory, () => FindFile("Data/BetrayalTargets.dat"));

        public UniversalFileWrapper<BetrayalJob> BetrayalJobs =>
            _BetrayalJobs = _BetrayalJobs ?? new UniversalFileWrapper<BetrayalJob>(_Memory, () => FindFile("Data/BetrayalJobs.dat"));

        public UniversalFileWrapper<BetrayalRank> BetrayalRanks =>
            _BetrayalRanks = _BetrayalRanks ?? 
                new UniversalFileWrapper<BetrayalRank>(_Memory, () => FindFile("Data/BetrayalRanks.dat"));

        public UniversalFileWrapper<BetrayalReward> BetrayalRewards =>
            _BetrayalRewards = _BetrayalRewards ?? 
                new UniversalFileWrapper<BetrayalReward>(_Memory, () => FindFile("Data/BetrayalTraitorRewards.dat"));

        public UniversalFileWrapper<BetrayalChoice> BetrayalChoises =>
            _BetrayalChoices = _BetrayalChoices ?? 
                new UniversalFileWrapper<BetrayalChoice>(_Memory, () => FindFile("Data/BetrayalChoices.dat"));

        public UniversalFileWrapper<BetrayalChoiceAction> BetrayalChoiceActions =>
            _BetrayalChoiceActions = _BetrayalChoiceActions ?? 
                new UniversalFileWrapper<BetrayalChoiceAction>(_Memory,
                    () => FindFile("Data/BetrayalChoiceActions.dat"));

        public UniversalFileWrapper<BetrayalDialogue> BetrayalDialogue =>
            _BetrayalDialogue = _BetrayalDialogue ?? 
                new UniversalFileWrapper<BetrayalDialogue>(_Memory, () => FindFile("Data/BetrayalDialogue.dat"));

        #endregion

        #region Heist

        public UniversalFileWrapper<HeistJobRecord> HeistJobs =>
            _HeistJobs = _HeistJobs ?? new UniversalFileWrapper<HeistJobRecord>(_Memory, () => FindFile("Data/HeistJobs.dat"));

        public UniversalFileWrapper<HeistChestRewardTypeRecord> HeistChestRewardType =>
            _HeistChestRewardTypes = _HeistChestRewardTypes ?? 
                new UniversalFileWrapper<HeistChestRewardTypeRecord>(_Memory,
                    () => FindFile("Data/HeistChestRewardTypes.dat"));
        public UniversalFileWrapper<HeistNpcRecord> HeistNpcs => _HeistNpcs = _HeistNpcs ?? 
            new UniversalFileWrapper<HeistNpcRecord>(_Memory, () => FindFile("Data/HeistNPCs.dat"));

        #endregion

        #region Metamorph

        private UniversalFileWrapper<MetamorphMetaSkill> _MetamorphMetaSkills;
        private UniversalFileWrapper<MetamorphMetaSkillType> _MetamorphMetaSkillTypes;
        private UniversalFileWrapper<MetamorphMetaMonster> _MetamorphMetaMonsters;
        private UniversalFileWrapper<MetamorphRewardType> _MetamorphRewardTypes;
        private UniversalFileWrapper<MetamorphRewardTypeItemsClient> _MetamorphRewardTypeItemsClient;

        public UniversalFileWrapper<MetamorphMetaSkill> MetamorphMetaSkills =>
            _MetamorphMetaSkills = _MetamorphMetaSkills ?? 
                new UniversalFileWrapper<MetamorphMetaSkill>(_Memory,
                    () => FindFile("Data/MetamorphosisMetaSkills.dat"));

        public UniversalFileWrapper<MetamorphMetaSkillType> MetamorphMetaSkillTypes =>
            _MetamorphMetaSkillTypes = _MetamorphMetaSkillTypes ?? 
                new UniversalFileWrapper<MetamorphMetaSkillType>(_Memory,
                    () => FindFile("Data/MetamorphosisMetaSkillTypes.dat"));

        public UniversalFileWrapper<MetamorphMetaMonster> MetamorphMetaMonsters =>
            _MetamorphMetaMonsters = _MetamorphMetaMonsters ?? 
                new UniversalFileWrapper<MetamorphMetaMonster>(_Memory,
                    () => FindFile("Data/MetamorphosisMetaMonsters.dat"));

        public UniversalFileWrapper<MetamorphRewardType> MetamorphRewardTypes =>
            _MetamorphRewardTypes = _MetamorphRewardTypes ?? 
                new UniversalFileWrapper<MetamorphRewardType>(_Memory,
                    () => FindFile("Data/MetamorphosisRewardTypes.dat"));

        public UniversalFileWrapper<MetamorphRewardTypeItemsClient> MetamorphRewardTypeItemsClient =>
            _MetamorphRewardTypeItemsClient = _MetamorphRewardTypeItemsClient ?? 
                new UniversalFileWrapper<MetamorphRewardTypeItemsClient>(_Memory,
                    () => FindFile("Data/MetamorphosisRewardTypeItemsClient.dat"));

        #endregion

        #region Bestiary

        private BestiaryCapturableMonsters _BestiaryCapturableMonsters;

        public BestiaryCapturableMonsters BestiaryCapturableMonsters =>
            _BestiaryCapturableMonsters = _BestiaryCapturableMonsters ?? 
                new BestiaryCapturableMonsters(_Memory, () => FindFile("Data/BestiaryCapturableMonsters.dat"));

        private UniversalFileWrapper<BestiaryRecipe> _BestiaryRecipes;

        public UniversalFileWrapper<BestiaryRecipe> BestiaryRecipes =>
            _BestiaryRecipes = _BestiaryRecipes ?? 
                new UniversalFileWrapper<BestiaryRecipe>(_Memory, () => FindFile("Data/BestiaryRecipes.dat"));

        private UniversalFileWrapper<BestiaryRecipeComponent> _BestiaryRecipeComponents;

        public UniversalFileWrapper<BestiaryRecipeComponent> BestiaryRecipeComponents =>
            _BestiaryRecipeComponents = _BestiaryRecipeComponents ?? 
                new UniversalFileWrapper<BestiaryRecipeComponent>(_Memory,
                    () => FindFile("Data/BestiaryRecipeComponent.dat"));

        private UniversalFileWrapper<BestiaryGroup> _BestiaryGroups;

        public UniversalFileWrapper<BestiaryGroup> BestiaryGroups =>
            _BestiaryGroups = _BestiaryGroups ?? 
                new UniversalFileWrapper<BestiaryGroup>(_Memory, () => FindFile("Data/BestiaryGroups.dat"));

        private UniversalFileWrapper<BestiaryFamily> _BestiaryFamilies;

        public UniversalFileWrapper<BestiaryFamily> BestiaryFamilies =>
            _BestiaryFamilies = _BestiaryFamilies ?? 
                new UniversalFileWrapper<BestiaryFamily>(_Memory, () => FindFile("Data/BestiaryFamilies.dat"));

        private UniversalFileWrapper<BestiaryGenus> _BestiaryGenuses;

        public UniversalFileWrapper<BestiaryGenus> BestiaryGenuses =>
            _BestiaryGenuses = _BestiaryGenuses ?? 
                new UniversalFileWrapper<BestiaryGenus>(_Memory, () => FindFile("Data/BestiaryGenus.dat"));

        #endregion

        #region NewAtlas

        private AtlasRegions _AtlasRegions;

        public AtlasRegions AtlasRegions =>
            _AtlasRegions = _AtlasRegions ?? new AtlasRegions(_Memory, () => FindFile("Data/AtlasRegions.dat"));

        #endregion

        public Dictionary<string, FileInformation> AllFiles { get; private set; }
        public SortedList<string, FileInformation> Metadata { get; } = new SortedList<string, FileInformation>();

        public SortedList<string, FileInformation> Data { get; private set; } =
            new SortedList<string, FileInformation>();

        public SortedList<string, FileInformation> OtherFiles { get; } = new SortedList<string, FileInformation>();

        public SortedList<string, FileInformation> LoadedInThisArea { get; private set; } =
            new SortedList<string, FileInformation>(1024);


        public event EventHandler<SortedList<string, FileInformation>> LoadedFiles;

        public void ReloadFiles()
        {
            using (new PerformanceTimer("Load files from memory"))
            {
                AllFiles = FilesFromMemory.GetAllFiles();
            }
        }

        public void ParseFiles(Dictionary<string, FileInformation> files)
        {
            foreach (var file in files)
            {
                this.Do_ParseFiles(file);
            }
        }

        public void ParseLoadedFiles(int gameAreaChangeCount)
        {
            this.LoadedInThisArea = new SortedList<string, FileInformation>(1024);

            foreach (var file in this.AllFiles)
            {
                if (file.Value.ChangeCount == gameAreaChangeCount)
                {
                    this.LoadedInThisArea[file.Key] = file.Value;
                }

                this.Do_ParseFiles(file);
            }

            this.LoadedFiles?.Invoke(this, this.LoadedInThisArea);
        }

        public long FindFile(string name)
        {
            try
            {
                if (AllFiles.TryGetValue(name, out var result))
                    return result.Ptr;
            }
            catch (KeyNotFoundException)
            {
                const string messageFormat = "Couldn't find the file in memory: {0}\nTry to restart the game.";
                MessageBox.Show(string.Format(messageFormat, name), "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            return 0;
        }

        private void Do_ParseFiles(KeyValuePair<string, FileInformation> file)
        {
            if (file.Key.StartsWith("Metadata/"))
            {
                this.Metadata[file.Key] = file.Value;
            }
            else if (file.Key.EndsWith(".dat"))
            {
                this.Data[file.Key] = file.Value;
            }
            //else if (file.Key.StartsWith("Art", StringComparison.OrdinalIgnoreCase))
            //{
            //    this.Art[file.Key] = file.Value;
            //}
            //else if (file.Key.StartsWith("Font", StringComparison.OrdinalIgnoreCase))
            //{
            //    this.Font[file.Key] = file.Value;
            //}
            else
            {
                this.OtherFiles[file.Key] = file.Value;
            }
        }
    }
}
