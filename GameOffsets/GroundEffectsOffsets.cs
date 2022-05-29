namespace GameOffsets
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GroundEffectsOffsets
    {
        [FieldOffset(0x00)] public long GroundEffectTypesKey;

        // 3.18.0d Layout
        // [FieldOffset(0x00)] public StdFile GroundEffectTypes;
        // [FieldOffset(0x10)] public int Unknown0;
        // [FieldOffset(0x14)] public int Unknown1;
        // [FieldOffset(0x18)] public StdFile BuffVisualKey0;
        // [FieldOffset(0x28)] public bool Flag0;
        // [FieldOffset(0x29)] public bool Flag1;
        // [FieldOffset(0x2A)] public short Unknown2;
        // [FieldOffset(0x2C)] public StdArray Data0;
        // [FieldOffset(0x3C)] public bool Flag1;
        // [FieldOffset(0x3D)] public StdArray AOFiles;
        // [FieldOffset(0x4D)] public StdArray Scripts;
        // [FieldOffset(0x5D)] public long OnComplete_Script;
        // [FieldOffset(0x65)] public StdFile BuffDefinitionsKey;
        // [FieldOffset(0x75)] public StdFile MiscObjectKey0;
        // [FieldOffset(0x85)] public StdFile MiscObjectKey1;
        // [FieldOffset(0x95)] public StdFile Unknown3;
        // [FieldOffset(0xA5)] public StdFile Unknown4;
        // [FieldOffset(0xB5)] public bool Flag2;
        // [FieldOffset(0xB6)] public bool Flag3;
        // [FieldOffset(0xB7)] public bool Flag4;
        // [FieldOffset(0xB8)] public StdFile BuffVisualKey1;
        // [FieldOffset(0xC8)] public StdFile Unknown5;
        // [FieldOffset(0xD8)] public bool Flag5;
        // [FieldOffset(0xD9)] public bool Flag6;
    }
}

