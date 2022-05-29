namespace GameOffsets
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GroundEffectComponentOffsets
    {
        [FieldOffset(0x28)] public long GroundEffectsKey;
        [FieldOffset(0x30)] public long GroundEffectsKeyFile;
        [FieldOffset(0x38)] public float TimeRemaining;
    }
}

