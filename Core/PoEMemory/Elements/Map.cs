using SharpDX;

namespace ExileCore.PoEMemory.Elements
{
    public class SubMap : Element
    {
        public Vector2 Shift => M.Read<Vector2>(Address + 0x270);
        public Vector2 DefaultShift => M.Read<Vector2>(Address + 0x278);
        public float Zoom => M.Read<float>(Address + 0x2B0);
    }

    public class Map : Element
    {
        //public Element MapProperties => ReadObjectAt<Element>(0x1FC + OffsetBuffers);
        private SubMap _largeMap;
        private SubMap _smallMap;
        public SubMap LargeMap => _largeMap = _largeMap ?? ReadObjectAt<SubMap>(0x280);
        public float LargeMapShiftX => LargeMap.Shift.X;
        public float LargeMapShiftY => LargeMap.Shift.Y;
        public float LargeMapZoom => LargeMap.Zoom;
        public SubMap SmallMiniMap => _smallMap = _smallMap ?? ReadObjectAt<SubMap>(0x288);
        public float SmallMinMapX => SmallMiniMap.Shift.X;
        public float SmallMinMapY => SmallMiniMap.Shift.Y;
        public float SmallMinMapZoom => SmallMiniMap.Zoom;
        public Element OrangeWords => ReadObjectAt<Element>(0x250);
        public Element BlueWords => ReadObjectAt<Element>(0x2A8);
    }
}
