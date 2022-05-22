namespace ExileCore.PoEMemory.MemoryObjects
{
    using System.Collections.Generic;
    using System.Linq;
    using ExileCore.PoEMemory.Elements.InventoryElements;
    using ExileCore.Shared.Cache;
    using ExileCore.Shared.Enums;
    using GameOffsets;

    public class Inventory : Element
    {
        private readonly CachedValue<InventoryOffsets> CachedData;
        private InventoryType _cacheInventoryType;

        public Inventory()
        {
            this.CachedData = new FrameCache<InventoryOffsets>(() => this.M.Read<InventoryOffsets>(this.Address));
        }

        public bool CursorHoverInventory => this._data.CursorInInventory == 1;

        public NormalInventoryItem HoverItem => this._data.HoverItem == 0 ? null
            : this.GetObject<NormalInventoryItem>(this._data.HoverItem);

        public Element InventoryUIElement => this.GetInventoryRootElement();

        public InventoryType InvType => this.GetInventoryType();

        public long ItemCount => this._data.ItemCount;

        public int MoveItemHoverState => this._data.MoveItemHoverState;

        public long TotalBoxesInInventoryRow => this._data.TotalBoxesInInventoryRow;

        /// <summary>
        ///     Shows Item details of locally visible inventories
        /// </summary>
        /// <returns>
        ///     A list of locally visible items.
        /// </returns>
        /// <remarks>
        ///     The data found here is what is populated into UI panels on server request by the game. Note that these items are "locally visible", as
        ///     in they still exist when you switch/close panels, until you change zones or request new data that wipes the old data. Previous
        ///     implementations simply dumped every item it could find to the user. For the basic inventories that have fixed sizes, this isn't an
        ///     issue. More recent tabs (Divination, Flask, Gems, Unique) use a flexible layout where items appear based on the filters provided by the
        ///     tab and will positionally move relative to other items. These inventory tabs are susceptible to overflowing the parent container and
        ///     not actually being visible. It's the users responsibility to check if the item is actually visible (e.g. intersects or contained within
        ///     some ancestral parent).
        /// </remarks>
        public IList<NormalInventoryItem> VisibleInventoryItems
        {
            get
            {
                var inventoryRoot = this.InventoryUIElement;

                if (inventoryRoot == null || inventoryRoot.Address == 0x00)
                {
                    return null;
                }

                var list = new List<NormalInventoryItem>();

                switch (this.InvType)
                {
                    case InventoryType.PlayerInventory:
                    case InventoryType.NormalStash:
                    case InventoryType.QuadStash:
                        {
                            var tabAgnosticItemSlots = inventoryRoot.Children.Where(slot => slot.ChildCount != 0);
                            list.AddRange(tabAgnosticItemSlots.Select(slot => slot.AsObject<NormalInventoryItem>()));
                            return list;
                        }

                    case InventoryType.BlightStash:
                        {
                            var tabAgnosticItemSlots1 = inventoryRoot.Children.Skip(5).Take(14).Where(slot => slot.ChildCount > 1);
                            var tabAgnosticItemSlots2 = inventoryRoot.Children.Skip(80).Take(2).Where(slot => slot.ChildCount > 1);

                            list.AddRange(
                                tabAgnosticItemSlots1.Concat(tabAgnosticItemSlots2)
                                    .Select(slot => slot[1].AsObject<BlightInventoryItem>()));

                            // The Blight Stash layout has one element that can be toggled or triggered to overlay over the blight map inventory slots.
                            if (inventoryRoot[82].IsVisibleLocal)
                            {
                                return list;
                            }

                            var tabSpecificItemSlots = inventoryRoot.Children.Skip(19).Take(60).Where(slot => slot.ChildCount > 1);
                            list.AddRange(tabSpecificItemSlots.Select(slot => slot[1].AsObject<BlightInventoryItem>()));
                            return list;
                        }

                    case InventoryType.CurrencyStash:
                        {
                            var tabAgnosticItemSlots = inventoryRoot.Children.Skip(3).Where(slot => slot.ChildCount > 1);

                            list.AddRange(tabAgnosticItemSlots.Select(slot => slot[1].AsObject<CurrencyInventoryItem>()));

                            if (inventoryRoot[1].IsVisibleLocal)
                            {
                                var generalItemSlots = inventoryRoot[1].Children.Where(slot => slot.ChildCount > 1);
                                list.AddRange(generalItemSlots.Select(slot => slot[1].AsObject<CurrencyInventoryItem>()));
                            }
                            else if (inventoryRoot[2].IsVisibleLocal)
                            {
                                var exoticItemSlots = inventoryRoot[2].Children.Where(slot => slot.ChildCount > 1);
                                list.AddRange(exoticItemSlots.Select(slot => slot[1].AsObject<CurrencyInventoryItem>()));
                            }

                            return list;
                        }

                    case InventoryType.DelveStash:
                        {
                            var tabAgnosticItemSlots = inventoryRoot.Children.Where(slot => slot.ChildCount > 1);
                            list.AddRange(tabAgnosticItemSlots.Select(slot => slot[1].AsObject<DelveInventoryItem>()));
                            return list;
                        }

                    case InventoryType.DivinationStash:
                        {
                            var divinationTabSlots = inventoryRoot[0][1]
                                ?.Children.Where(slot => slot.IsVisibleLocal && slot.ChildCount > 1 && slot[1].ChildCount > 1);

                            if (divinationTabSlots == null)
                            {
                                return list;
                            }

                            list.AddRange(divinationTabSlots.Select(slot => slot[1][1].AsObject<DivinationInventoryItem>()));
                            return list;
                        }

                    case InventoryType.EssenceStash:
                        {
                            var tabAgnosticItemSlots = inventoryRoot.Children.Where(slot => slot.ChildCount > 1);
                            list.AddRange(tabAgnosticItemSlots.Select(slot => slot[1].AsObject<EssenceInventoryItem>()));
                            return list;
                        }

                    case InventoryType.FlaskStash:
                        {
                            var subInventoriesCache = inventoryRoot.GetChildFromIndices(1, 0, 1);

                            if (subInventoriesCache == null)
                            {
                                return list;
                            }

                            var visibleSubInventories = subInventoriesCache.Children.Where(
                                subInventory => subInventory.IsVisibleLocal && subInventory.ChildCount > 1);

                            var visibleItemSlots = visibleSubInventories.SelectMany(x => x[1].Children).Where(slot => slot.ChildCount > 1);

                            list.AddRange(visibleItemSlots.Select(slot => slot[1].AsObject<FlaskInventoryItem>()));
                            return list;
                        }

                    case InventoryType.GemStash:
                        {
                            var subInventoriesCache = inventoryRoot.GetChildFromIndices(1, 0, 1);

                            if (subInventoriesCache == null)
                            {
                                return list;
                            }

                            var visibleSubInventories = subInventoriesCache.Children.Where(
                                subInventory => subInventory.IsVisibleLocal && subInventory.ChildCount > 1);

                            var visibleItemSlots = visibleSubInventories.SelectMany(x => x[1].Children).Where(slot => slot.ChildCount > 1);

                            list.AddRange(visibleItemSlots.Select(slot => slot[1].AsObject<GemInventoryItem>()));
                            return list;
                        }

                    case InventoryType.FragmentStash:
                        {
                            if (inventoryRoot[0].IsVisibleLocal)
                            {
                                var generalTabSlots = inventoryRoot[0].Children.Where(slot => slot.ChildCount > 1);
                                list.AddRange(generalTabSlots.Select(slot => slot[1].AsObject<FragmentInventoryItem>()));
                                return list;
                            }

                            if (inventoryRoot[1].IsVisibleLocal)
                            {
                                var breachTabSlots = inventoryRoot[1].Children.Where(slot => slot.ChildCount > 1);
                                list.AddRange(breachTabSlots.Select(slot => slot[1].AsObject<FragmentInventoryItem>()));
                                return list;
                            }

                            if (inventoryRoot[2].IsVisibleLocal)
                            {
                                var scarabTabSlots = inventoryRoot[2].Children.Where(slot => slot.ChildCount > 1);
                                list.AddRange(scarabTabSlots.Select(slot => slot[1].AsObject<FragmentInventoryItem>()));
                                return list;
                            }

                            if (inventoryRoot[3].IsVisibleLocal)
                            {
                                if (inventoryRoot[3].ChildCount < 2)
                                {
                                    return list;
                                }

                                var eldritchMavenTab = inventoryRoot[3][0].IsVisibleLocal ? inventoryRoot[3][0] :
                                    inventoryRoot[3][1].IsVisibleLocal ? inventoryRoot[3][1] : null;

                                if (eldritchMavenTab == null || eldritchMavenTab.ChildCount == 0)
                                {
                                    return list;
                                }

                                var eldritchMavenTabSlots = eldritchMavenTab[0]
                                    .Children.SelectMany(row => row.Children)
                                    .Where(slot => slot.ChildCount > 0);

                                list.AddRange(eldritchMavenTabSlots.Select(slot => slot.AsObject<EldritchMavenFragmentInventoryItem>()));
                            }

                            return list;
                        }

                    case InventoryType.MapStash:
                        {
                            var mapTabSlots = inventoryRoot[3]
                                .Children.FirstOrDefault(subInventory => subInventory.IsVisibleLocal)
                                ?.Children.Where(slot => slot.ChildCount > 0);

                            if (mapTabSlots == null)
                            {
                                return list;
                            }

                            list.AddRange(mapTabSlots.Select(slot => slot.AsObject<NormalInventoryItem>()));
                            return list;
                        }

                    case InventoryType.MetamorphStash:
                        {
                            var tabAgnosticSlots1 = inventoryRoot.Children.Skip(1).Take(8).Where(slot => slot.ChildCount > 1);
                            var tabAgnosticSlots2 = inventoryRoot.Children.Skip(14).Take(3).Where(slot => slot.ChildCount > 1);

                            list.AddRange(
                                tabAgnosticSlots1.Concat(tabAgnosticSlots2).Select(slot => slot[1].AsObject<MetamorphInventoryItem>()));

                            var visibleSubInventory = inventoryRoot.Children.Skip(9)
                                .Take(5)
                                .FirstOrDefault(subInventory => subInventory.IsVisibleLocal);

                            if (visibleSubInventory == null || visibleSubInventory.ChildCount <= 0)
                            {
                                return list;
                            }

                            var tabSpecificSlots = visibleSubInventory[0].Children.Where(slot => slot.ChildCount > 1);

                            list.AddRange(tabSpecificSlots.Select(slot => slot[1].AsObject<MetamorphInventoryItem>()));
                            return list;
                        }

                    case InventoryType.DeliriumStash:
                        {
                            var tabAgnosticItemSlots = inventoryRoot.Children.Where(slot => slot.ChildCount > 1);
                            list.AddRange(tabAgnosticItemSlots.Select(slot => slot[1].AsObject<DeliriumInventoryItem>()));
                            return list;
                        }

                    case InventoryType.UniqueStash:
                        {
                            var visibleSubInventory
                                = inventoryRoot[1].Children.FirstOrDefault(subInventory => subInventory.IsVisibleLocal)?[0]?[1];

                            if (visibleSubInventory == null)
                            {
                                return list;
                            }

                            var uniqueItemSlots = visibleSubInventory.Children.Where(
                                slot => slot.IsVisibleLocal && slot.ChildCount == 4 && slot[3].ChildCount > 1);
                            list.AddRange(uniqueItemSlots.Select(slot => slot[3][1].AsObject<NormalInventoryItem>()));
                            return list;
                        }

                    case InventoryType.InvalidInventory:
                    default:
                        return list;
                }
            }
        }

        public new int X => this._data.XReal;

        public int XFake => this._data.XFake;

        public new int Y => this._data.YReal;

        public int YFake => this._data.YFake;

        private InventoryOffsets _data => this.CachedData.Value;

        // Works even if inventory is currently not in view.
        // As long as game have fetched inventory data from Server.
        // Will return the item based on x,y format.
        // Give more control to user what to do with
        // duplicate items (items taking more than 1 slot)
        // or slots where items doesn't exists (return null).
        public Entity this[int x, int y, int xLength]
        {
            get
            {
                var invAddr = this.M.Read<long>(this.Address + 0x410, 0x640, 0x38);
                y *= xLength;
                var itmAddr = this.M.Read<long>(invAddr + ((x + y) * 8));

                if (itmAddr <= 0)
                {
                    return null;
                }

                return this.ReadObject<Entity>(itmAddr);
            }
        }

        private InventoryType GetInventoryType()
        {
            if (this._cacheInventoryType != InventoryType.InvalidInventory)
            {
                return this._cacheInventoryType;
            }

            if (this.Address == 0)
            {
                return InventoryType.InvalidInventory;
            }

            // For Poe MemoryLeak bug where ChildCount of PlayerInventory keep
            // Increasing on Area/Map Change. Ref:
            // http://www.ownedcore.com/forums/mmo/path-of-exile/poe-bots-programs/511580-poehud-overlay-updated-362.html#post3718876
            // Original Value of ChildCount should be 0x18
            for (var j = 1; j < InventoryList.InventoryCount; j++)
            {
                if (this.TheGame.IngameState.IngameUi.InventoryPanel[(InventoryIndex)j].Address == this.Address)
                {
                    this._cacheInventoryType = InventoryType.PlayerInventory;
                    return this._cacheInventoryType;
                }
            }

            switch (this.Parent.ChildCount)
            {
                case 1:
                    return this._cacheInventoryType = this.TotalBoxesInInventoryRow == 24 ? InventoryType.QuadStash
                        : InventoryType.NormalStash;
                case 4:
                    return this._cacheInventoryType = this.Parent[0].ChildCount == 4 ? InventoryType.GemStash :
                        this.Parent[0].ChildCount == 5 ? InventoryType.FlaskStash : InventoryType.InvalidInventory;
                case 5:
                    return this._cacheInventoryType = this.Parent[0].ChildCount == 3 ? InventoryType.DivinationStash :
                        this.Parent[0].ChildCount == 49 ? InventoryType.FragmentStash : InventoryType.InvalidInventory;
                case 7:
                    return this._cacheInventoryType = InventoryType.MapStash;
                case 9:
                    return this._cacheInventoryType = InventoryType.UniqueStash;
                case 17:
                    return this._cacheInventoryType = InventoryType.MetamorphStash;
                case 18:
                    return this._cacheInventoryType = InventoryType.CurrencyStash;
                case 35:
                    return this._cacheInventoryType = InventoryType.DelveStash;
                case 83:
                    return this._cacheInventoryType = InventoryType.BlightStash;
                case 88:
                    return this._cacheInventoryType = InventoryType.DeliriumStash;
                case 111:
                    return this._cacheInventoryType = InventoryType.EssenceStash;
                default:
                    return this._cacheInventoryType = InventoryType.InvalidInventory;
            }
        }

        private Element GetInventoryRootElement()
        {
            switch (this.InvType)
            {
                case InventoryType.PlayerInventory:
                case InventoryType.NormalStash:
                case InventoryType.QuadStash:
                    return this;
                case InventoryType.CurrencyStash:
                case InventoryType.BlightStash:
                case InventoryType.DeliriumStash:
                case InventoryType.DelveStash:
                case InventoryType.DivinationStash:
                case InventoryType.EssenceStash:
                case InventoryType.GemStash:
                case InventoryType.FlaskStash:
                case InventoryType.FragmentStash:
                case InventoryType.MapStash:
                case InventoryType.MetamorphStash:
                case InventoryType.UniqueStash:
                    return this.Parent;
                case InventoryType.InvalidInventory:
                default:
                    return null;
            }
        }
    }
}
