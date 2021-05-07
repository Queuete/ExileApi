using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Nodes;

namespace ExileCore
{
    public class EntityCollectSettingsContainer
    {
        public Stack<Entity> Simple { get; set; }
        public Queue<uint> KeyForDelete { get; set; }
        public ConcurrentDictionary<uint, Entity> EntityCache { get; set; }
        public MultiThreadManager MultiThreadManager { get; set; }
        public Func<ToggleNode> ParseServer { get; set; }
        public Func<long> EntitiesCount { get; set; }
        public uint EntitiesVersion { get; set; }
        public bool NeedUpdate { get; set; } = true;
        public ToggleNode CollectEntitiesInParallelWhenMoreThanX { get; set; }
        public DebugInformation DebugInformation { get; set; }
        public bool Break { get; set; }
        public Func<bool> ParseEntitiesInMultiThread { get; set; }
    }

    public class EntityListWrapper
    {
        private readonly CoreSettings _settings;
        private readonly int coroutineTimeWait = 100;
        private readonly ConcurrentDictionary<uint, Entity> entityCache;
        private readonly GameController gameController;
        private readonly Queue<uint> keysForDelete = new Queue<uint>(24);

        private readonly Coroutine parallelUpdateDictionary;
        private readonly Stack<Entity> Simple = new Stack<Entity>(512);
        private readonly Coroutine updateEntity;
        private readonly EntityCollectSettingsContainer entityCollectSettingsContainer;
        private static EntityListWrapper _instance;
        public EntityListWrapper(GameController gameController, CoreSettings settings, MultiThreadManager multiThreadManager)
        {
            _instance = this;
            this.gameController = gameController;
            _settings = settings;

            entityCache = new ConcurrentDictionary<uint, Entity>();
            gameController.Area.OnAreaChange += AreaChanged;
            EntitiesVersion = 0;

            updateEntity =
                new Coroutine(RefreshState, new WaitTime(coroutineTimeWait), null, "Update Entity")
                    {Priority = CoroutinePriority.High, SyncModWork = true};

            var collectEntitiesDebug = new DebugInformation("Collect Entities");

            entityCollectSettingsContainer = new EntityCollectSettingsContainer();
            entityCollectSettingsContainer.Simple = Simple;
            entityCollectSettingsContainer.KeyForDelete = keysForDelete;
            entityCollectSettingsContainer.EntityCache = entityCache;
            entityCollectSettingsContainer.MultiThreadManager = multiThreadManager;
            entityCollectSettingsContainer.ParseServer = () => settings.ParseServerEntities;
            entityCollectSettingsContainer.ParseEntitiesInMultiThread = () => settings.ParseEntitiesInMultiThread;
            entityCollectSettingsContainer.EntitiesCount = () => gameController.IngameState.Data.EntitiesCount;
            entityCollectSettingsContainer.EntitiesVersion = EntitiesVersion;
            entityCollectSettingsContainer.CollectEntitiesInParallelWhenMoreThanX = settings.CollectEntitiesInParallelWhenMoreThanX;
            entityCollectSettingsContainer.DebugInformation = collectEntitiesDebug;

            IEnumerator Test()
            {
                while (true)
                {
                    yield return gameController.IngameState.Data.EntityList.CollectEntities(entityCollectSettingsContainer);
                    yield return new WaitTime(1000 / settings.EntitiesUpdate);
                    parallelUpdateDictionary.UpdateTicks((uint) (parallelUpdateDictionary.Ticks + 1));
                }
            }

            parallelUpdateDictionary = new Coroutine(Test(), null, "Collect entites") {SyncModWork = true};
            UpdateCondition(1000 / settings.EntitiesUpdate);

            settings.EntitiesUpdate.OnValueChanged += (sender, i) => { UpdateCondition(1000 / i); };

            var enumValues = typeof(EntityType).GetEnumValues();
            ValidEntitiesByType = new Dictionary<EntityType, List<Entity>>(enumValues.Length);

            foreach (EntityType enumValue in enumValues)
            {
                ValidEntitiesByType[enumValue] = new List<Entity>(8);
            }

            PlayerUpdate += (sender, entity) => Entity.Player = entity;
        }

        public ICollection<Entity> Entities => entityCache.Values;
        public uint EntitiesVersion { get; }
        public Entity Player { get; private set; }
        public List<Entity> OnlyValidEntities { get; } = new List<Entity>(500);
        public List<Entity> NotOnlyValidEntities { get; } = new List<Entity>(500);
        public Dictionary<uint, Entity> NotValidDict { get; } = new Dictionary<uint, Entity>(500);
        public Dictionary<EntityType, List<Entity>> ValidEntitiesByType { get; }

        public void StartWork()
        {
            Core.MainRunner.Run(updateEntity);
            Core.ParallelRunner.Run(parallelUpdateDictionary);
        }

        private void UpdateCondition(int coroutineTimeWait = 50)
        {
            parallelUpdateDictionary.UpdateCondtion(new WaitTime(coroutineTimeWait));
            updateEntity.UpdateCondtion(new WaitTime(coroutineTimeWait));
        }

#pragma warning disable CS0067
        public event Action<Entity> EntityAdded;
        public event Action<Entity> EntityAddedAny;
        public event Action<Entity> EntityIgnored;
        public event Action<Entity> EntityRemoved;
#pragma warning restore CS0067

        private void AreaChanged(AreaInstance area)
        {
            try
            {
                entityCollectSettingsContainer.Break = true;
                var dataLocalPlayer = gameController.Game.IngameState.Data.LocalPlayer;

                if (Player == null)
                {
                    if (dataLocalPlayer.Path.StartsWith("Meta"))
                    {
                        Player = dataLocalPlayer;
                        Player.IsValid = true;
                        PlayerUpdate?.Invoke(this, Player);
                    }
                }
                else
                {
                    if (Player.Address != dataLocalPlayer.Address)
                    {
                        if (dataLocalPlayer.Path.StartsWith("Meta"))
                        {
                            Player = dataLocalPlayer;
                            Player.IsValid = true;
                            PlayerUpdate?.Invoke(this, Player);
                        }
                    }
                }

                entityCache.Clear();
                OnlyValidEntities.Clear();
                NotOnlyValidEntities.Clear();

                foreach (var e in ValidEntitiesByType)
                {
                    e.Value.Clear();
                }
            }
            catch (Exception e)
            {
                DebugWindow.LogError($"{nameof(EntityListWrapper)} -> {e}");
            }
        }

        private void UpdateEntityCollections()
        {
            OnlyValidEntities.Clear();
            NotOnlyValidEntities.Clear();
            NotValidDict.Clear();

            foreach (var e in ValidEntitiesByType)
            {
                e.Value.Clear();
            }

            while (keysForDelete.Count > 0)
            {
                var key = keysForDelete.Dequeue();

                if (entityCache.TryGetValue(key, out var entity))
                {
                    EntityRemoved?.Invoke(entity);
                    entityCache.TryRemove(key, out _);
                }
            }

            foreach (var entity in entityCache)
            {
                var entityValue = entity.Value;

                if (entityValue.IsValid)
                {
                    OnlyValidEntities.Add(entityValue);
                    ValidEntitiesByType[entityValue.Type].Add(entityValue);
                }
                else
                {
                    NotOnlyValidEntities.Add(entityValue);
                    NotValidDict[entityValue.Id] = entityValue;

                }
            }
        }

        public void RefreshState()
        {
            if (gameController.Area.CurrentArea == null) return;
            if (entityCollectSettingsContainer.NeedUpdate) return;
            if (Player == null || !Player.IsValid) return;

            while (Simple.Count > 0)
            {
                var entity = Simple.Pop();

                if (entity == null)
                {
                    DebugWindow.LogError($"{nameof(EntityListWrapper)}.{nameof(RefreshState)} entity is null. (Very strange).");
                    continue;
                }

                var entityId = entity.Id;
                if (entityCache.TryGetValue(entityId, out _)) continue;

                if (entityId >= int.MaxValue && !_settings.ParseServerEntities)
                    continue;

                if (entity.Type == EntityType.Error)
                    continue;

                EntityAddedAny?.Invoke(entity);
                if ((int) entity.Type >= 100) EntityAdded?.Invoke(entity);

                entityCache[entityId] = entity;
            }

            UpdateEntityCollections();
            entityCollectSettingsContainer.NeedUpdate = true;
        }

        public event EventHandler<Entity> PlayerUpdate;

        public static Entity GetEntityById(uint id)
        {
            return _instance.entityCache.TryGetValue(id, out var result) ? result : null;
        }

        public string GetLabelForEntity(Entity entity)
        {
            var hashSet = new HashSet<long>();
            var entityLabelMap = gameController.Game.IngameState.EntityLabelMap;
            var num = entityLabelMap;

            while (true)
            {
                hashSet.Add(num);
                if (gameController.Memory.Read<long>(num + 0x10) == entity.Address) break;

                num = gameController.Memory.Read<long>(num);
                if (hashSet.Contains(num) || num == 0 || num == -1) return null;
            }

            return gameController.Game.ReadObject<EntityLabel>(num + 0x18).Text;
        }
    }
}
