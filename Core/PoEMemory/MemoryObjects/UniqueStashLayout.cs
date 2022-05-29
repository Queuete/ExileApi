namespace ExileCore.PoEMemory.MemoryObjects
{
    using GameOffsets;

    public class UniqueStashLayout : RemoteMemoryObject
    {
        public UniqueStashLayout()
        {
            this.UniqueName = string.Empty;
            this.ItemVisualIdentity = new ItemVisualIdentity();
        }

        public string UniqueName
        {
            get;
            private set;
        }

        public ItemVisualIdentity ItemVisualIdentity
        {
            get;
            private set;
        }

        /// <inheritdoc />
        protected override void OnAddressChange()
        {
            var data = this.M.Read<UniqueStashLayoutOffsets>(this.Address);

            this.UniqueName = this.M.ReadStringU(this.M.Read<long>(data.UniqueNameKey + 0x04));
            this.ItemVisualIdentity = this.TheGame.Files.ItemVisualIdentities.GetByAddress(data.VisualIdentityKey);
        }
    }
}
