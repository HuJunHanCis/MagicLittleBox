using System;
using S7.Net;

namespace PlcHelper
{
    public partial class ApHelper
    {
        /// <summary>
        /// 读取总电量
        /// </summary>
        /// <returns>总电量</returns>
        public UInt32 GetTotalActiveEnergy()
        {
            return (UInt32)myPlc.Read(DataType.DataBlock, EnergyDBAddress, TotalActiveElectricalEnergy, VarType.DWord, 1);
        }

        /// <summary>
        /// 读取A相电压
        /// </summary>
        /// <returns>A相电压值</returns>
        public Int16 GetAVoltage()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, EnergyDBAddress, AVoltage, VarType.Int, 1);
        }

        /// <summary>
        /// 读取B相电压
        /// </summary>
        /// <returns>B相电压值</returns>
        public Int16 GetBVoltage()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, EnergyDBAddress, BVoltage, VarType.Word, 1);
        }

        /// <summary>
        /// 读取C相电压
        /// </summary>
        /// <returns>C相电压值</returns>
        public Int16 GetCVoltage()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, EnergyDBAddress, CVoltage, VarType.Word, 1);
        }

        /// <summary>
        /// 读取A相电流
        /// </summary>
        /// <returns>A相电流值</returns>
        public Int16 GetACurrent()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, EnergyDBAddress, ACurrent, VarType.Word, 1);
        }

        /// <summary>
        /// 读取B相电流
        /// </summary>
        /// <returns>B相电流值</returns>
        public Int16 GetBCurrent()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, EnergyDBAddress, BCurrent, VarType.Word, 1);
        }

        /// <summary>
        /// 读取C相电流
        /// </summary>
        /// <returns>C相电流值</returns>
        public Int16 GetCCurrent()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, EnergyDBAddress, CCurrent, VarType.Word, 1);
        }

        /// <summary>
        /// 读取总有功功率
        /// </summary>
        /// <returns>总有功功率值</returns>
        public Int16 GetTotalActivePower()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, EnergyDBAddress, TotalActivePower, VarType.Word, 1);
        }

        /// <summary>
        /// 读取电源频率
        /// </summary>
        /// <returns>工频电源频率</returns>
        public Int16 GetPowerFrequency()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, EnergyDBAddress, Frequency, VarType.Word, 1);
        }

        /// <summary>
        /// 读取切割氧压力
        /// </summary>
        /// <returns></returns>
        public float GetCuttingOxyginPress()
        {
            return (float)myPlc.Read(DataType.DataBlock, EnergyDBAddress, CuttingOxyginPress, VarType.Real, 1);
        }

        /// <summary>
        /// 读取切割氧瞬时流量
        /// </summary>
        /// <returns></returns>
        public float GetCuttingOxyginInstantaneousFlow()
        {
            return (float)myPlc.Read(DataType.DataBlock, EnergyDBAddress, CuttingOxyginInstantaneousFlow, VarType.Real, 1);
        }

        /// <summary>
        /// 读取切割氧累计流量
        /// </summary>
        /// <returns></returns>
        public float GetCuttingOxyginAccumulatedFlow()
        {
            return (float)myPlc.Read(DataType.DataBlock, EnergyDBAddress, CuttingOxyginAccumulatedFlow, VarType.Real, 1);
        }

        /// <summary>
        /// 读取燃气压力
        /// </summary>
        /// <returns></returns>
        public float GetGasPress()
        {
            return (float)myPlc.Read(DataType.DataBlock, EnergyDBAddress, GasPress, VarType.Real, 1);
        }

        /// <summary>
        /// 读取燃气瞬时流量
        /// </summary>
        /// <returns></returns>
        public float GetGasInstantaneousFlow()
        {
            return (float)myPlc.Read(DataType.DataBlock, EnergyDBAddress, GasInstantaneousFlow, VarType.Real, 1);
        }

        /// <summary>
        /// 读取燃气累计流量
        /// </summary>
        /// <returns></returns>
        public float GetGasAccumulatedFlow()
        {
            return (float)myPlc.Read(DataType.DataBlock, EnergyDBAddress, GasAccumulatedFlow, VarType.Real, 1);
        }

        /// <summary>
        /// 读取预热氧压力
        /// </summary>
        /// <returns></returns>
        public float GetPreheatOxyginPress()
        {
            return (float)myPlc.Read(DataType.DataBlock, EnergyDBAddress, PreheatOxyginPress, VarType.Real, 1);
        }

        /// <summary>
        /// 读取预热氧瞬时流量
        /// </summary>
        /// <returns></returns>
        public float GetPreheatOxyginInstantaneousFlow()
        {
            return (float)myPlc.Read(DataType.DataBlock, EnergyDBAddress, PreheatOxyginInstantaneousFlow, VarType.Real, 1);
        }

        /// <summary>
        /// 读取预热氧累计流量
        /// </summary>
        /// <returns></returns>
        public float GetPreheatOxyginAccumulatedFlow()
        {
            return (float)myPlc.Read(DataType.DataBlock, EnergyDBAddress, PreheatOxyginAccumulatedFlow, VarType.Real, 1);
        }
    }
}