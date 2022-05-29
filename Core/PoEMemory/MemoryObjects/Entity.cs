using System;
using System.Collections.Generic;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using GameOffsets;
using SharpDX;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class Entity : RemoteMemoryObject
    {
        private readonly Dictionary<Type, Component> _cacheComponents = new Dictionary<Type, Component>(4);
        private readonly CachedValue<bool> _hiddenCheckCache;
        private Vector3 _boundsCenterPos = Vector3.Zero;
        private Dictionary<string, long> _cacheComponents2;
        private float _distancePlayer = float.MaxValue;
        private EntityOffsets? _entityOffsets;
        private Vector2 _gridPos = Vector2.Zero;
        private long? _id;
        private uint? _inventoryId;
        private bool _isAlive;
        private bool _isDead;
        private CachedValue<bool> _isHostile;
        private bool _isOpened;
        private bool _isTargetable;
        private string _metadata;
        private string _path;
        private Vector3 _pos = Vector3.Zero;
        private MonsterRarity? _rarity;
        private string _renderName = "Empty";
        private Dictionary<GameStat, int> _stats;
        private readonly ValidCache<List<Buff>> buffCache;
        private bool isHidden;
        private readonly object locker = new object();
        private int pathReadErrorTimes;

        public Entity()
        {
            _hiddenCheckCache = new LatancyCache<bool>(() =>
            {
                if (IsValid)
                    isHidden = HasComponent<Buffs>() && GetComponent<Buffs>().HasBuff("hidden_monster");

                return isHidden;
            }, 50);

            buffCache = this.ValidCache(() => GetComponent<Buffs>()?.BuffsList);
        }

        public static Entity Player { get; set; }
        private static Dictionary<string, string> ComponentsName { get; } = new Dictionary<string, string>();

        public float DistancePlayer
        {
            get
            {
                if (Player == null)
                    return _distancePlayer;

                if (IsValid)
                {
                    _distancePlayer = Player.GridPos.Distance(GridPos);
                    return _distancePlayer;
                }

                _distancePlayer = Player.GridPos.Distance(GridPos);
                return _distancePlayer;
            }
        }

        public EntityOffsets EntityOffsets
        {
            get
            {
                if (_entityOffsets != null)
                    return _entityOffsets.Value;

                if (Address != 0)
                    _entityOffsets = M.Read<EntityOffsets>(Address);

                if (_entityOffsets != null) return _entityOffsets.Value;
                IsValid = false;
                return default;
            }
        }

        public EntityType Type { get; private set; }
        public LeagueType League { get; private set; } = LeagueType.General;
        public bool IsHidden => _hiddenCheckCache.Value;
        public string Debug => $"{EntityOffsets.EntityDetailsPtr:X} List: {EntityOffsets.ComponentListPtr:X}";
        public uint Version { get; set; }
        public bool IsValid { get; set; }

        public bool IsAlive
        {
            get
            {
                if (!IsValid)
                    return _isAlive;

                var life = GetComponent<Life>();

                if (life == null || life.OwnerAddress != Address)
                {
                    if (_distancePlayer < 70)
                        _isAlive = false;

                    return _isAlive;
                }

                _isAlive = life.CurHP > 0;
                return _isAlive;
            }
        }

        public Vector3 Pos
        {
            get
            {
                if (!IsValid)
                    return _pos;

                var render = GetComponent<Render>();

                if (render == null)
                    return _pos;

                _pos.X = render.X;
                _pos.Y = render.Y;
                _pos.Z = render.Z + render.Bounds.Z;
                return _pos;
            }
        }

        public Vector3 BoundsCenterPos
        {
            get
            {
                if (!IsValid)
                    return _boundsCenterPos;

                var render = GetComponent<Render>();

                if (render == null)
                    return _boundsCenterPos;

                _boundsCenterPos = render.InteractCenter;
                return _boundsCenterPos;
            }
        }

        public Vector2 GridPos
        {
            get
            {
                if (!IsValid)
                    return _gridPos;

                var positioned = GetComponent<Positioned>();

                if (positioned == null)
                    return _gridPos;

                if (positioned.OwnerAddress != Address)
                    return _gridPos;

                _gridPos = positioned.GridPos;
                return _gridPos;
            }
        }

        public string RenderName
        {
            get
            {
                if (!IsValid)
                    return _renderName;

                var render = GetComponent<Render>();

                if (render == null)
                    return _renderName;

                _renderName = render.Name;
                return _renderName;
            }
        }

        public MonsterRarity Rarity => (MonsterRarity)(_rarity = _rarity ?? (GetComponent<ObjectMagicProperties>()?.Rarity ?? MonsterRarity.White));

        public bool IsOpened
        {
            get
            {
                if (!IsValid)
                    return _isOpened;

                var chest = GetComponent<Chest>();

                if (chest == null)
                    return _isOpened;

                var targetable = GetComponent<Targetable>();

                if (targetable == null)
                    return _isOpened;

                _isOpened = !targetable.isTargetable || chest.IsOpened;
                return _isOpened;
            }
        }

        public bool IsDead
        {
            get
            {
                if (!IsValid)
                    return _isDead;

                _isDead = !IsAlive;
                return _isDead;
            }
        }

        public Dictionary<GameStat, int> Stats
        {
            get
            {
                if (!IsValid)
                    return _stats;

                var stats = GetComponent<Stats>();

                if (stats == null)
                    return _stats;

                if (stats.OwnerAddress != Address)
                {
                    stats = GetComponentFromMemory<Stats>();

                    if (stats.OwnerAddress != Address)
                        return _stats;
                }

                var statsStatDictionary = stats.StatDictionary;

                if (statsStatDictionary.Count == 0 && (_stats == null || _stats.Count != 0))
                    return _stats;

                _stats = statsStatDictionary;

                return _stats;
            }
        }

        public bool IsTargetable
        {
            get
            {
                if (!IsValid)
                {
                    if (_isTargetable && DistancePlayer < 100)
                        _isTargetable = false;

                    return _isTargetable;
                }

                var targetable = GetComponent<Targetable>();
                _isTargetable = targetable != null && targetable.isTargetable;
                return _isTargetable;
            }
        }

        public bool IsTransitioned => IsTransitionedHelper();
        private bool IsTransitionedHelper()
        {
            var transitionable = GetComponent<Transitionable>();
            var flag = transitionable?.Flag1; //1, 2
            if (!flag.HasValue) return false;
            return flag.Value == 2;
        }

        public List<Buff> Buffs => buffCache.Value;
        private string CachePath { get; set; }

        public string Path
        {
            get
            {
                if (_path == null)
                {
                    var entityDetails = M.Read<EntityDetails>(EntityOffsets.EntityDetailsPtr);

                    if (entityDetails.PathName.Path == 0)
                    {
                        if (CachePath == null)
                        {
                            IsValid = false;
                            return null;
                        }

                        return CachePath;
                    }

                    _path = Cache.StringCache.Read($"{entityDetails.PathName.Path}{entityDetails.PathName.Length}", () => entityDetails.PathName.ToString(M));

                    //  _path = p.ToString(M);
                    //_path= M.Read<PathEntityOffsets>(EntityOffsets.Head.MainObject).ToString(M);
                    if (!_path.StartsWith("Metadata"))
                    {
                        _path = entityDetails.PathName.ToString(M);

                        // Cache.StringCache.Remove($"{nameof(Entity)}{EntityOffsets.Head.MainObject}");
                        Cache.StringCache.Remove($"{entityDetails.PathName.Path}{entityDetails.PathName.Length}");
                    }

                    if (_path.Length > 0 && _path[0] != 'M')
                    {
                        pathReadErrorTimes++;
                        IsValid = false;
                        _path = null;

                        if (pathReadErrorTimes > 10)
                        {
                            _path = "ERROR PATH";
                            DebugWindow.LogError("Entity path error.");
                        }
                    }
                    else
                        CachePath = _path;
                }

                return _path;
            }
        }

        public string Metadata
        {
            get
            {
                if (_metadata != null || Path == null) return _metadata;

                var splitIndex = Path.IndexOf("@", StringComparison.Ordinal);

                return splitIndex != -1 ? _metadata = Path.Substring(0, splitIndex) : Path;
            }
        }

        public uint Id => (uint)(_id = _id ?? M.Read<uint>(Address + 0x60));
        public uint InventoryId => (uint)(_inventoryId = _inventoryId ?? M.Read<uint>(Address + 0x70));

        //public bool IsValid => M.Read<int>(EntityOffsets.Head.MainObject+0x18,0) == 0x65004D;
        public Dictionary<string, long> CacheComp => _cacheComponents2 ?? (_cacheComponents2 = GetComponents());
        public bool IsHostile =>
            _isHostile?.Value ?? (_isHostile = new TimeCache<bool>(() => (GetComponent<Positioned>()?.Reaction & 0x7f) != 1, 100)).Value;
        private Dictionary<Type, object> PluginData { get; } = new Dictionary<Type, object>();
        public event EventHandler<Entity> OnUpdate;

        public override string ToString()
        {
            return $"<{Type}> ({Rarity}) {Metadata}: ({Address:X})";
        }

        public float Distance(Entity entity)
        {
            return GridPos.Distance(entity.GridPos);
        }

        protected override void OnAddressChange()
        {
            _entityOffsets = M.Read<EntityOffsets>(Address);

            // _id = null;
            _inventoryId = null;
            _pos = Vector3.Zero;
            _cacheComponents.Clear();
            _cacheComponents2 = null;

            if (Type == EntityType.Error)
            {
                Type = ParseType();
                if (Type != EntityType.Error) IsValid = true;
            }

            OnUpdate?.Invoke(this, this);
        }

        public bool Check(uint entityId)
        {
          if (_id != null && _id != entityId)
            {
                DebugWindow.LogMsg($"Was ID: {Id} New ID: {entityId} To Path: {Path}", 3);
                _id = entityId;
                _path = null;
                _metadata = null;
                Type = ParseType();
            }

          switch (Type)
          {
            case EntityType.Error:
              return false;
            case EntityType.Effect:
            case EntityType.Daemon:
              return true;
            default:
              return CacheComp != null && Id == entityId && CheckRarity();
          }
        }

        private bool CheckRarity()
        {
            return Rarity >= MonsterRarity.White && Rarity <= MonsterRarity.Unique;
        }

        public void UpdatePointer(long newAddress)
        {
            Address = newAddress;
        }

        private Dictionary<string, long> GetComponents()
        {
            lock (locker)
            {
                var result = new Dictionary<string, long>();

                var entityComponent = M.ReadPointersArray(EntityOffsets.ComponentListPtr.First, EntityOffsets.ComponentListPtr.Last);
                var entityDetails = M.Read<EntityDetails>(EntityOffsets.EntityDetailsPtr);
                var lookupPtr = M.Read<ComponentLookup>(entityDetails.ComponentLookupPtr);
                if (lookupPtr.Capacity < 1)
                {
                    return result;
                }

                foreach (var bucket in M.ReadAsArray<ComponentArrayStructure>(lookupPtr.ComponentArray, ((int)lookupPtr.Capacity + 1) / 8))
                {
                    if (bucket.Flag0 != ComponentArrayStructure.InvalidPointerFlagValue)
                    {
                        var name = M.ReadString(bucket.Pointer0.NamePtr);
                        if (!string.IsNullOrWhiteSpace(name) && !result.ContainsKey(name) && bucket.Pointer0.Index >= 0 && bucket.Pointer0.Index < entityComponent.Count)
                        {
                            result.Add(name, entityComponent[bucket.Pointer0.Index]);
                        }
                    }
                    if (bucket.Flag1 != ComponentArrayStructure.InvalidPointerFlagValue)
                    {
                        var name = M.ReadString(bucket.Pointer1.NamePtr);
                        if (!string.IsNullOrWhiteSpace(name) && !result.ContainsKey(name) && bucket.Pointer1.Index >= 0 && bucket.Pointer1.Index < entityComponent.Count)
                        {
                            result.Add(name, entityComponent[bucket.Pointer1.Index]);
                        }
                    }
                    if (bucket.Flag2 != ComponentArrayStructure.InvalidPointerFlagValue)
                    {
                        var name = M.ReadString(bucket.Pointer2.NamePtr);
                        if (!string.IsNullOrWhiteSpace(name) && !result.ContainsKey(name) && bucket.Pointer2.Index >= 0 && bucket.Pointer2.Index < entityComponent.Count)
                        {
                            result.Add(name, entityComponent[bucket.Pointer2.Index]);
                        }
                    }
                    if (bucket.Flag3 != ComponentArrayStructure.InvalidPointerFlagValue)
                    {
                        var name = M.ReadString(bucket.Pointer3.NamePtr);
                        if (!string.IsNullOrWhiteSpace(name) && !result.ContainsKey(name) && bucket.Pointer3.Index >= 0 && bucket.Pointer3.Index < entityComponent.Count)
                        {
                            result.Add(name, entityComponent[bucket.Pointer3.Index]);
                        }
                    }
                    if (bucket.Flag4 != ComponentArrayStructure.InvalidPointerFlagValue)
                    {
                        var name = M.ReadString(bucket.Pointer4.NamePtr);
                        if (!string.IsNullOrWhiteSpace(name) && !result.ContainsKey(name) && bucket.Pointer4.Index >= 0 && bucket.Pointer4.Index < entityComponent.Count)
                        {
                            result.Add(name, entityComponent[bucket.Pointer4.Index]);
                        }
                    }
                    if (bucket.Flag5 != ComponentArrayStructure.InvalidPointerFlagValue)
                    {
                        var name = M.ReadString(bucket.Pointer5.NamePtr);
                        if (!string.IsNullOrWhiteSpace(name) && !result.ContainsKey(name) && bucket.Pointer5.Index >= 0 && bucket.Pointer5.Index < entityComponent.Count)
                        {
                            result.Add(name, entityComponent[bucket.Pointer5.Index]);
                        }
                    }
                    if (bucket.Flag6 != ComponentArrayStructure.InvalidPointerFlagValue)
                    {
                        var name = M.ReadString(bucket.Pointer6.NamePtr);
                        if (!string.IsNullOrWhiteSpace(name) && !result.ContainsKey(name) && bucket.Pointer6.Index >= 0 && bucket.Pointer6.Index < entityComponent.Count)
                        {
                            result.Add(name, entityComponent[bucket.Pointer6.Index]);
                        }
                    }
                    if (bucket.Flag7 != ComponentArrayStructure.InvalidPointerFlagValue)
                    {
                        var name8 = M.ReadString(bucket.Pointer7.NamePtr);
                        if (!string.IsNullOrWhiteSpace(name8) && !result.ContainsKey(name8) && bucket.Pointer7.Index >= 0 && bucket.Pointer7.Index < entityComponent.Count)
                        {
                            result.Add(name8, entityComponent[bucket.Pointer7.Index]);
                        }
                    }
                }

                return result;
            }
        }

        public bool HasComponent<T>() where T : Component, new()
        {
            return CacheComp != null && CacheComp.TryGetValue(typeof(T).Name, out var address) && address != 0;
        }

        /*public bool HasComponent<T>() where T : Component, new()
        {
            string name = typeof(T).Name;
            long componentLookup = ComponentLookup;
            var addr = M.Read<long>(componentLookup);
            int i = 0;
            while (!M.ReadString(M.Read<long>(addr + 0x10)).Equals(name))
            {
                if (addr == 0)
                    return false;
                addr = M.Read<long>(addr);
                ++i;
                if (addr == componentLookup || addr == 0 || addr == -1 || i >= 200)
                    return false;
            }

            CacheComp[name] = M.Read<long>(ComponentList + M.Read<int>(addr+0x18)*8);
            return true;
        }*/

        public T GetComponent<T>() where T : Component, new()
        {
            if (_cacheComponents.TryGetValue(typeof(T), out var result)) return (T)result;

            if (CacheComp != null && CacheComp.TryGetValue(typeof(T).Name, out var address))
            {
                var component = GetObject<T>(address);
                _cacheComponents[typeof(T)] = component;
                return component;
            }

            return null;
        }

        public bool TryGetComponent<T>(out T component) where T : Component, new()
        {
            component = default(T);

            if (this._cacheComponents.TryGetValue(typeof(T), out var result))
            {
                component = (T)result;
                return true;
            }

            if (this.CacheComp.TryGetValue(typeof(T).Name, out var componentAddress))
            {
                component = this.GetObject<T>(componentAddress);

                if (component != null)
                {
                    this._cacheComponents[typeof(T)] = component;
                    return true;
                }
            }

            return false;
        }

        public bool CheckComponentForValid<T>() where T : Component, new()
        {
            var c = GetComponent<T>();

            if (c.OwnerAddress == Address) return true;
            var componentFromMemory = GetComponentFromMemory<T>();
            return componentFromMemory.OwnerAddress == Address;
        }

        public T GetComponentFromMemory<T>() where T : Component, new()
        {
            if (CacheComp.TryGetValue(typeof(T).Name, out var address))
            {
                var component = GetObject<T>(address);
                _cacheComponents[typeof(T)] = component;
                return component;
            }

            return null;
        }

        private EntityType ParseType()
        {
            // if (EntityOffsets.ComponentList <= 0) return EntityType.Error;

            if (string.IsNullOrEmpty(Path)) return EntityType.Error;

            if (Path.StartsWith("Metadata/Effects/", StringComparison.Ordinal)) return EntityType.Effect;

            if (Path.StartsWith("Metadata/Monsters/Daemon/", StringComparison.Ordinal)) return EntityType.Daemon;

            if (Version > 0 && Id > int.MaxValue)
                return EntityType.ServerObject;

            if (HasComponent<Chest>())
            {
                if (Path.StartsWith("Metadata/Chests/DelveChests", StringComparison.Ordinal))
                {
                    League = LeagueType.Delve;
                    return EntityType.Chest;
                }

                if (Path.StartsWith("Metadata/Chests/Incursion", StringComparison.Ordinal))
                {
                    League = LeagueType.Incursion;
                    return EntityType.Chest;
                }

                if (Path.StartsWith("Metadata/Chests/Legion", StringComparison.Ordinal))
                {
                    League = LeagueType.Legion;
                    return EntityType.Chest;
                }

                return EntityType.Chest;
            }

            if (HasComponent<NPC>() && Path.StartsWith("Metadata/NPC", StringComparison.Ordinal))
                return EntityType.Npc;

            if (HasComponent<Monster>())
            {
                if (Path.StartsWith("Metadata/Monsters/LegionLeague/", StringComparison.Ordinal))
                    League = LeagueType.Legion;
                if (Path.StartsWith("Metadata/Monsters/LeagueAffliction/", StringComparison.Ordinal))
                    League = LeagueType.Delirium;

                return EntityType.Monster;
            }

            if (HasComponent<Shrine>())
                return EntityType.Shrine;

            if (HasComponent<WorldItem>())
                return EntityType.WorldItem;

            if (HasComponent<Player>())
                return EntityType.Player;

            if (Path.StartsWith("Metadata/MiscellaneousObjects/Harvest", StringComparison.Ordinal) || Path.StartsWith("Metadata/Terrain/Leagues/Harvest", StringComparison.Ordinal))
            {
                League = LeagueType.Harvest;
                return EntityType.MiscellaneousObjects;
            }

            if (HasComponent<MinimapIcon>())
            {
                if (Path.Equals("Metadata/Terrain/Missions/Hideouts/Objects/HideoutCraftingBench", StringComparison.Ordinal))
                    return EntityType.CraftUnlock;

                if (HasComponent<AreaTransition>()) return EntityType.AreaTransition;

                if (Path.EndsWith("Waypoint", StringComparison.Ordinal)) return EntityType.Waypoint;

                if (HasComponent<Portal>()) return EntityType.TownPortal;

                if (HasComponent<Monolith>()) return EntityType.Monolith;

                if (HasComponent<Transitionable>() && Path.StartsWith("Metadata/MiscellaneousObjects/Abyss"))
                {
                    League = LeagueType.Abyss;
                    return EntityType.MiscellaneousObjects;
                }

                if (Path.Equals("Metadata/Terrain/Leagues/Legion/Objects/LegionInitiator", StringComparison.Ordinal))
                    return EntityType.LegionMonolith;

                if (Path.Equals("Metadata/MiscellaneousObjects/Stash", StringComparison.Ordinal)) return EntityType.Stash;
                if (Path.Equals("Metadata/MiscellaneousObjects/GuildStash", StringComparison.Ordinal)) return EntityType.GuildStash;

                if (Path.Equals("Metadata/MiscellaneousObjects/Delve/DelveCraftingBench", StringComparison.Ordinal))
                    return EntityType.DelveCraftingBench;

                if (Path.Equals("Metadata/MiscellaneousObjects/Breach/BreachObject", StringComparison.Ordinal)) return EntityType.Breach;
                if (Path.Equals("Metadata/Terrain/Leagues/Delve/Objects/DelveMineral")) return EntityType.Resource;

                return EntityType.IngameIcon;
            }

            if (HasComponent<Portal>()) return EntityType.Portal;

            if (HasComponent<HideoutDoodad>()) return EntityType.HideoutDecoration;

            if (HasComponent<Monolith>()) return EntityType.MiniMonolith;

            if (HasComponent<ClientBetrayalChoice>()) return EntityType.BetrayalChoice;

            if (HasComponent<RenderItem>()) return EntityType.Item;

            if (Path.StartsWith("Metadata/MiscellaneousObjects/Lights", StringComparison.Ordinal)) return EntityType.Light;

            if (Path.StartsWith("Metadata/Terrain", StringComparison.Ordinal)) return EntityType.Terrain;

            if (Path.StartsWith("Metadata/Pet", StringComparison.Ordinal)) return EntityType.Pet;

            return EntityType.None;
        }

        public T GetHudComponent<T>() where T : class
        {
            if (PluginData.TryGetValue(typeof(T), out var result)) return (T)result;
            return null;
        }

        public void SetHudComponent<T>(T data)
        {
            lock (locker)
            {
                PluginData[typeof(T)] = data;
            }
        }
    }
}
