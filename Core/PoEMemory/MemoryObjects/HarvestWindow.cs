namespace ExileCore.PoEMemory.MemoryObjects
{
    using System.Collections.Generic;
    using ExileCore.PoEMemory.Elements;
    using ExileCore.PoEMemory.Elements.InventoryElements;

    public class HarvestWindow : Element
    {
        public List<HarvestCraft> Crafts => this.TryGetChildFromIndices(out var craftsListPanel, 8, 0, 1)
            ? craftsListPanel.GetChildrenAs<HarvestCraft>() : new List<HarvestCraft>();

        public Element CraftButton => this.TryGetChildFromIndices(out var panel, 11, 1) ? panel : null;

        public Entity Item => this.StashInventoryItemSlot?.Item ?? this.PlayerInventoryItemSlot?.Item;

        public NormalInventoryItem PlayerInventoryItemSlot => this.TryGetChildFromIndices(out var itemSlotPanel, 11, 0, 0, 0, 1)
            ? itemSlotPanel.AsObject<NormalInventoryItem>() : null;

        public NormalInventoryItem StashInventoryItemSlot => this.TryGetChildFromIndices(out var itemSlotPanel, 11, 0, 1, 0, 1)
            ? itemSlotPanel.AsObject<NormalInventoryItem>() : null;
    }
}
