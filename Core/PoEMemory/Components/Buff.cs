using System;
using GameOffsets;

namespace ExileCore.PoEMemory.Components
{
    public class Buff : RemoteMemoryObject
    {
        private readonly Lazy<string> _name;
        private BuffOffsets? _offsets;

        public Buff()
        {
            _name = new Lazy<string>(() =>
                {
                    var formattableString = $"{nameof(Buff)}{BuffOffsets.Name}";
                    string read;
                    var tries = 0;

                    do
                    {
                        read = Cache.StringCache.Read(formattableString,
                            () => M.ReadStringU(M.Read<BuffStringOffsets>(BuffOffsets.Name).String));

                        if (read == string.Empty) Cache.StringCache.Remove(formattableString);

                        tries++;
                    } while (read == string.Empty && tries < 7);

                    return read;
                });
        }

        public BuffOffsets BuffOffsets => (BuffOffsets)(_offsets = _offsets ?? M.Read<BuffOffsets>(Address));
        public string Name => _name.Value;
        public byte Charges => BuffOffsets.Charges;

        //public int SkillId => M.Read<int>(Address + 0x5C); // I think this is part of another structure referenced in a pointer at 0x58
        public float MaxTime => BuffOffsets.MaxTime; // infinity for auras and always on buff
        public float Timer => BuffOffsets.Timer; // timeleft

        public override string ToString()
        {
            return $"{Name} - Charges: {Charges} MaxTime: {MaxTime} Timer: {Timer}";
        }
    }
}