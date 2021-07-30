using ExileCore.PoEMemory.MemoryObjects;

namespace ExileCore.PoEMemory.Components
{
    public class Animated : Component
    {
        public Entity BaseAnimatedObjectEntity => GetObject<Entity>(M.Read<long>(Address + 0x1C0));
    }
}
