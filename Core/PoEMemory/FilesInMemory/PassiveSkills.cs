using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore.Shared.Interfaces;

namespace ExileCore.PoEMemory.FilesInMemory
{
    public class PassiveSkills : UniversalFileWrapper<PassiveSkill>
    {
        private List<PassiveSkill> _EntriesList;
        private bool _Loaded;

        public PassiveSkills(IMemory m, Func<long> address) : base(m, address)
        {
        }

        public Dictionary<int, PassiveSkill> PassiveSkillsDictionary { get; } = new Dictionary<int, PassiveSkill>();
        public new IList<PassiveSkill> EntriesList => _EntriesList = _EntriesList ?? base.EntriesList.ToList();

        public PassiveSkill GetPassiveSkillByPassiveId(int index)
        {
            CheckCache();

            if (!_Loaded)
            {
                foreach (var passiveSkill in EntriesList)
                {
                    EntryAdded(passiveSkill.Address, passiveSkill);
                }

                _Loaded = true;
            }

            PassiveSkillsDictionary.TryGetValue(index, out var result);
            return result;
        }

        public PassiveSkill GetPassiveSkillById(string id)
        {
            return EntriesList.FirstOrDefault(x => x.Id == id);
        }

        protected new void EntryAdded(long addr, PassiveSkill entry)
        {
            PassiveSkillsDictionary.Add(entry.PassiveId, entry);
        }

        public new PassiveSkill GetByAddress(long address)
        {
            return base.GetByAddress(address);
        }
    }
}