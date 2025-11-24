namespace PlcHelper
{
    public partial class ApHelper
    {
        /// <summary>
        /// DB 地址
        /// </summary>
        readonly int EnergyDBAddress = 40;

        // Electric
        /// <summary>
        /// 总电量，双字
        /// </summary>
        readonly int TotalActiveElectricalEnergy = 0;
        readonly int AVoltage = 4;
        readonly int BVoltage = 6;
        readonly int CVoltage = 8;
        readonly int ACurrent = 10;
        readonly int BCurrent = 12;
        readonly int CCurrent = 14;
        // readonly int AActivePower = 16;
        // readonly int BActivePower = 18;
        // readonly int CActivePower = 20;
        readonly int TotalActivePower = 22;
        readonly int Frequency = 48;

        readonly int CuttingOxyginPress = 126;
        readonly int CuttingOxyginInstantaneousFlow = 134;
        readonly int CuttingOxyginAccumulatedFlow = 146;
        readonly int GasPress = 154;
        readonly int GasInstantaneousFlow = 162;
        readonly int GasAccumulatedFlow = 174;
        readonly int PreheatOxyginPress = 182;
        readonly int PreheatOxyginInstantaneousFlow = 190;
        readonly int PreheatOxyginAccumulatedFlow = 202;
    }
}