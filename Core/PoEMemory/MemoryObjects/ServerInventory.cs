using System.Collections.Generic;
using System.Linq;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using GameOffsets;
using GameOffsets.Native;
using SharpDX;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class ServerInventory : RemoteMemoryObject
    {
        private readonly CachedValue<ServerInventoryOffsets> cachedValue;
        private readonly int hashReadLimit = 1500;
        public ServerInventory()
        {
            cachedValue = new FrameCache<ServerInventoryOffsets>(() => M.Read<ServerInventoryOffsets>(Address));
        }

        private ServerInventoryOffsets Struct => cachedValue.Value;
        public InventoryTypeE InventType => (InventoryTypeE)Struct.InventType;
        public InventorySlotE InventSlot => (InventorySlotE)Struct.InventSlot;
        public int Columns => Struct.Columns;
        public int Rows => Struct.Rows;
        public bool IsRequested => Struct.IsRequested == 1;
        public long CountItems => Struct.CountItems;
        public int TotalItemsCounts => Struct.TotalItemsCount;
        public int ServerRequestCounter => Struct.ServerRequestCounter;
        public IList<InventSlotItem> InventorySlotItems => ReadHashMap(Struct.InventorySlotItemsPtr, hashReadLimit).Values.ToList();
        public long Hash => Struct.Hash;
        public IList<Entity> Items => InventorySlotItems.Select(x => x.Item).ToList();

        public InventSlotItem this[int x, int y]
        {
            get
            {
                var invAddr = Struct.InventoryItemsPtr;
                y *= Columns;
                var itmAddr = M.Read<long>(invAddr + (x + y) * 0x08);

                if (itmAddr <= 0)
                    return null;

                return GetObject<InventSlotItem>(itmAddr);
            }
        }

        public Dictionary<int, InventSlotItem> ReadHashMap(long pointer, int limitMax)
        {
            var result = new Dictionary<int, InventSlotItem>();

            var stack = new Stack<HashNode>();
            var startNode = GetObject<HashNode>(pointer);
            var item = startNode.Root;
            stack.Push(item);

            while (stack.Count != 0)
            {
                var node = stack.Pop();

                if (!node.IsNull)
                    result[node.Key] = node.Value1;

                var prev = node.Previous;

                if (!prev.IsNull)
                    stack.Push(prev);

                var next = node.Next;

                if (!next.IsNull)
                    stack.Push(next);

                if (limitMax-- < 0)
                {
                    DebugWindow.LogError($"Fixed possible memory leak (ServerInventory.ReadHashMap @ {Address:X})");
                    break;
                }
            }

            return result;
        }

        public class HashNode : RemoteMemoryObject
        {
            private readonly FrameCache<NativeHashNode> frameCache;

            public HashNode()
            {
                frameCache = new FrameCache<NativeHashNode>(() => M.Read<NativeHashNode>(Address));
            }

            private NativeHashNode NativeHashNode => frameCache.Value;
            public HashNode Previous => GetObject<HashNode>(NativeHashNode.Previous);
            public HashNode Root => GetObject<HashNode>(NativeHashNode.Root);
            public HashNode Next => GetObject<HashNode>(NativeHashNode.Next);

            //public readonly byte Unknown;
            public bool IsNull => NativeHashNode.IsNull != 0;

            //private readonly byte byte_0;
            //private readonly byte byte_1;
            public int Key => NativeHashNode.Key;

            //public readonly int Useless;
            public InventSlotItem Value1 => GetObject<InventSlotItem>(NativeHashNode.Value1);

            //public readonly long Value2;
        }

        public class InventSlotItem : RemoteMemoryObject
        {
            public Vector2 InventoryPosition => Location.InventoryPosition;
            private ItemMinMaxLocation Location => M.Read<ItemMinMaxLocation>(Address + 0x08);
            public Entity Item => ReadObject<Entity>(Address);
            public int PosX => M.Read<int>(Address + 0x8);
            public int PosY => M.Read<int>(Address + 0xc);
            public int SizeX => M.Read<int>(Address + 0x10) - PosX;
            public int SizeY => M.Read<int>(Address + 0x14) - PosY;
            private RectangleF ClientRect => GetClientRect();

            public RectangleF GetClientRect()
            {
                var playerInventElement = TheGame.IngameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory];
                var inventClientRect = playerInventElement.GetClientRect();
                var cellSize = inventClientRect.Width / 12;
                return Location.GetItemRect(inventClientRect.X, inventClientRect.Y, cellSize);
            }

            public override string ToString()
            {
                return $"InventSlotItem: {Location}, Item: {Item}";
            }

            private struct ItemMinMaxLocation
            {
                private int XMin { get; set; }
                private int YMin { get; set; }
                private int XMax { get; set; }
                private int YMax { get; set; }

                public RectangleF GetItemRect(float invStartX, float invStartY, float cellsize)
                {
                    return new RectangleF(
                        invStartX + cellsize * XMin,
                        invStartY + cellsize * YMin,
                        (XMax - XMin) * cellsize,
                        (YMax - YMin) * cellsize);
                }

                public Vector2 InventoryPosition => new Vector2(XMin, YMin);

                public override string ToString()
                {
                    return $"({XMin}, {YMin}, {XMax}, {YMax})";
                }
            }
        }
    }
}
