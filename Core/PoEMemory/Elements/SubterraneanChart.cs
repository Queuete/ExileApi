namespace ExileCore.PoEMemory.Elements
{
    public class SubterraneanChart : Element
    {
        private DelveElement _Grid;
        public DelveElement GridElement =>
            Address != 0 ? (_Grid = _Grid ?? GetObject<DelveElement>(M.Read<long>(Address + 0x1C8, 0x698))) : null;
    }
}