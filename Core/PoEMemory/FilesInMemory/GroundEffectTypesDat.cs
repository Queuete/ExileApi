﻿namespace ExileCore.PoEMemory.FilesInMemory
{
    using System;
    using ExileCore.PoEMemory.MemoryObjects;
    using ExileCore.Shared.Interfaces;

    public class GroundEffectTypesDat : UniversalFileWrapper<GroundEffectTypes>
    {
        public GroundEffectTypesDat(IMemory m, Func<long> addressFn) : base(m, addressFn)
        {
        }
    }
}
