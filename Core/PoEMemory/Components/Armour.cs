namespace ExileCore.PoEMemory.Components
{
    public class Armour : Component
    {
        public int EvasionScoreMin => Address != 0 ? M.Read<int>(Address + 0x10, 0x10) : 0;
        public int EvasionScoreMax => Address != 0 ? M.Read<int>(Address + 0x10, 0x14) : 0;

        public int ArmourScoreMin => Address != 0 ? M.Read<int>(Address + 0x10, 0x18) : 0;
        public int ArmourScoreMax => Address != 0 ? M.Read<int>(Address + 0x10, 0x1C) : 0;

        public int EnergyShieldScoreMin => Address != 0 ? M.Read<int>(Address + 0x10, 0x20) : 0;
        public int EnergyShieldScoreMax => Address != 0 ? M.Read<int>(Address + 0x10, 0x24) : 0;
        
        public int WardScoreMin => Address != 0 ? M.Read<int>(Address + 0x10, 0x28) : 0;
        public int WardScoreMax => Address != 0 ? M.Read<int>(Address + 0x10, 0x2C) : 0;

        public int IncreasedMovementSpeedScore => Address != 0 ? M.Read<int>(Address + 0x10, 0x38) : 0;
    }
}
