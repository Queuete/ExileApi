using System;
using System.Collections.Generic;
using System.Globalization;
using ExileCore.Shared.Enums;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class ActorSkill : RemoteMemoryObject
    {
        public ushort Id => M.Read<ushort>(Address + 0x10);
        public GrantedEffectsPerLevel EffectsPerLevel => ReadObject<GrantedEffectsPerLevel>(Address + 0x20);
        public bool CanBeUsedWithWeapon => M.Read<byte>(Address + 0x52) > 0; // might be 0x50
        public bool CanBeUsed => M.Read<byte>(Address + 0x50) == 0; // might be 0x51
        public int Cost => M.Read<byte>(Address + 0x4C); // might be 0x54
        public int TotalUses => M.Read<int>(Address + 0x58);
        public float Cooldown => M.Read<int>(Address + 0x60) / 100f; //Converted milliseconds to seconds 
        public int SoulsPerUse => M.Read<int>(Address + 0x70);
        public int TotalVaalUses => M.Read<int>(Address + 0x74);
        public bool IsOnSkillBar => SkillSlotIndex != -1;
        public int SkillSlotIndex => TheGame.IngameState.ServerData.SkillBarIds.IndexOf(Id);

        public string Name
        {
            get
            {
                var id = Id;
                var effects = EffectsPerLevel;

                if (effects != null)
                {
                    var skill = effects.SkillGemWrapper;
                    var name = skill.Name;

                    if (string.IsNullOrEmpty(name))
                    {
                        name = skill.ActiveSkill.InternalName;

                        if (string.IsNullOrEmpty(name))
                            name = Id.ToString(CultureInfo.InvariantCulture);
                    }

                    return name;
                }
                else
                {
                    string name;

                    switch (id)
                    {
                        case 0x266:
                            name = "Interaction";
                            break;

                        case 0x2909:
                            name = "Move";
                            break;

                        default:
                            if (id != 0x37d9)
                                name = InternalName;
                            else
                                name = "WashedUp";

                            break;
                    }

                    return name;
                }
            }
        }

        internal int SlotIdentifier => (Id >> 8) & 0xff;
        public int SocketIndex => (SlotIdentifier >> 2) & 15;
        public bool IsUserSkill => (SlotIdentifier & 0x80) > 0;
        public bool AllowedToCast => CanBeUsedWithWeapon && CanBeUsed;
        public byte SkillUseStage => M.Read<byte>(Address + 0x8);
        public bool IsUsing => SkillUseStage > 2; //GetStat(PlayerStats.CastingSpell) > 0;
        public bool PrepareForUsage => SkillUseStage == 1; //GetStat(PlayerStats.CastingSpell) > 0;
        public TimeSpan CastTime => TimeSpan.FromMilliseconds((int) Math.Ceiling(1000f / (HundredTimesAttacksPerSecond / 100f)));
        public float Dps => GetStat(GameStat.HundredTimesDamagePerSecond + (IsUsing ? 4 : 0)) / 100f;
        public int HundredTimesAttacksPerSecond =>
            GetStat(IsUsing ? GameStat.HundredTimesCastsPerSecond : GameStat.HundredTimesAttacksPerSecond);
        public bool IsTotem => GetStat(GameStat.IsTotem) == 1 || GetStat(GameStat.SkillIsTotemified) == 1;
        public bool IsTrap => GetStat(GameStat.IsTrap) == 1 || GetStat(GameStat.SkillIsTrapped) == 1;
        public bool IsVaalSkill => SoulsPerUse >= 1 && TotalVaalUses >= 1;
        private bool IsMine => true; //TODO
        /*public int UsesAvailable
        {
            get
            {

            if (MaxUses == 0)
            {
                if (IsTotem)
                {
                    return GetStat(PlayerStats.SkillDisplayNumberOfTotemsAllowed);
                }
                return 1;
            }
            return -1;

            if (IsMine)
            {
                return (GetStat(PlayerStats.SkillDisplayNumberOfRemoteMinesAllowed) - NumberDeployed);
            }
            return (MaxUses - NumberDeployed);                         
            }
        }
  

        public int NumberDeployed
        {
            get
            {
                return DeployedObjects.Count;
            }
        }*/

        //Doesn't work after patch or what
        //public List<DeployedObject> DeployedObjects => ObjectManager.Instance.GameController.Player.GetComponent<Actor>().DeployedObjects.Where(x => x.ObjectKey == Id).ToList();

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
                    default:
                        if (Id != 0x37d9)
                            return Id.ToString(CultureInfo.InvariantCulture);
                        else
                            return "WashedUp";
                }
            }
        }

        public Dictionary<GameStat, int> Stats
        {
            get
            {
                var statsPtr = M.Read<long>(Address + 0x78);
                var result = new Dictionary<GameStat, int>();

                ReadStats(result, statsPtr);

                //var additionalStatsPtr = M.Read<long>(Address + 0x60);
                //ReadStats(result, additionalStatsPtr);
                //And one more additional pointer in 0x10 -> 0x78 that I don't want to read
                return result;
            }
        }

        internal void ReadStats(Dictionary<GameStat, int> stats, long address)
        {
            var statPtrStart = M.Read<long>(address + 0xF0);
            var statPtrEnd = M.Read<long>(address + 0xF8);

            var key = 0;
            var value = 0;
            var total_stats = (int) (statPtrEnd - statPtrStart);
            var bytes = M.ReadMem(statPtrStart, total_stats);

            for (var i = 0; i < bytes.Length; i += 8)
            {
                key = BitConverter.ToInt32(bytes, i);
                value = BitConverter.ToInt32(bytes, i + 0x04);
                stats[(GameStat) key] = value;
            }
        }

        public int GetStat(GameStat stat)
        {
            if (!Stats.TryGetValue(stat, out var num)) return 0;
            return num;
        }

        public override string ToString()
        {
            return $"IsUsing: {IsUsing}, {Name}, Id: {Id}, InternalName: {InternalName}, CanBeUsed: {CanBeUsed}";
        }
    }
}
