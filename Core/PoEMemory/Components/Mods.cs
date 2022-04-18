namespace ExileCore.PoEMemory.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExileCore.PoEMemory.MemoryObjects;
    using ExileCore.PoEMemory.Models;
    using ExileCore.Shared.Cache;
    using ExileCore.Shared.Enums;
    using ExileCore.Shared.Helpers;
    using GameOffsets;
    using GameOffsets.Native;

    public class Mods : Component
    {
        private readonly CachedValue<ModsComponentOffsets> _cachedData;
        private readonly CachedValue<ModsComponentDetailsOffsets> _cachedDetails;

        public Mods()
        {
            _cachedData =
                new FrameCache<ModsComponentOffsets>(() => M.Read<ModsComponentOffsets>(Address));
            _cachedDetails =
                new FrameCache<ModsComponentDetailsOffsets>(() => M.Read<ModsComponentDetailsOffsets>(_cachedData.Value.ModsComponentDetailsKey));
        }

        private ModsComponentOffsets _data => _cachedData.Value;

        private ModsComponentDetailsOffsets _details => _cachedDetails.Value;

        private Lazy<List<ItemMod>> _enchantedMods =>
            new Lazy<List<ItemMod>>(() => GetMods(_data.EnchantedModsArray).ToList());

        private Lazy<List<ItemMod>> _explicitMods =>
            new Lazy<List<ItemMod>>(() => GetMods(_data.ExplicitModsArray).ToList());

        private Lazy<List<ItemMod>> _implicitMods =>
            new Lazy<List<ItemMod>>(() => GetMods(_data.ImplicitModsArray).ToList());

        private Lazy<List<ItemMod>> _scourgedMods =>
            new Lazy<List<ItemMod>>(() => GetMods(_data.ScourgeModsArray).ToList());

        [Obsolete("Use FracturedCount", false)]
        public int CountFractured => FracturedCount;

        public IEnumerable<ItemMod> EnchantedMods => _enchantedMods.Value;

        public IEnumerable<ItemMod> ExplicitMods => _explicitMods.Value;

        public int FracturedCount => HumanFracturedStats.Count;

        public long Hash => _data.ImplicitModsArray.GetHashCode()
          ^ _data.ExplicitModsArray.GetHashCode()
          ^ _data.GetHashCode();

        [Obsolete("Use IsFractured", false)]
        public bool HaveFractured => IsFractured;

        public List<string> HumanCraftedStats => GetStats(_details.CraftedStatsArray);

        public List<string> HumanEnchantedStats => GetStats(_details.EnchantedStatsArray);

        public List<string> HumanFracturedStats => GetStats(_details.FracturedStatsArray);

        public List<string> HumanImpStats => GetStats(_details.ImplicitStatsArray);

        public List<string> HumanScourgeStats => GetStats(_details.ScourgeStatsArray);

        public List<string> HumanStats => GetStats(_details.ExplicitStatsArray);

        public bool Identified => Address != 0 && _data.Identified;

        public IEnumerable<ItemMod> ImplicitMods => _implicitMods.Value;

        public short IncubatorKills => _data.IncubatorKillCount;

        public string IncubatorName =>
            Address != 0 && _data.IncubatorKey != 0 ? M.ReadStringU(M.Read<long>(_data.IncubatorKey, 0x20)) : null;

        public bool IsFractured => FracturedCount > 0;

        public bool IsMirrored => Address != 0 && _data.IsMirrored == 1;

        public bool IsSplit => Address != 0 && _data.IsSplit == 1;

        public bool IsSynthesized => Address != 0 && _data.IsSynthesised == 1;

        public bool IsTalisman => ItemMods != null && ItemMods.Any(x => x.RawName.StartsWith("Talisman"));

        public bool IsUsable => Address != 0 && _data.IsUsable == 1;

        public bool IsVeiled => VeiledCount > 0;

        public int ItemLevel => Address != 0 ? _data.ItemLevel : 1;

        public List<ItemMod> ItemMods => ScourgedMods.Concat(EnchantedMods, ImplicitMods, ExplicitMods).ToList();

        public ItemRarity ItemRarity => Address != 0 ? (ItemRarity)_data.ItemRarity : ItemRarity.Normal;

        public ItemStats ItemStats => new ItemStats(Owner);

        public int RequiredLevel => Address != 0 ? _data.RequiredLevel : 1;

        public IEnumerable<ItemMod> ScourgedMods => _scourgedMods.Value;

        [Obsolete("Use IsSynthesized", false)]
        public bool Synthesised => IsSynthesized;

        public int SynthesizedCount => IsSynthesized ? HumanImpStats.Count : 0;

        public int TalismanCount => IsTalisman ? HumanImpStats.Count : 0;

        public string UniqueName => GetUniqueName(_data.UniqueName);

        public int VeiledCount => ItemMods?.Count(x => x.Group.StartsWith("Veiled")) ?? 0;

        private IEnumerable<ItemMod> GetMods(NativePtrArray source)
        {
            var mods = new List<ItemMod>();
            if (Address == 0)
            {
                return mods;
            }

            if (source.Size / ModsComponentOffsets.ItemModRecordSize > 24)
            {
                return mods;
            }

            for (var modAddress = source.First;
                 modAddress < source.Last;
                 modAddress += ModsComponentOffsets.ItemModRecordSize)
            {
                mods.Add(GetObject<ItemMod>(modAddress));
            }

            return mods;
        }

        private List<string> GetStats(NativePtrArray source)
        {
            var stats = new List<string>();
            if (Address == 0)
            {
                return stats;
            }

            var readPointersArray =
                M.ReadPointersArray(source.First, source.Last, ModsComponentOffsets.StatRecordSize);

            stats.AddRange(readPointersArray.Select(statAddress =>
                Cache.StringCache.Read($"{nameof(Mods)}{statAddress}", () => M.ReadStringU(statAddress))));

            return stats;
        }

        private string GetUniqueName(NativePtrArray source)
        {
            var words = new List<string>();
            if (Address == 0)
            {
                return string.Empty;
            }

            for (var first = source.First; first < source.Last; first += ModsComponentOffsets.NameRecordSize)
            {
                words.Add(M.ReadStringU(M.Read<long>(first, ModsComponentOffsets.NameOffset)).Trim());
            }

            return Cache.StringCache.Read($"{nameof(Mods)}{source.First}", () => string.Join(" ", words.ToArray()));
        }
    }
}
