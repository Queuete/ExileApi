using ExileCore.PoEMemory.MemoryObjects;

namespace ExileCore.PoEMemory.Elements
{
    public class EntityLabel : Element
    {
        long b_offs = 0x378; //base offset for NativeStringU struct
        public int Length
        {
            get
            {
                var num = (int)M.Read<long>(Address + b_offs + 0x10); //NativeStringU + 0x10
                return num <= 0 || num > 1024 ? 0 : num;
            }
        }

        public int Capacity
        {
            get
            {
                var num = (int)M.Read<long>(Address + b_offs+ 0x18); //NativeStringU + 0x18
                return num <= 0 || num > 1024 ? 0 : num;
            }
        }

        public override string Text
        {
            get
            {
                var length = Length;

                if (length <= 0 || length > 1024)
                {
                    return string.Empty;
                }

                var address = Capacity < 8 ? Address + b_offs : M.Read<long>(Address + b_offs);
                return M.ReadStringU(address, length * 2, false);
            }
        }

        public string Text2 => NativeStringReader.ReadString(Address + b_offs, M);//same like all  this

        public string Text3 => NativeStringReader.ReadStringLong(Address + 0x608, M); // not sure how to check this
    }
}
