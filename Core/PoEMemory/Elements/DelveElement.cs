using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Helpers;
using GameOffsets.Native;
using SharpDX;

namespace ExileCore.PoEMemory.Elements
{
    public class DelveElement : Element
    {
        private readonly CachedValue<IList<DelveBigCell>> _cachedValue;
        private RectangleF rect = RectangleF.Empty;

        public DelveElement()
        {
            _cachedValue = new ConditionalCache<IList<DelveBigCell>>(() => Children.Select(x => x.AsObject<DelveBigCell>()).ToList(), () =>
            {
                if (GetClientRect() == rect) return false;
                rect = GetClientRect();
                return true;
            });
        }

        public IList<DelveBigCell> Cells => _cachedValue.Value;
    }

    public class DelveBigCell : Element
    {
        private readonly CachedValue<IList<DelveCell>> _cachedValue;
        private RectangleF rect = RectangleF.Empty;
        //private string text;
        //private long? type;

        public DelveBigCell()
        {
            _cachedValue = new ConditionalCache<IList<DelveCell>>(() => Children.Select(x => x.AsObject<DelveCell>()).ToList(), () =>
            {
                if (GetClientRect() != rect)
                {
                    rect = GetClientRect();
                    return true;
                }

                return false;
            });
        }

        public IList<DelveCell> Cells => _cachedValue.Value;
        //public long TypePtr => type ?? (type = M.Read<long>(Address + 0x150)).Value;
        //public override string Text => text = text ?? M.ReadStringU(M.Read<long>(TypePtr + 0x0));
    }

    public class DelveCell : Element
    {
        private DelveCellInfoStrings info;
        private NativeStringU mods => M.Read<NativeStringU>(Address + 0x4E0);
        public string Mods => mods.ToString(M);
        //private NativeStringU mines => M.Read<NativeStringU>(M.Read<long>(Address + 0x150) + 0x38);
        //public string MinesText => mines.ToString(M);
        public DelveCellInfoStrings Info => info = info ?? ReadObjectAt<DelveCellInfoStrings>(0x6C0);
        public override string Text => $"{Info.TestString} [{Info.TestString5}]";
        public bool HasEncounter => M.Read<bool>(Address + 0x45B);
        public bool IsCellVisible => M.Read<bool>(Address + 0x45C);
        public bool PathIsHidden => M.Read<bool>(Address + 0x45F);
        public bool WasVisited => M.Read<bool>(Address + 0x460);
        public string IconPath => M.ReadStringU(M.Read<long>(Address + 0x578, 0x0));
        /// <summary> The cell id, e.g. Azurite3_1 </summary>
        public string Type => M.ReadStringU(M.Read<long>(Address + 0x628, 0x0));
        /// <summary> The cell display name, e.g. Azurite Fissure </summary>
        public string TypeHuman => M.ReadStringU(M.Read<long>(Address + 0x628, 0x8));
        public string BiomeName => M.ReadStringU(M.Read<long>(Address + 0x638, 0x0));
        public string BiomeDisplayName => M.ReadStringU(M.Read<long>(Address + 0x638, 0x8));
        public bool IsFeatureHovered => M.Read<bool>(Address + 0x677);
        public bool IsCellHovered => M.Read<bool>(Address + 0x678);
    }

    public class DelveCellInfoStrings : RemoteMemoryObject
    {
        private bool _interesting;
        private string _testString;
        private string _testString2;
        private string _testString3;
        private string _testString4;
        private string _testString5;
        private string _testStringGood;
        public string TestString => _testString = _testString ?? M.ReadStringU(M.Read<long>(Address));
        public string TestStringGood => _testStringGood = _testStringGood ?? _testString.InsertBeforeUpperCase(Environment.NewLine);
        public string TestString2 => _testString2 = _testString2 ?? M.ReadStringU(M.Read<long>(Address + 0x8));
        public string TestString3 => _testString3 = _testString3 ?? M.ReadStringU(M.Read<long>(Address + 0x30));
        public string TestString4 => _testString4 = _testString4 ?? M.ReadStringU(M.Read<long>(Address + 0x60));

        public string TestString5
        {
            get
            {
                var s = _testString5;
                if (s != null) return s;

                _testString5 = M.ReadStringU(M.Read<long>(Address + 0x68));

                return _testString5;
            }
        }

        public bool Interesting
        {
            get
            {
                if (_testString5 != null) return _interesting;
                var testString5 = TestString5;

                _interesting = testString5.Length > 1 && !testString5.EndsWith("Azurite") &&
                    !TestString.StartsWith("Azurite3") && !testString5.EndsWith("Weapons") &&
                    !testString5.EndsWith("Armour") && !testString5.EndsWith("Jewellery") &&
                    !testString5.EndsWith("Items") || TestString.StartsWith("Obstruction");

                return _interesting;
            }
        }
    }
}
