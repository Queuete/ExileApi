using System;
using System.Net.Sockets;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;

namespace ExileCore.PoEMemory.Models
{
    public sealed class ItemStats
    {
        private static StatTranslator translate;
        private readonly Entity item;
        private readonly float[] stats;

        public ItemStats(Entity item)
        {
            this.item = item;
            translate = translate ?? new StatTranslator();
            stats = new float[Enum.GetValues(typeof(ItemStatEnum)).Length];
            if (item.HasComponent<Sockets>()) ParseSockets();
            if (item.HasComponent<Mods>()) ParseExplicitMods();
            if (item.HasComponent<Weapon>()) ParseWeaponStats();
        }

        private void ParseWeaponStats()
        {
            var component = item.GetComponent<Weapon>();
            var num = (component.DamageMin + component.DamageMax) / 2f + GetStat(ItemStatEnum.LocalPhysicalDamage);
            num *= 1f + (GetStat(ItemStatEnum.LocalPhysicalDamagePercent) + item.GetComponent<Quality>().ItemQuality) / 100f;
            AddToMod(ItemStatEnum.AveragePhysicalDamage, num);
            var num2 = 1f / (component.AttackTime / 1000f);
            num2 *= 1f + GetStat(ItemStatEnum.LocalAttackSpeed) / 100f;
            AddToMod(ItemStatEnum.AttackPerSecond, num2);
            var num3 = component.CritChance / 100f;
            num3 *= 1f + GetStat(ItemStatEnum.LocalCritChance) / 100f;
            AddToMod(ItemStatEnum.WeaponCritChance, num3);

            var num4 = GetStat(ItemStatEnum.LocalAddedColdDamage) + GetStat(ItemStatEnum.LocalAddedFireDamage) +
                       GetStat(ItemStatEnum.LocalAddedLightningDamage);

            AddToMod(ItemStatEnum.AverageElementalDamage, num4);
            AddToMod(ItemStatEnum.DPS, (num + num4) * num2);
            AddToMod(ItemStatEnum.PhysicalDPS, num * num2);
        }

        private void ParseExplicitMods()
        {
            foreach (var current in item.GetComponent<Mods>().ItemMods)
            {
                translate.Translate(this, current);
            }

            AddToMod(ItemStatEnum.ElementalResistance,
                GetStat(ItemStatEnum.LightningResistance) + GetStat(ItemStatEnum.FireResistance) +
                GetStat(ItemStatEnum.ColdResistance));

            AddToMod(ItemStatEnum.TotalResistance, GetStat(ItemStatEnum.ElementalResistance) + GetStat(ItemStatEnum.TotalResistance));
        }

        private void ParseSockets()
        {
            //todo ParseSockets do nothing
        }

        public void AddToMod(ItemStatEnum stat, float value)
        {
            stats[(int) stat] += value;
        }

        public float GetStat(ItemStatEnum stat)
        {
            return stats[(int) stat];
        }

        [Obsolete]
        public ItemType GetSlot()
        {
            return ItemType.All;
        }
    }
}
