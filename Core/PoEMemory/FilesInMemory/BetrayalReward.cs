﻿namespace ExileCore.PoEMemory.FilesInMemory
{
    public class BetrayalReward : RemoteMemoryObject
    {
        public BetrayalJob Job => TheGame.Files.BetrayalJobs.GetByAddress(M.Read<long>(Address + 0x0));
        public BetrayalTarget Target => TheGame.Files.BetrayalTargets.GetByAddress(M.Read<long>(Address + 0x10));
        public BetrayalRank Rank => TheGame.Files.BetrayalRanks.GetByAddress(M.Read<long>(Address + 0x20));
        public string Reward => M.ReadStringU(M.Read<long>(Address + 0x30));

        public override string ToString()
        {
            return Reward;
        }
    }
}
