namespace ExileCore.PoEMemory.FilesInMemory
{
    using System;
    using ExileCore.PoEMemory.MemoryObjects;
    using ExileCore.Shared.Interfaces;

    public class GroundEffectsDat : UniversalFileWrapper<GroundEffects>
    {
        public GroundEffectsDat(IMemory m, Func<long> addressFn) : base(m, addressFn)
        {
        }
    }
}
