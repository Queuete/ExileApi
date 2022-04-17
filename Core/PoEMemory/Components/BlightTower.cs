using System.IO;

namespace ExileCore.PoEMemory.Components
{
    public class BlightTower : Component
    {
        private string _id;
        private string _name;
        private string _icon;
        private string _iconFileName;
        private long DatInfo => M.Read<long>(Address + 0x28);
        public string Id => _id = _id ?? M.ReadStringU(M.Read<long>(DatInfo));
        public string Name => _name = _name ?? M.ReadStringU(M.Read<long>(DatInfo + 0x8));
        public string Icon => _icon = _icon ?? M.ReadStringU(M.Read<long>(DatInfo + 0x18));
        public string IconFileName => _iconFileName = _iconFileName ?? Path.GetFileNameWithoutExtension(Icon);
    }
}