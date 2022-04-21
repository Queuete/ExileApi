using ExileCore.Shared.Cache;
using GameOffsets;
using GameOffsets.Native;
using SharpDX;

namespace ExileCore.PoEMemory.Components
{
    public class Positioned : Component
    {
        private readonly CachedValue<PositionedComponentOffsets> _cachedValue;

        public Positioned()
        {
            _cachedValue = new FrameCache<PositionedComponentOffsets>(() => M.Read<PositionedComponentOffsets>(Address));
        }

        public PositionedComponentOffsets PositionedStruct => _cachedValue.Value;
        public new long OwnerAddress => PositionedStruct.OwnerAddress;
        public int GridX => PositionedStruct.GridX;
        public int GridY => PositionedStruct.GridY;
        public Vector2 GridPos => new Vector2(GridX, GridY);
        public Vector2i GridPosI => new Vector2i(GridX, GridY);
        public Vector2 WorldPos => new Vector2(WorldX, WorldY);
        public float Rotation => PositionedStruct.Rotation;
        public float WorldX => PositionedStruct.WorldX;
        public float WorldY => PositionedStruct.WorldY;
        public float RotationDeg => Rotation * (180 / MathUtil.Pi);
        public byte Reaction => PositionedStruct.Reaction;
        public int Size => PositionedStruct.Size;
        public float Scale => PositionedStruct.Scale;
    }
}
