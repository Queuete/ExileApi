using System;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class BestiaryCapturableMonster : RemoteMemoryObject
    {
        private BestiaryCapturableMonster _BestiaryCapturableMonsterKey;
        private BestiaryGenus _BestiaryGenus;
        private BestiaryGroup _BestiaryGroup;
        private string _MonsterName;
        private MonsterVariety _MonsterVariety;
        public int Id { get; set; }
        public string MonsterName => _MonsterName = _MonsterName ?? M.ReadStringU(M.Read<long>(Address + 0x20));
        public MonsterVariety MonsterVariety =>
            _MonsterVariety = _MonsterVariety ?? TheGame.Files.MonsterVarieties.GetByAddress(M.Read<long>(Address + 0x0));
        public BestiaryGroup BestiaryGroup =>
            _BestiaryGroup = _BestiaryGroup ?? TheGame.Files.BestiaryGroups.GetByAddress(M.Read<long>(Address + 0x10));
        public long BestiaryEncountersPtr => M.Read<long>(Address + 0x30);
        [Obsolete("[3.16] Doesn't seem to be here anymore")]
        public BestiaryCapturableMonster BestiaryCapturableMonsterKey =>
            _BestiaryCapturableMonsterKey = _BestiaryCapturableMonsterKey ?? TheGame.Files.BestiaryCapturableMonsters.GetByAddress(M.Read<long>(Address + 0x6a));
        public BestiaryGenus BestiaryGenus =>
            _BestiaryGenus = _BestiaryGenus ?? TheGame.Files.BestiaryGenuses.GetByAddress(M.Read<long>(Address + 0x59));
        public int AmountCaptured => TheGame.IngameState.Data.ServerData.GetBeastCapturedAmount(this);

        public override string ToString()
        {
            return $"Name: {MonsterName}, Group: {BestiaryGroup.Name}, Family: {BestiaryGroup.Family.Name}, Captured: {AmountCaptured}";
        }
    }
}