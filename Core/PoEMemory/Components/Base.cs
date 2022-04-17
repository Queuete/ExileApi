using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using GameOffsets.Native;
using GameOffsets;
using ExileCore.Shared.Cache;

namespace ExileCore.PoEMemory.Components
{
    public class Base : Component
    {
        private readonly CachedValue<BaseComponentOffsets> _CachedComponent;
        private readonly CachedValue<BaseInfoOffsets> _CachedInfo;

        private string _Name;

        public Base()
        {
            _CachedComponent = new FrameCache<BaseComponentOffsets>(() => M.Read<BaseComponentOffsets>(Address));
            _CachedInfo =
                new FrameCache<BaseInfoOffsets>(() => M.Read<BaseInfoOffsets>(_CachedComponent.Value.BaseInfoKey));
        }

        private BaseComponentOffsets ComponentStruct => _CachedComponent.Value;
        private BaseInfoOffsets InfoStruct => _CachedInfo.Value;

        public string Name => _Name = _Name ?? M.ReadStringU(InfoStruct.NameKey);
        public byte ItemCellsSizeX => InfoStruct.ItemCellSizeX;
        public byte ItemCellsSizeY => InfoStruct.ItemCellSizeY;
        private Influence InfluenceFlag => (Influence)ComponentStruct.InfluenceFlag;
        public bool isShaper => (InfluenceFlag & Influence.Shaper) == Influence.Shaper;
        public bool isElder => (InfluenceFlag & Influence.Elder) == Influence.Elder;
        public bool isCrusader => (InfluenceFlag & Influence.Crusader) == Influence.Crusader;
        public bool isHunter => (InfluenceFlag & Influence.Hunter) == Influence.Hunter;
        public bool isRedeemer => (InfluenceFlag & Influence.Redeemer) == Influence.Redeemer;
        public bool isWarlord => (InfluenceFlag & Influence.Warlord) == Influence.Warlord;
        public bool isCorrupted => (ComponentStruct.isCorrupted & 0x01) == 0x01;
        public int UnspentAbsorbedCorruption => ComponentStruct.UnspentAbsorbedCorruption;
        public int ScourgedTier => ComponentStruct.ScourgedTier;
        public string PublicPrice => M.ReadStringU(ComponentStruct.PublicPricePtr);
        // public bool isFractured => M.Read<byte>(Address + 0x98) == 0; // TODO: 3.12.2
    }
}
