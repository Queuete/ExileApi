using ExileCore.PoEMemory.Elements.InventoryElements;
using System.Collections.Generic;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class RitualWindow : Element
    {
        public Element ItemList => GetChildAtIndex(11);
        public IList<NormalInventoryItem> Items => ExtractNormalInventoryItems(ItemList?.Children);

        private IList<NormalInventoryItem> ExtractNormalInventoryItems(IList<Element> children)
        {
            var resultList = new List<NormalInventoryItem>();
            if (children.Count > 0)
            {
                for (var i = 1; i < children.Count; i++)
                {
                    resultList.Add(children[i].AsObject<NormalInventoryItem>());
                }
            }
            return resultList;
        }
    }
}
