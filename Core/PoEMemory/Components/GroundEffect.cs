namespace ExileCore.PoEMemory.Components
{
    using ExileCore.PoEMemory.MemoryObjects;
    using GameOffsets;

    public class GroundEffect : Component
    {
        public GroundEffects Effect
        {
            get;
            set;
        }

        public float TimeRemaining
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void OnAddressChange()
        {
            if (this.Address != 0)
            {
                var data = this.M.Read<GroundEffectComponentOffsets>(this.Address);
                this.Effect = this.TheGame.Files.GroundEffects.GetByAddress(data.GroundEffectsKey);
                this.TimeRemaining = data.TimeRemaining;
            }
        }
    }
}
