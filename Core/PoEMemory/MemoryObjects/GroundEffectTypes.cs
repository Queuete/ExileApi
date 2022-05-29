namespace ExileCore.PoEMemory.MemoryObjects
{
    using GameOffsets;

    public class GroundEffectTypes : RemoteMemoryObject
    {
        private GroundEffectTypesOffsets _data => this.M.Read<GroundEffectTypesOffsets>(this.Address);

        public string Id => this.M.ReadStringU(this._data.Id);

        public string BuffId0 => this.M.ReadStringU(this.M.Read<long>(this._data.BuffDefinitionsKey0));

        public string BuffId1 => this.M.ReadStringU(this.M.Read<long>(this._data.BuffDefinitionsKey1));
    }
}
