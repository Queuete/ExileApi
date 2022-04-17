using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using System;
using System.Collections.Generic;

namespace ExileCore.PoEMemory.Elements
{
    public class InventoryElement : Element
    {
        private InventoryList _allInventories;
        private InventoryList AllInventories => _allInventories = _allInventories ?? GetObjectAt<InventoryList>(0x370);
        public Inventory this[InventoryIndex k] => AllInventories[k];

        public IList<Element> GetItemsInInventory()
        {
            var playerInventory = GetElementSlot(InventoryIndex.PlayerInventory);
            var items = playerInventory.Children;
            items.RemoveAt(0);
            return items;
        }

        public Element GetElementSlot(InventoryIndex inventoryIndex)
        {
          switch (inventoryIndex)
          {
            case InventoryIndex.None:
              throw new ArgumentOutOfRangeException(nameof(inventoryIndex));
            case InventoryIndex.Helm:
              return EquippedItems.GetChildAtIndex(10);
            case InventoryIndex.Amulet:
              return EquippedItems.GetChildAtIndex(11);
            case InventoryIndex.Chest:
              return EquippedItems.GetChildAtIndex(12);
            case InventoryIndex.LWeapon:
              return EquippedItems.GetChildAtIndex(14);
            case InventoryIndex.RWeapon:
              return EquippedItems.GetChildAtIndex(13);
            case InventoryIndex.LWeaponSwap:
              return EquippedItems.GetChildAtIndex(3);
            case InventoryIndex.RWeaponSwap:
              return EquippedItems.GetChildAtIndex(2);
            case InventoryIndex.LRing:
              return EquippedItems.GetChildAtIndex(17);
            case InventoryIndex.RRing:
              return EquippedItems.GetChildAtIndex(18);
            case InventoryIndex.Gloves:
              return EquippedItems.GetChildAtIndex(19);
            case InventoryIndex.Belt:
              return EquippedItems.GetChildAtIndex(20);
            case InventoryIndex.Boots:
              return EquippedItems.GetChildAtIndex(21);
            case InventoryIndex.PlayerInventory:
              return EquippedItems.GetChildAtIndex(22);
            case InventoryIndex.Flask:
              return EquippedItems.GetChildAtIndex(23);
            case InventoryIndex.Trinket:
              return EquippedItems.GetChildAtIndex(24);
            case InventoryIndex.BloodCrucible:
              return EquippedItems.GetChildAtIndex(1);
            default:
              throw new ArgumentOutOfRangeException(nameof(inventoryIndex));
          }
        }
        private Element EquippedItems => GetChildAtIndex(3);
    }
}
