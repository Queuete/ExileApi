namespace ExileCore.PoEMemory.Components
{
    using ExileCore.PoEMemory.MemoryObjects;
    using ExileCore.Shared.Cache;
    using GameOffsets;

    public class HeistRewardDisplay : Component
    {
        private readonly CachedValue<HeistRewardDisplayComponentOffsets> CachedData;

        public HeistRewardDisplay()
        {
            this.CachedData
                = new FrameCache<HeistRewardDisplayComponentOffsets>(() => this.M.Read<HeistRewardDisplayComponentOffsets>(this.Address));
        }

        public Entity RewardItem => this.GetObject<Entity>(this._data.RewardItemKey);

        private HeistRewardDisplayComponentOffsets _data => this.CachedData.Value;
    }
}
