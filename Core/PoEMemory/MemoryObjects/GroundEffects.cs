namespace ExileCore.PoEMemory.MemoryObjects
{
    using GameOffsets;

    public class GroundEffects : RemoteMemoryObject
    {
        private GroundEffectsOffsets _data => this.M.Read<GroundEffectsOffsets>(this.Address);

        public GroundEffectTypes GroundEffectType => this.TheGame.Files.GroundEffectTypes.GetByAddress(this._data.GroundEffectTypesKey);
    }
}
