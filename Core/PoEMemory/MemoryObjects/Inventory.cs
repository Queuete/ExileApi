using System.Collections.Generic;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using GameOffsets;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class Inventory : Element
    {
        private static int ItemCountOff = Extensions.GetOffset<InventoryOffsets>(nameof(InventoryOffsets.ItemCount));

        private static int TotalBoxesInInventoryRowOff =
            Extensions.GetOffset<InventoryOffsets>(nameof(InventoryOffsets.TotalBoxesInInventoryRow));

        private readonly CachedValue<InventoryOffsets> _cachedValue;
        private InventoryType _cacheInventoryType;

        public Inventory()
        {
            _cachedValue = new FrameCache<InventoryOffsets>(() => M.Read<InventoryOffsets>(Address));
        }

        public long ItemCount => InventoryStruct.ItemCount; // M.Read<long>(Address + ItemCountOff); //This one is correct
        public long TotalBoxesInInventoryRow => InventoryStruct.TotalBoxesInInventoryRow; // M.Read<int>(Address + TotalBoxesInInventoryRowOff);
        private InventoryOffsets InventoryStruct => _cachedValue.Value;
        public NormalInventoryItem HoverItem => InventoryStruct.HoverItem == 0 ? null : GetObject<NormalInventoryItem>(InventoryStruct.HoverItem);
        public int MoveItemHoverState => InventoryStruct.MoveItemHoverState;
        public new int X => InventoryStruct.XReal;
        public new int Y => InventoryStruct.YReal;
        public int XFake => InventoryStruct.XFake;
        public int YFake => InventoryStruct.YFake;
        public bool CursorHoverInventory => InventoryStruct.CursorInInventory == 1;
        public InventoryType InvType => GetInvType();
        public Element InventoryUIElement => getInventoryElement();

        // Shows Item details of visible inventory/stashes
        public IList<NormalInventoryItem> VisibleInventoryItems
        {
            get
            {
                var InvRoot = InventoryUIElement;

                if (InvRoot == null || InvRoot.Address == 0x00)
                    return null;
                /*else if (!InvRoot.IsVisible)
                    return null;*/

                var list = new List<NormalInventoryItem>();

                switch (InvType)
                {
                    case InventoryType.PlayerInventory:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount == 0) continue; //3.3 fix, Can cause problems but filter out first incorrect item
                            var normalItem = item.AsObject<NormalInventoryItem>();
                            list.Add(normalItem);
                        }
                        break;

                    case InventoryType.NormalStash:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount == 0) continue; //3.3 fix, Can cause problems but filter out first incorrect item
                            var normalItem = item.AsObject<NormalInventoryItem>();
                            list.Add(normalItem);
                        }
                        break;

                    case InventoryType.QuadStash:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount == 0) continue; //3.3 fix, Can cause problems but filter out first incorrect item
                            var normalItem = item.AsObject<NormalInventoryItem>();
                            list.Add(normalItem);
                        }
                        break;

                    //For 3.3 child count is 3, not 2 as earlier, so we using the second one
                    case InventoryType.CurrencyStash:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount > 1)
                                list.Add(item[1].AsObject<CurrencyInventoryItem>());
                        }
                        break;

                    case InventoryType.EssenceStash:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount > 1)
                                list.Add(item[1].AsObject<EssenceInventoryItem>());
                        }
                        break;

                    case InventoryType.FragmentStash:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount > 1)
                                list.Add(item[1].AsObject<FragmentInventoryItem>());
                        }
                        break;

                    case InventoryType.DivinationStash:
                        foreach (var item in InvRoot.Children)
                        {
                            // Divination Stash tab isn't loaded.
                            if (item.ChildCount < 2)
                                return null;

                            if (item.Children[1].ChildCount > 1)
                                list.Add(item[1][1].AsObject<DivinationInventoryItem>());
                        }
                        break;

                    case InventoryType.MapStash:
                        foreach (var subInventories in InvRoot.Children[3].Children)
                        {
                            // VisibleInventoryItems would only be found in Visible Sub Inventory :p
                            if (!subInventories.IsVisible)
                                continue;

                            // All empty sub Inventories have full ChildCount (72) but all childcount have 0 items.
                            if (subInventories.ChildCount == 72 &&
                                subInventories.Children[0].AsObject<NormalInventoryItem>().Item.Address == 0x00)
                                continue;

                            foreach (var item in subInventories.Children)
                            {
                                if (item.ChildCount == 0) continue; //3.3 fix
                                list.Add(item.AsObject<NormalInventoryItem>());
                            }
                        }
                        break;

                    case InventoryType.DelveStash:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount > 1)
                                list.Add(item[1].AsObject<DelveInventoryItem>());
                        }
                        break;

                    case InventoryType.BlightStash:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount > 1)
                                list.Add(item[1].AsObject<BlightInventoryItem>());
                        }
                        break;

                    case InventoryType.DeliriumStash:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount > 1)
                                list.Add(item[1].AsObject<DeliriumInventoryItem>());
                        }
                        break;

                    case InventoryType.MetamorphStash:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount > 1)
                                list.Add(item[1].AsObject<MetamorphInventoryItem>());
                        }
                        break;

                    case InventoryType.UniqueStash:
                        foreach (var item in InvRoot.Children)
                        {
                            if (item.ChildCount > 1)
                                list.Add(item[1].AsObject<NormalInventoryItem>());
                        }
                        break;

                }

                return list;
            }
        }

        // Works even if inventory is currently not in view.
        // As long as game have fetched inventory data from Server.
        // Will return the item based on x,y format.
        // Give more controll to user what to do with
        // dublicate items (items taking more than 1 slot)
        // or slots where items doesn't exists (return null).
        public Entity this[int x, int y, int xLength]
        {
            get
            {
                var invAddr = M.Read<long>(Address + 0x410, 0x640, 0x38);
                y = y * xLength;
                var itmAddr = M.Read<long>(invAddr + (x + y) * 8);

                if (itmAddr <= 0)
                    return null;

                return ReadObject<Entity>(itmAddr);
            }
        }

        private InventoryType GetInvType()
        {
            if (_cacheInventoryType != InventoryType.InvalidInventory) return _cacheInventoryType;

            if (Address == 0) return InventoryType.InvalidInventory;

            // For Poe MemoryLeak bug where ChildCount of PlayerInventory keep
            // Increasing on Area/Map Change. Ref:
            // http://www.ownedcore.com/forums/mmo/path-of-exile/poe-bots-programs/511580-poehud-overlay-updated-362.html#post3718876
            // Orriginal Value of ChildCount should be 0x18
            for (var j = 1; j < InventoryList.InventoryCount; j++)
            {
                if (TheGame.IngameState.IngameUi.InventoryPanel[(InventoryIndex) j].Address == Address)
                {
                    _cacheInventoryType = InventoryType.PlayerInventory;
                    return _cacheInventoryType;
                }
            }

            var parentChildCount = AsObject<Element>().Parent.ChildCount;

            switch (parentChildCount)
            {
                case 0x6F:
                    _cacheInventoryType = InventoryType.EssenceStash;
                    break;
                case 0x4B:
                    _cacheInventoryType = InventoryType.CurrencyStash;
                    break;
                case 0x86:
                    _cacheInventoryType = InventoryType.FragmentStash;
                    break;
                case 0x5:
                    _cacheInventoryType = InventoryType.DivinationStash;
                    break;
                case 0x6:
                    if (AsObject<Element>().Parent.Children[0].ChildCount == 9)
                    {
                        _cacheInventoryType = InventoryType.MapStash;
                        break;
                    }
                    _cacheInventoryType = InventoryType.InvalidInventory;
                    break;
                case 0x01:
                    // Normal Stash and Quad Stash is same.
                    if (TotalBoxesInInventoryRow == 24) _cacheInventoryType = InventoryType.QuadStash;
                    _cacheInventoryType = InventoryType.NormalStash;
                    break;
                case 0x23:
                    _cacheInventoryType = InventoryType.DelveStash;
                    break;
                case 0x52:
                    _cacheInventoryType = InventoryType.BlightStash;
                    break;
                case 0x1C:
                    _cacheInventoryType = InventoryType.DeliriumStash;
                    break;
                case 0x11:
                    _cacheInventoryType = InventoryType.MetamorphStash;
                    break;
                case 0x9:
                    _cacheInventoryType = InventoryType.UniqueStash;
                    break;
                default:
                    _cacheInventoryType = InventoryType.InvalidInventory;
                    break;
            }

            return _cacheInventoryType;
        }

        private Element getInventoryElement()
        {
            switch (InvType)
            {
                case InventoryType.PlayerInventory:
                case InventoryType.NormalStash:
                case InventoryType.QuadStash:
                    return AsObject<Element>();
                case InventoryType.CurrencyStash:
                case InventoryType.EssenceStash:
                case InventoryType.FragmentStash:
                case InventoryType.DelveStash:
                    return AsObject<Element>().Parent;
                case InventoryType.DivinationStash:
                    return GetObject<Element>(M.Read<long>(Address + OffsetBuffers + 0x24, 0x08));
                case InventoryType.MapStash:
                    return AsObject<Element>().Parent.AsObject<MapStashTabElement>();
                default:
                    return null;
            }
        }
    }
}
