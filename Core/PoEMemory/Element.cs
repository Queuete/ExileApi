using System;
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
        private readonly CachedValue<bool> _cacheElementIsVisibleLocal;
        private readonly List<Element> _childrens = new List<Element>();
        private CachedValue<RectangleF> _getClientRect;

        private Element _parent;
        private long childHashCache;

        public Element()
        {
            _cacheElement = new FrameCache<ElementOffsets>(() => Address == 0 ? default : M.Read<ElementOffsets>(Address));
            _cacheElementIsVisibleLocal = new FrameCache<bool>(() => Address != 0 && M.Read<bool>(Address + IsVisibleLocalOff));
        }

        public ElementOffsets Elem => _cacheElement.Value;
        public bool IsValid => Elem.SelfPointer == Address;
        public long ChildCount => (Elem.ChildEnd - Elem.ChildStart) / 8;
        public bool IsVisibleLocal => (Elem.IsVisibleLocal & 8) == 8;
        public Element Root => TheGame.IngameState.UIRoot;
        public Element Parent => Elem.Parent == 0 ? null : (_parent ??= GetObject<Element>(Elem.Parent));
        public Vector2 Position => Elem.Position;
        public float X => Elem.X;
        public float Y => Elem.Y;
        public Element Tooltip => Address == 0 ? null : GetObject<Element>(M.Read<long>(Address + 0x338));
        public Element Tooltip2 => Address == 0 ? null : GetObject<Element>(M.Read<long>(Address + 0x418)); //working with Ultimatum window with three selection icons
        public bool  IsMouseOver => M.Read<byte>(Address + 0x238) == 1; //Works with most button-like elements. Doesn't work with elem that look like a link (in NPC dialogs)
        public float Scale => Elem.Scale;
        public float Width => Elem.Width;
        public float Height => Elem.Height;
        public bool IsHighlighted => Elem.isHighlighted;
        [Obsolete("Use IsHighlighted instead of isHighlighted, this will be removed for 3.15")] // remove this property at end of 3.14
        public bool isHighlighted => IsHighlighted;

        public ColorBGRA BorderColor => new ColorBGRA(Elem.ElementBorderColor);
        public ColorBGRA BackgroundColor => new ColorBGRA(Elem.ElementBackgroundColor);
        public ColorBGRA OverlayColor => new ColorBGRA(Elem.ElementOverlayColor);

        public ColorBGRA TextBoxBorderColor => new ColorBGRA(Elem.TextBoxBorderColor);
        public ColorBGRA TextBoxBackgroundColor => new ColorBGRA(Elem.TextBoxBackgroundColor);
        public ColorBGRA TextBoxOverlayColor => new ColorBGRA(Elem.TextBoxOverlayColor);

        public virtual string Text
        {
            get
            {
                var text = AsObject<EntityLabel>().Text2;
                return !string.IsNullOrWhiteSpace(text) ? text.Replace("\u00A0\u00A0\u00A0\u00A0", "{{icon}}") : null;
            }
        }

        public virtual string LongText
        {
            get
            {
                var text = AsObject<EntityLabel>().Text3;
                return !string.IsNullOrWhiteSpace(text) ? text.Replace("\u00A0\u00A0\u00A0\u00A0", "{{icon}}") : null;
            }
        }

        public bool IsVisible
        {
            get
            {
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

            _childrens.AddRange(pointers.Select(GetObject<Element>).ToList());
            childHashCache = ChildHash;
            return _childrens;
        }

        public List<T> GetChildrenAs<T>() where T : Element, new()
        {
            var e = Elem;
            if (Address == 0 || e.ChildStart == 0 || e.ChildEnd == 0 || ChildCount < 0) return new List<T>();

            var pointers = M.ReadPointersArray(e.ChildStart, e.ChildEnd);

            return pointers.Count != ChildCount ? new List<T>() : pointers.Select(GetObject<T>).ToList();
        }

        private IList<Element> GetParentChain()
        {
            var list = new List<Element>();

            if (Address == 0)
                return list;

            var hashSet = new HashSet<Element>();
            var root = Root;
            var parent = Parent;

            if (root == null)
                return list;

            while (parent != null && !hashSet.Contains(parent) && root.Address != parent.Address && parent.Address != 0)
            {
                list.Add(parent);
                hashSet.Add(parent);
                parent = parent.Parent;
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
            var currentElement = this;

            foreach (var index in indices)
            {
                currentElement = currentElement.GetChildAtIndex(index);

                if (currentElement == null)
                {
                    var str = "";
                    indices.ForEach(i => str += $"[{i}] ");
                    DebugWindow.LogMsg($"{nameof(Element)} with index: {index} not found. Indices: {str}");
                    return null;
                }

                if (currentElement.Address == 0)
                {
                    var str = "";
                    indices.ForEach(i => str += $"[{i}] ");
                    DebugWindow.LogMsg($"{nameof(Element)} with index: {index} 0 address. Indices: {str}");
                    return GetObject<Element>(0);
                }
            }

            return currentElement;
        }

        public Element GetChildAtIndex(int index)
        {
            return index >= ChildCount ? null : GetObject<Element>(M.Read<long>(Address + ChildStartOffset, index * 8));
        }
        public void GetAllStrings(List<string> res)
        {
            if (Text?.Length > 0)
            {
                res.Add(Text);
            }
            foreach (var ch in Children)
                ch.GetAllStrings(res);
        }
        public void GetAllTextElements(List<Element> res)
        {
            if (Text?.Length > 0)
            {
                res.Add(this);
            }
            foreach (var ch in Children)
                ch.GetAllTextElements(res);
        }
        public Element GetElementByString(string str)
        {
            return Text == str ? this : Children.Select(child => child.GetElementByString(str)).FirstOrDefault(element => element != null);
        }
    }
}
