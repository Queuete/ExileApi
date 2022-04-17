namespace ExileCore.PoEMemory.MemoryObjects
{
    public class BestiaryGroup : RemoteMemoryObject
    {
        private BestiaryFamily family;
        private string groupId;
        private string name;
        public int Id { get; internal set; }
        public string GroupId => groupId = groupId ?? M.ReadStringU(M.Read<long>(Address));
        public string Description => M.ReadStringU(M.Read<long>(Address + 0x8));
        public string Illustration => M.ReadStringU(M.Read<long>(Address + 0x10));
        public string Name => name = name ?? M.ReadStringU(M.Read<long>(Address + 0x18));
        public string SmallIcon => M.ReadStringU(M.Read<long>(Address + 0x20));
        public string ItemIcon => M.ReadStringU(M.Read<long>(Address + 0x28));
        public BestiaryFamily Family => family = family ?? TheGame.Files.BestiaryFamilies.GetByAddress(M.Read<long>(Address + 0x30));

        public override string ToString()
        {
            return Name;
        }
    }
}
