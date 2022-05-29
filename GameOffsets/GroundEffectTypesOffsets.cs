namespace GameOffsets
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GroundEffectTypesOffsets
    {
        [FieldOffset(0x00)] public long Id;
        [FieldOffset(0x20)] public long BuffDefinitionsKey0;
        [FieldOffset(0x30)] public long BuffDefinitionsKey1;

        // 3.18.0d Layout
        // [FieldOffset(0x00)] public long Id;
        // [FieldOffset(0x08)] public int Unknown0;
        // [FieldOffset(0x0C)] public int Unknown1;
        // [FieldOffset(0x10)] public StdFile ArtVariation_StatsKey;
        // [FieldOffset(0x20)] public StdFile BuffDefinitionsKey0;
        // [FieldOffset(0x30)] public StdFile BuffDefinitionsKey1;
    }
}
