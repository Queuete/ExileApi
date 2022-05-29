namespace ExileCore.PoEMemory.MemoryObjects
{
    using GameOffsets;

    public class ItemVisualIdentity : RemoteMemoryObject
    {
        public string Id
        {
            get;
            private set;
        }

        public string ArtFileName
        {
            get;
            private set;
        }

        /// <inheritdoc />
        protected override void OnAddressChange()
        {
            var data = this.M.Read<ItemVisualIdentityOffsets>(this.Address);

            this.Id = this.M.ReadStringU(data.IdKey);
            this.ArtFileName = this.M.ReadStringU(data.ArtFileNameKey);
        }
    }
}
