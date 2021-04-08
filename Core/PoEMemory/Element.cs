using System.Collections.Generic;
using System.Linq;
using ExileCore.PoEMemory.Elements;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Helpers;
using GameOffsets;
using MoreLinq;
using SharpDX;

namespace ExileCore.PoEMemory
{
    public class Element : RemoteMemoryObject
    {
        public const int OffsetBuffers = 0;
        private static readonly int IsVisibleLocalOff = Extensions.GetOffset<ElementOffsets>(nameof(ElementOffsets.IsVisibleLocal));
        private static readonly int ChildStartOffset = Extensions.GetOffset<ElementOffsets>(nameof(ElementOffsets.ChildStart));

        // dd id
        // dd (something zero)
        // 16 dup <128-bytes structure>
        // then the rest is
        private readonly CachedValue<ElementOffsets> _cacheElement;
        private CachedValue<bool> _cacheElementIsVisibleLocal;
        private readonly List<Element> _childrens = new List<Element>();
        private CachedValue<RectangleF> _getClientRect;

        // public Element Root => Elem.Root==0 ?null: GetObject<Element>(M.Read<long>(Elem.Root+0xE8));
        private Element _parent;
        private long childHashCache;

        public Element()
        {
            _cacheElement = new FrameCache<ElementOffsets>(() => Address == 0 ? default : M.Read<ElementOffsets>(Address));
            _cacheElementIsVisibleLocal = new FrameCache<bool>(() => Address == 0 ? default : M.Read<bool>(Address + IsVisibleLocalOff));
        }

        public ElementOffsets Elem => _cacheElement.Value;
        public bool IsValid => Elem.SelfPointer == Address;

        //public int vTable => Elem.vTable;
        public long ChildCount => (Elem.ChildEnd - Elem.ChildStart) / 8;

        //public bool IsVisibleLocal => Address!=0 && _cacheElementIsVisibleLocal.Value;
        public bool IsVisibleLocal => (Elem.IsVisibleLocal & 4) == 4;
        public Element Root => TheGame.IngameState.UIRoot;
        public Element Parent => Elem.Parent == 0 ? null : _parent ?? (_parent = GetObject<Element>(Elem.Parent));
        public Vector2 Position => Elem.Position;
        public float X => Elem.X;
        public float Y => Elem.Y;
        public Element Tooltip => Address == 0 ? null : GetObject<Element>(M.Read<long>(Address + 0x338)); //0x7F0
        public float Scale => Elem.Scale;
        public float Width => Elem.Width;
        public float Height => Elem.Height;
        public bool isHighlighted => Elem.isHighlighted;

        public virtual string Text
        {
            get
            {
                var text = AsObject<EntityLabel>().Text2;
                if (!string.IsNullOrWhiteSpace(text)) return text.Replace("\u00A0\u00A0\u00A0\u00A0", "{{icon}}");
                return null;
            }
        }

        public virtual string LongText
        {
            get
            {
                var text = AsObject<EntityLabel>().Text3;
                if (!string.IsNullOrWhiteSpace(text)) return text.Replace("\u00A0\u00A0\u00A0\u00A0", "{{icon}}");
                return null;
            }
        }

        /*public virtual string Text
        {
            get
            {
                var text = Elem.TestString.ToString(M);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.Replace("\u00A0\u00A0\u00A0\u00A0","{{icon}}");
                }
                return null;
            }
        }*/

        public bool IsVisible
        {
            get
            {
                //998
                if (Address >= 1770350607106052 || Address <= 0) return false;
                return IsVisibleLocal && GetParentChain().All(current => current.IsVisibleLocal);
            }
        }

        public IList<Element> Children => GetChildren<Element>();
        public long ChildHash => Elem.Childs.GetHashCode();
        public RectangleF GetClientRectCache =>
            _getClientRect?.Value ?? (_getClientRect = new TimeCache<RectangleF>(GetClientRect, 200)).Value;
        public Element this[int index] => GetChildAtIndex(index);

        protected List<Element> GetChildren<T>() where T : Element
        {
            var e = Elem;
            if (Address == 0 || e.ChildStart == 0 || e.ChildEnd == 0 || ChildCount < 0) return _childrens;

            if (ChildHash == childHashCache)
                return _childrens;

            var pointers = M.ReadPointersArray(e.ChildStart, e.ChildEnd);

            if (pointers.Count != ChildCount) return _childrens;
            _childrens.Clear();

            foreach (var pointer in pointers)
            {
                _childrens.Add(GetObject<Element>(pointer));
            }

            childHashCache = ChildHash;
            return _childrens;
        }

        public List<T> GetChildrenAs<T>() where T : Element, new()
        {
            var e = Elem;
            if (Address == 0 || e.ChildStart == 0 || e.ChildEnd == 0 || ChildCount < 0) return new List<T>();

            var pointers = M.ReadPointersArray(e.ChildStart, e.ChildEnd);

            if (pointers.Count != ChildCount)
                return new List<T>();

            var results = new List<T>();

            foreach (var pointer in pointers)
            {
                results.Add(GetObject<T>(pointer));
            }

            return results;
        }

        private IList<Element> GetParentChain()
        {
            var list = new List<Element>();

            if (Address == 0)
                return list;

            var hashSet = new HashSet<Element>();
            var root = Root;
            var parent = Parent;

            if (root == null || parent == null)
                return list;

            while (!hashSet.Contains(parent) && root.Address != parent.Address && parent.Address != 0)
            {
                list.Add(parent);
                hashSet.Add(parent);
                parent = parent.Parent;

                if (parent == null)
                    break;
            }

            return list;
        }

        public Vector2 GetParentPos()
        {
            float num = 0;
            float num2 = 0;
            var rootScale = TheGame.IngameState.UIRoot.Scale;

            foreach (var current in GetParentChain())
            {
                num += current.X * current.Scale / rootScale;
                num2 += current.Y * current.Scale / rootScale;
            }

            return new Vector2(num, num2);
        }

        public virtual RectangleF GetClientRect()
        {
            if (Address == 0) return RectangleF.Empty;
            var vPos = GetParentPos();
            float width = TheGame.IngameState.Camera.Width;
            float height = TheGame.IngameState.Camera.Height;
            var ratioFixMult = width / height / 1.6f;
            var xScale = width / 2560f / ratioFixMult;
            var yScale = height / 1600f;

            var rootScale = TheGame.IngameState.UIRoot.Scale;
            var num = (vPos.X + X * Scale / rootScale) * xScale;
            var num2 = (vPos.Y + Y * Scale / rootScale) * yScale;
            return new RectangleF(num, num2, xScale * Width * Scale / rootScale, yScale * Height * Scale / rootScale);
        }

        public Element GetChildFromIndices(params int[] indices)
        {
            var poe_UElement = this;

            foreach (var index in indices)
            {
                poe_UElement = poe_UElement.GetChildAtIndex(index);

                if (poe_UElement == null)
                {
                    var str = "";
                    indices.ForEach(i => str += $"[{i}] ");
                    DebugWindow.LogMsg($"{nameof(Element)} with index: {index} not found. Indices: {str}");
                    return null;
                }

                if (poe_UElement.Address == 0)
                {
                    var str = "";
                    indices.ForEach(i => str += $"[{i}] ");
                    DebugWindow.LogMsg($"{nameof(Element)} with index: {index} 0 address. Indices: {str}");
                    return GetObject<Element>(0);
                }
            }

            return poe_UElement;
        }

        public Element GetChildAtIndex(int index)
        {
            return index >= ChildCount ? null : GetObject<Element>(M.Read<long>(Address + ChildStartOffset, index * 8));
        }
         public void GetAllStrings(List<string> res) {
            if(Text?.Length > 0) {
                res.Add(Text);
            }
            foreach(var ch in Children)
                ch.GetAllStrings(res);
        }
        public void GetAllTextElements(List<Element> res) {
            if(Text?.Length > 0) {
                res.Add(this);
            }
            foreach(var ch in Children)
                ch.GetAllTextElements(res);
        }
        public void GetElementByString(string str, ref Element res) {
            if(Text == str) {
                res = this;
                return;
            }
            foreach(var ch in Children)
                ch.GetElementByString(str, ref res);
        }
    }
}
