using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using GameOffsets;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class ActorSkill : RemoteMemoryObject
    {
        public ActorSkill()
        {
            _skillUiState = new AreaCache<long>(GetSkillUiStatePtr);
        }

        public ushort Id => M.Read<ushort>(Address + 0x10);
        public GrantedEffectsPerLevel EffectsPerLevel => ReadObject<GrantedEffectsPerLevel>(Address + 0x18);
        public bool CanBeUsedWithWeapon => M.Read<byte>(Address + 0x50) > 0;
        public bool CanBeUsed => M.Read<byte>(Address + 0x51) == 0;
        public int TotalUses => M.Read<int>(Address + 0x54);
        public float Cooldown => M.Read<int>(Address + 0x5C) / 100f; //Converted milliseconds to seconds 
        public int SoulsPerUse => M.Read<int>(Address + 0x6C);
        public int TotalVaalUses => M.Read<int>(Address + 0x70);
        public bool IsOnSkillBar => SkillSlotIndex != -1;
        public int SkillSlotIndex => TheGame.IngameState.Data.ServerData.SkillBarIds.IndexOf(Id);

        public string Name
        {
            get
            {
                var id = Id;
                var effects = EffectsPerLevel;

                if (effects != null)
                {
                    var skill = effects.SkillGemWrapper;

                    if (!string.IsNullOrEmpty(skill.Name)) return skill.Name;

                    return string.IsNullOrEmpty(skill.ActiveSkill.InternalName)
                        ? skill.ActiveSkill.InternalName
                        : Id.ToString(CultureInfo.InvariantCulture);
                }

                switch (id)
                {
                  case 0x266:
                    return "Interaction";
                  case 0x2909:
                    return "Move";
                  case 0x37d9:
                    return "WashedUp";
                  default:
                    return InternalName;
                }
            }
        }

        internal int SlotIdentifier => (Id >> 8) & 0xff;
        public int SocketIndex => (SlotIdentifier >> 2) & 15;
        public bool IsUserSkill => (SlotIdentifier & 0x80) > 0;
        public bool AllowedToCast => CanBeUsedWithWeapon && CanBeUsed;
        public byte SkillUseStage => M.Read<byte>(Address + 0x8);
        public bool IsUsing => SkillUseStage > 2;
        public bool PrepareForUsage => SkillUseStage == 1;
        public float Dps => GetStat(GameStat.HundredTimesDamagePerSecond + (IsUsing ? 4 : 0)) / 100f;
        public TimeSpan CastTime => IsInstant
            ? TimeSpan.FromMilliseconds(0)
            : TimeSpan.FromMilliseconds((int)Math.Ceiling(1000f / (HundredTimesAttacksPerSecond / 100f)));
        public int HundredTimesAttacksPerSecond
        {
            get
            {
                if (IsCry) return 60;
                if (IsInstant) return 0;

                return GetStat(
                    IsSpell
                    ? GameStat.HundredTimesCastsPerSecond
                    : GameStat.HundredTimesAttacksPerSecond
                );
            }
        }
        public bool IsSpell => GetStat(GameStat.CastingSpell) == 1;
        public bool IsAttack => GetStat(GameStat.SkillIsAttack) == 1;
        public bool IsCry => InternalName.EndsWith("_cry");
        public bool IsInstant => GetStat(GameStat.SkillIsInstant) == 1;
        public bool IsMine => GetStat(GameStat.IsRemoteMine) == 1 || GetStat(GameStat.SkillIsMined) == 1;
        public bool IsTotem => GetStat(GameStat.IsTotem) == 1 || GetStat(GameStat.SkillIsTotemified) == 1;
        public bool IsTrap => GetStat(GameStat.IsTrap) == 1 || GetStat(GameStat.SkillIsTrapped) == 1;
        public bool IsVaalSkill => SoulsPerUse >= 1 && TotalVaalUses >= 1;
        public Actor Actor { get; private set; }

        public ActorSkill SetActor(Actor actor)
        {
            Actor = actor;
            return this;
        }

        private readonly CachedValue<long> _skillUiState;

        public bool IsOnCooldown
        {
            get
            {
                var ptr = _skillUiState.Value;
                if (ptr == long.MaxValue)
                    return false;
                var state = M.Read<SkillUiStateOffsets>(ptr);
                return (state.CooldownHigh - state.CooldownLow) >> 4 >= state.NumberOfUses;
            }
        }
        public int RemainingUses
        {
            get
            {
                var ptr = _skillUiState.Value;
                if (ptr == long.MaxValue)
                    return 0;
                var state = M.Read<SkillUiStateOffsets>(ptr);
                return state.NumberOfUses - ((int)(state.CooldownHigh - state.CooldownLow) >> 4);
            }
        }

        long GetSkillUiStatePtr()
        {
            foreach(var ptr in Actor.SkillUiStateOffsets)
            {
                var state = M.Read<SkillUiStateOffsets>(ptr);
                if (state.SkillId == Id)
                    return ptr;
            }
            return long.MaxValue;
        }

        public string InternalName
        {
            get
            {
                var effects = EffectsPerLevel;

                if (effects != null)
                    return effects.SkillGemWrapper.ActiveSkill.InternalName;

                switch (Id)
                {
                  case 0x266:
                    return "Interaction";
                  case 0x2909:
                    return "Move";
                  case 0x3D79:
                    return "WashedUp";
                  default:
                    return Id.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        public Dictionary<GameStat, int> Stats
        {
            get
            {
                var statsPtr = M.Read<long>(Address + 0xA0);
                var result = new Dictionary<GameStat, int>();

                ReadStats(result, statsPtr);

                return result;
            }
        }

        internal void ReadStats(Dictionary<GameStat, int> stats, long address)
        {
            var statPtrStart = M.Read<long>(address + 0xE8);
            var statPtrEnd = M.Read<long>(address + 0xF0);

            var totalStats = (int)(statPtrEnd - statPtrStart);
            var bytes = M.ReadMem(statPtrStart, totalStats);

            for (var i = 0; i < bytes.Length; i += 8)
            {
                var key = 0;
                var value = 0;
                try
                {
                    key = BitConverter.ToInt32(bytes, i);
                    value = BitConverter.ToInt32(bytes, i + 0x04);
                }
                catch (Exception e)
                {
                    DebugWindow.LogError($"ActorSkill.ReadStats -> BitConverter failed, i: {i}");
                    DebugWindow.LogError($"ActorSkill.ReadStats -> {e.Message}");
                    continue;
                }

                stats[(GameStat)key] = value;
            }
        }

        public int GetStat(GameStat stat)
        {
            return !Stats.TryGetValue(stat, out var num) ? 0 : num;
        }

        public override string ToString()
        {
            return $"IsUsing: {IsUsing}, {Name}, Id: {Id}, InternalName: {InternalName}, CanBeUsed: {CanBeUsed}";
        }
    }
}
