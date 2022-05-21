using SharpDX;

namespace ExileCore.PoEMemory.Elements.InventoryElements
{
    public class DivinationInventoryItem : NormalInventoryItem
    {
        // Inventory Position in Essence Stash is always invalid.
        // Also, as items are fixed, so Inventory Position doesn't matter.
        public override int InventPosX => 0;
        public override int InventPosY => 0;

        public override RectangleF GetClientRect()
        {
            var unshiftedPosition = this.Parent.Parent.GetClientRect();

            var scrollBarPanel = this.Parent.Parent.Parent.Parent[2];

            var currentTicksY = this.M.Read<int>(scrollBarPanel.Address + 0x29C);

            unshiftedPosition.Y -= 72.56f * currentTicksY; // TODO: Check if needs to be scaled by resolution

            return unshiftedPosition;
        }
    }
}
