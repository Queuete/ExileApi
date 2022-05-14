using System;
using System.Collections.Generic;
using ExileCore.Shared.Interfaces;

namespace ExileCore.PoEMemory
{
    public abstract class FileInMemory
    {
        private readonly Func<long> fAddress;

        protected FileInMemory(IMemory m, Func<long> address)
        {
            M = m;
            Address = address();
            fAddress = address;
        }

        public IMemory M { get; }
        public long Address { get; }
        private int NumberOfRecords => M.Read<int>(fAddress() + 0x30, 0x40);

        protected IEnumerable<long> RecordAddresses()
        {
            if (fAddress() == 0)
            {
                yield return 0;
                yield break;
            }

            var cnt = NumberOfRecords;

            if (cnt == 0)
            {
                yield return 0;
                yield break;
            }

            var firstRec = M.Read<long>(fAddress() + 0x30, 0x0);
            var lastRec = M.Read<long>(fAddress() + 0x30, 0x8);

            var recLen = (lastRec - firstRec) / cnt;

            for (var i = 0; i < cnt; i++)
            {
                yield return firstRec + i * recLen;
            }
        }
    }
}
