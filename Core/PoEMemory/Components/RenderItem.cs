namespace ExileCore.PoEMemory.Components
{
    public class RenderItem : Component
    {
        public string ResourcePath =>
            Cache.StringCache.Read($"{nameof(RenderItem)}{Address + 0x28}", () => M.ReadStringU(M.Read<long>(Address + 0x28)));
    }
}
