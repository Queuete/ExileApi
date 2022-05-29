namespace ExileCore.PoEMemory.FilesInMemory
{
    using System;
    using System.Collections.Generic;
    using ExileCore.PoEMemory.MemoryObjects;
    using ExileCore.Shared.Interfaces;

    public class ItemVisualIdentitiesDat : UniversalFileWrapper<ItemVisualIdentity>
    {
        private readonly Dictionary<string, List<ItemVisualIdentity>> ByArtPath = new Dictionary<string, List<ItemVisualIdentity>>();

        public ItemVisualIdentitiesDat(IMemory m, Func<long> address) : base(m, address)
        {
        }

        public List<ItemVisualIdentity> GetByArtPath(string artPath)
        {
            this.CheckCache();

            return this.ByArtPath.TryGetValue(artPath, out var result) ? result : new List<ItemVisualIdentity>();
        }

        protected override void EntryAdded(long addr, ItemVisualIdentity entry)
        {
            if (!this.ByArtPath.TryGetValue(entry.ArtFileName, out var identityGroup))
            {
                identityGroup = new List<ItemVisualIdentity>();
                this.ByArtPath[entry.ArtFileName] = identityGroup;
            }

            identityGroup.Add(entry);
        }
    }
}
