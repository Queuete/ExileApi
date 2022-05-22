namespace ExileCore.PoEMemory.Elements.InventoryElements
{
    using SharpDX;

    public class EldritchMavenFragmentInventoryItem : NormalInventoryItem
    {
        public override int InventPosX => base.InventPosX % 12;

        public override RectangleF GetClientRect()
        {
            var baseDimensions = base.GetClientRect();
            var parentDimensions = this.Parent.GetClientRectCache;

            return new RectangleF(
                parentDimensions.TopLeft.X + (this.InventPosX * baseDimensions.Width),
                parentDimensions.TopLeft.Y,
                baseDimensions.Width,
                baseDimensions.Height);
        }
    }
}
