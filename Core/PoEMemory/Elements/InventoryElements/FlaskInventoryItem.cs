namespace ExileCore.PoEMemory.Elements.InventoryElements
{
    using SharpDX;

    public class FlaskInventoryItem : NormalInventoryItem
    {
        public override int InventPosX => 0;

        public override int InventPosY => 0;

        public override RectangleF GetClientRect()
        {
            return this.Parent.GetClientRect();
        }
    }
}
