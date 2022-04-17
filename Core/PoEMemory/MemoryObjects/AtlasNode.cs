using System.Collections.Generic;
using ExileCore.PoEMemory.FilesInMemory.Atlas;
using SharpDX;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class AtlasNode : RemoteMemoryObject
    {
        private WorldArea area;
        private float posX = -1;
        private float posY = -1;
        private string text;
        public WorldArea Area => area = area ?? TheGame.Files.WorldAreas.GetByAddress(M.Read<long>(Address));
        public Vector2 PosL0 => GetPosByLayer(0);
        public Vector2 PosL1 => GetPosByLayer(1);
        public Vector2 PosL2 => GetPosByLayer(2);
        public Vector2 PosL3 => GetPosByLayer(3);
        public Vector2 PosL4 => GetPosByLayer(4);
        public Vector2 DefaultPos => PosL0;
        public float PosX => posX; // != -1 ? posX : posX = M.Read<float>(Address + 0x11D);
        public float PosY => posY; // != -1 ? posY : posY = M.Read<float>(Address + 0x121);
        public byte Flag0 => M.Read<byte>(Address + 0x20);

        public List<int> Tiers => new List<int>
        {
            M.Read<int>(Address + 0xA1),
            M.Read<int>(Address + 0xA5),
            M.Read<int>(Address + 0xA9),
            M.Read<int>(Address + 0xAD),
            M.Read<int>(Address + 0xB1)
        };

        public string FlavourText => text = text ?? M.ReadStringU(M.Read<long>(M.Read<long>(Address + 0x31) + 0x0C));

        public AtlasRegion AtlasRegion => TheGame.Files.AtlasRegions.GetByAddress(M.Read<long>(Address + 0x41));

        public Vector2 GetPosByLayer(int layer)
        {
            const int X_START = 0xB5;
            const int Y_START = 0xC9;

            var x = M.Read<float>(Address + X_START + layer * sizeof(float));
            var y = M.Read<float>(Address + Y_START + layer * sizeof(float));
            return new Vector2(x, y);
        }

        public int GetLayerByTier(int tier)
        {
            return Tiers.IndexOf(tier);
        }

        public int GetTierByLayer(int layer)
        {
            const int TIER_START = 0xA1;

            return M.Read<int>(Address + TIER_START + layer * sizeof(int));
        }

        public bool IsUniqueMap
        {
            get
            {
                var uniqTest = M.ReadStringU(M.Read<long>(Address + 0x10, 0));
                return !string.IsNullOrEmpty(uniqTest) && uniqTest.StartsWith("Uniq");
            }
        }

        public override string ToString()
        {
            return $"{Area.Name}";
        }
    }
}
