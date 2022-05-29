namespace GameOffsets
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ItemVisualIdentityOffsets
    {
        [FieldOffset(0x00)] public long IdKey;
        [FieldOffset(0x08)] public long ArtFileNameKey;

        // 3.18.0c Layout
        // [FieldOffset(0x00)] public long IdKey;
        // [FieldOffset(0x08)] public long DDSFileNameKey;
        // [FieldOffset(0x10)] public long AOFileNameKey;
        // [FieldOffset(0x18)] public StdFile SoundEffect;
        // [FieldOffset(0x28)] public int Unknown0;
        // [FieldOffset(0x2C)] public long AOFileName2Key;
        // [FieldOffset(0x34)] public StdArray MarauderSMFiles;
        // [FieldOffset(0x44)] public StdArray RangerSMFiles;
        // [FieldOffset(0x54)] public StdArray WitchSMFiles;
        // [FieldOffset(0x64)] public StdArray DuelistSMFiles;
        // [FieldOffset(0x74)] public StdArray TemplarSMFiles;
        // [FieldOffset(0x84)] public StdArray ShadowSMFiles;
        // [FieldOffset(0x94)] public StdArray ScionSMFiles;
        // [FieldOffset(0xA4)] public long MarauderShapeNameKey;
        // [FieldOffset(0xAC)] public long RangerShapeNameKey;
        // [FieldOffset(0xB4)] public long WitchShapeNameKey;
        // [FieldOffset(0xBC)] public long DuelistShapeNameKey;
        // [FieldOffset(0xC4)] public long TemplarShapeNameKey;
        // [FieldOffset(0xCC)] public long ShadowShapeNameKey;
        // [FieldOffset(0xD4)] public long ScionShapeNameKey;
        // [FieldOffset(0xDC)] public int Unknown1;
        // [FieldOffset(0xE0)] public int MtxGenerationId;
        // [FieldOffset(0xE4)] public StdArray PickupAchievements;
        // [FieldOffset(0xF4)] public StdArray SMFiles;
        // [FieldOffset(0x104)] public StdArray IdentifyAchievements;
        // [FieldOffset(0x114)] public long EPKFileNameKey;
        // [FieldOffset(0x11C)] public StdArray CorruptAchievements;
        // [FieldOffset(0x12C)] public bool IsAlternateArt;
        // [FieldOffset(0x12D)] public bool Flag3;
        // [FieldOffset(0x12E)] public StdArray CraftAchievements;
        // [FieldOffset(0x13E)] public long AnimationLocationKey;
        // [FieldOffset(0x146)] public long ClawAOFileNameKey;
        // [FieldOffset(0x14E)] public long DaggerAOFileNameKey;
        // [FieldOffset(0x156)] public long WandAOFileNameKey;
        // [FieldOffset(0x15E)] public long Sword1HAOFileNameKey;
        // [FieldOffset(0x166)] public long ThrustingAOFileNameKey;
        // [FieldOffset(0x16E)] public long Sword2HAOFileNameKey;
        // [FieldOffset(0x176)] public long Mace2HAOFileNameKey;
        // [FieldOffset(0x17E)] public long StaffAOFileNameKey;
        // [FieldOffset(0x186)] public long BowAOFileNameKey;
        // [FieldOffset(0x18E)] public long Axe1HAOFileNameKey;
        // [FieldOffset(0x196)] public long SceptreAOFileNameKey;
        // [FieldOffset(0x19E)] public long Axe2HAOFileNameKey;
        // [FieldOffset(0x1A6)] public bool IsAtlasOfWorldsMapIcon;
        // [FieldOffset(0x1A7)] public bool IsSpecialTier16MapIcon;
        // [FieldOffset(0x1A8)] public StdArray Unknown16;
        // [FieldOffset(0x1B8)] public bool Unknown17;
        // [FieldOffset(0x1B9)] public StdArray EquipAchievements;
        // [FieldOffset(0x1C9)] public long AOFileName3Key;
        // [FieldOffset(0x1D1)] public int TypeIndex;
        // [FieldOffset(0x1D5)] public StdFile ScalingStat1;
        // [FieldOffset(0x1E5)] public StdFile ScalingStat2;
        // [FieldOffset(0x1F5)] public StdFile ScalingStat3;
        // [FieldOffset(0x205)] public StdFile ScalingStat4;
    }
}
