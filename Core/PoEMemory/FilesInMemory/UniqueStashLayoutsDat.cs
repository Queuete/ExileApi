namespace ExileCore.PoEMemory.FilesInMemory
{
    using System;
    using System.Collections.Generic;
    using ExileCore.PoEMemory.MemoryObjects;
    using ExileCore.Shared.Interfaces;

    public class UniqueStashLayoutsDat : UniversalFileWrapper<UniqueStashLayout>
    {
        private readonly Dictionary<long, List<UniqueStashLayout>> ByVisualIdentityAddress = new Dictionary<long, List<UniqueStashLayout>>();

        public UniqueStashLayoutsDat(IMemory m, Func<long> address) : base(m, address)
        {
        }

        public List<UniqueStashLayout> GetByVisualIdentity(ItemVisualIdentity visualIdentity)
        {
            this.CheckCache();
            this.ByVisualIdentityAddress.TryGetValue(visualIdentity.Address, out var result);
            return result;
        }

        protected override void EntryAdded(long address, UniqueStashLayout entry)
        {
            if (!this.ByVisualIdentityAddress.TryGetValue(entry.ItemVisualIdentity.Address, out var identityGroup))
            {
                identityGroup = new List<UniqueStashLayout>();
                this.ByVisualIdentityAddress[entry.ItemVisualIdentity.Address] = identityGroup;
            }
            identityGroup.Add(entry);
        }
    }
}
