using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExileCore.PoEMemory.Elements;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Helpers;
using GameOffsets;
using SharpDX;

namespace ExileCore.PoEMemory
{
    public class Element : RemoteMemoryObject
    {
        public const int OffsetBuffers = 0;
        private static readonly int IsVisibleLocalOffset = Extensions.GetOffset<ElementOffsets>(nameof(ElementOffsets.IsVisibleLocal));
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
            _cacheElementIsVisibleLocal = new FrameCache<bool>(() => Address != 0 && M.Read<bool>(Address + IsVisibleLocalOffset));
        }

        public ElementOffsets Elem => _cacheElement.Value;
        public bool IsValid => Elem.SelfPointer == Address;
        public long ChildCount => (Elem.ChildEnd - Elem.ChildStart) / 8;
        public bool IsVisibleLocal => (Elem.IsVisibleLocal & 8) == 8;
        public Element Root => TheGame.IngameState.UIRoot;
        public Element Parent => Elem.Parent == 0 ? null : (_parent = _parent ?? GetObject<Element>(Elem.Parent));
        public Vector2 Position => Elem.Position;
        public float X => Elem.X;
        public float Y => Elem.Y;
        public Element Tooltip => Address == 0 ? null : GetObject<Element>(Elem.Tooltip);
        public float Scale => Elem.Scale;
        public float Width => Elem.Width;
        public float Height => Elem.Height;
        public bool IsHighlighted => Elem.isHighlighted;
        [Obsolete("Element.isHighlighted is deprecated. Use IsHighlighted.", false)]
        public bool isHighlighted => IsHighlighted;
        public ColorBGRA BorderColor => new ColorBGRA(Elem.ElementBorderColor);
        public ColorBGRA BackgroundColor => new ColorBGRA(Elem.ElementBackgroundColor);
        public ColorBGRA OverlayColor => new ColorBGRA(Elem.ElementOverlayColor);

        public ColorBGRA TextBoxBorderColor => new ColorBGRA(Elem.TextBoxBorderColor);
        public ColorBGRA TextBoxBackgroundColor => new ColorBGRA(Elem.TextBoxBackgroundColor);
        public ColorBGRA TextBoxOverlayColor => new ColorBGRA(Elem.TextBoxOverlayColor);

        public ColorBGRA HighlightBorderColor => new ColorBGRA(Elem.HighlightBorderColor);

        public virtual string Text => Sanitize(AsObject<EntityLabel>().Text);

        public virtual string LongText => Sanitize(AsObject<EntityLabel>().Text3);

        private static string Sanitize(string text)
        {
            return !string.IsNullOrWhiteSpace(text) ? text.Replace("\u00A0\u00A0\u00A0\u00A0", "{{icon}}") : string.Empty;
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

        public int? IndexInParent => Parent?.Children.IndexOf(this);

        public string PathFromRoot
        {
            get
            {
                var parentChain = GetParentChain();
                if (parentChain.Count != 0)
                {
                    parentChain.RemoveAt(parentChain.Count - 1);
                    parentChain.Reverse();
                }
                parentChain.Add(this);
                var properties = (from property in TheGame.IngameState.IngameUi.GetType().GetProperties()
                                  where typeof(Element).IsAssignableFrom(property.PropertyType)
                                  where property.GetIndexParameters().Length == 0
                                  let value = property.GetValue(TheGame.IngameState.IngameUi) as Element
                                  where value?.Address == parentChain.First().Address
                                  select property.Name).ToList();

                return (properties.Count > 0 ? $"({properties.First()})" : "") + string.Join("->", parentChain.Select(x => x.IndexInParent));
            }
        }

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

        private List<Element> GetParentChain()
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

        public bool TryGetChildFromIndices(out Element child, params int[] indices)
        {
            child = default(Element);

            var currentElement = this;
            foreach (var index in indices)
            {
                currentElement = currentElement[index];

                if (currentElement == null)
                {
                    return false;
                }
            }
            child = currentElement;
            return true;
        }

        public Element GetChildFromIndices(params int[] indices)
        {
            var currentElement = this;

            StringBuilder BuildErrorString(int errorIndex)
            {
                var str = new StringBuilder();
                foreach (var i in indices)
                {
                    if (i == errorIndex)
                    {
                        str.Append('>');
                    }

                    str.AppendFormat("[{0}] ", i);
                    if (i == errorIndex)
                    {
                        str.Append('<');
                    }
                }

                return str;
            }

            for (var indexNumber = 0; indexNumber < indices.Length; indexNumber++)
            {
                var index = indices[indexNumber];
                currentElement = currentElement.GetChildAtIndex(index);

                if (currentElement == null)
                {
                    // TODO: Should probably throw instead of spamming message.
                    DebugWindow.LogDebug($"{nameof(Element)} with index {index} was not found. Indices: {BuildErrorString(indexNumber)}");
                    return null;
                }

                if (currentElement.Address == 0)
                {
                    // TODO: Should probably throw instead of spamming message.
                    DebugWindow.LogDebug($"{nameof(Element)} with index {index} has address = 0. Indices: {BuildErrorString(indexNumber)}");

                    // TODO: Be consistent and just return null since we didn't find the actual element we wanted. This requires user check if
                    //       the returned element is has a valid address, something we know isn't true already.
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
