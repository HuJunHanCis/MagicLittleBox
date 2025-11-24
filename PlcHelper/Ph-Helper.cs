using System;
using System.Threading;
using S7.Net;
using OtherHelper;

namespace PlcHelper
{
    public partial class ApHelper
    {
        Plc myPlc;
        public Boolean Online { get; set; }
        public ApHelper(string ip)
        {
            myPlc = new Plc(CpuType.S71200, ip, 0, 0);
            try
            {
                myPlc.Open();
                Online = true;
            }
            catch (Exception e1)
            {
                Online = false;
                // Console.WriteLine(e1.Message);
                // MessageBox.Show(e1.Message);
            }
        }
        public void Disconnect()
        {
            try
            {
                if (myPlc != null && myPlc.IsConnected)
                {
                    myPlc.Close();
                    ((IDisposable)myPlc).Dispose();
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine("PLC释放失败：" + ex.Message);
            }
            finally
            {
                myPlc = null;
                Online = false;
            }
        }
        public int EnsureTrussEnabled(int delayInSecond)
        {
            int count = 0;

            while (!IsXEnabled() || !IsYEnabled())
            {
                try
                {
                    XEnable(true);
                    YEnable(true);
                }
                catch (Exception)
                {

                }
                Thread.Sleep(100);
                if (count++ > 10 * delayInSecond)
                {
                    return -1;
                }
            }
            return 0;
        }

        private Int16 BooleanToInt16(Boolean value)
        {
            return (Int16)(value ? 1 : 0);
        }

        #region X轴设定与控制


        /// <summary>
        /// X轴清除运动命令
        /// </summary>
        public void XClearMovingCmds()
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XJogPAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XJogNAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XStartPositioningAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XRestartAddress, (Int16)0);
        }

        /// <summary>
        /// X轴清除所有命令
        /// </summary>
        public void XClearAllCommands()
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XJogPAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XJogNAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XStartPositioningAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XRestartAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XStopAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XPauseAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XReferencePointCalibratedAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XStartToHomeAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, XAlarmResetedAddress, (Int16)0);
        }

        /// <summary>
        /// X轴使能
        /// </summary>
        public void XEnable(bool value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XEnableAddress, value.ToInt16());
        }

        /// <summary>
        /// X轴正向手动
        /// </summary>
        public void XJogP(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XJogPAddress, value.ToInt16());
        }

        /// <summary>
        /// X轴负向手动
        /// </summary>
        public void XJogN(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XJogNAddress, value.ToInt16());
        }

        /// <summary>
        /// X轴启动定位
        /// </summary>
        public void XStart(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XStartPositioningAddress, value.ToInt16());
        }

        /// <summary>
        /// X轴回原点启动
        /// </summary>
        public void XStartToHome(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XStartToHomeAddress, value.ToInt16());
        }

        /// <summary>
        /// X轴启动零点校准
        /// </summary>
        public void XStartReferencePointCalibrating(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XStartCalibratingReferencePointAddress, value.ToInt16());
        }

        /// <summary>
        /// X轴设定定位位置
        /// </summary>
        public void XSetPosition(float position)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XSetPositionAddress, position);

        }
        /// <summary>
        /// 获取X轴位置的设定值
        /// </summary>
        /// <returns></returns>
        public float GetXSetPosition()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XSetPositionAddress, VarType.Real, 1);
        }
        /// <summary>
        /// 获取X轴的设定速度
        /// </summary>
        /// <returns></returns>
        public float GetXSetSpeed()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XSetSpeedAddress, VarType.Real, 1);
        }

        /// <summary>
        /// X轴设定定位速度
        /// </summary>
        public void XSetSpeed(float speed)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XSetSpeedAddress, speed);
        }

        /// <summary>
        /// X轴报警清除
        /// </summary>
        public void XResetAlarm(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XResetAlarmAddress, value.ToInt16());
        }

        /// <summary>
        /// X轴重新启动
        /// </summary>
        /// <param name="value">设定值，true： 置1， false:清零</param>
        public void XRestart(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XRestartAddress, value.ToInt16());
        }

        /// <summary>
        /// X轴设定Jog速度
        /// </summary>
        public void XSetJogSpeed(float jogSpeed)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XSetJogSpeedAddress, jogSpeed);
        }

        /// <summary>
        /// X轴暂停
        /// </summary>
        public void XPause(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XPauseAddress, value.ToInt16());
        }

        /// <summary>
        /// X轴停止
        /// </summary>
        public void XStop(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XStopAddress, value.ToInt16());
        }

        /// <summary>
        /// X轴加速度设置
        /// </summary>
        public void XSetAcceleration(float acceleration)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XSetAccelerationAddress, acceleration);
        }

        /// <summary>
        /// X轴减速度设置
        /// </summary>
        public void XSetDcceleration(float deceleration)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, XSetDecelerationAddress, deceleration);
        }



        #endregion


        #region X轴状态

        public bool IsXError()
        {
            return (bool)myPlc.Read(DataType.DataBlock, DBAddress, 230, VarType.Byte, 1, 0);
        }
        public UInt16 XErrorID()
        {
            return (UInt16)myPlc.Read(DataType.DataBlock, DBAddress, 232, VarType.Word, 1);
        }
        public UInt16 XErrorInfo()
        {
            return (UInt16)myPlc.Read(DataType.DataBlock, DBAddress, 234, VarType.Word, 1);
        }

        /// <summary>
        /// X轴报警清除完成
        /// </summary>
        public bool IsXAlarmResetState()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, XAlarmResetedAddress, VarType.Int, 1) != 0;
        }

        /// <summary>
        /// X轴上电使能中
        /// </summary>
        public bool IsXEnabled()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, XEnabledAddress, VarType.Int, 1) != 0;
        }

        /// <summary>
        /// X轴零点校准完成
        /// </summary>
        public bool IsXReferencePointCalibrated()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, XReferencePointCalibratedAddress, VarType.Int, 1) != 0;
        }

        /// <summary>
        /// 获取X轴当前位置
        /// </summary>
        public float GetXCurrentPosition()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XCurrentPositionAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取X轴当前速度
        /// </summary>
        public float GetXCurrentSpeed()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XCurrentSpeedAddress, VarType.Real, 1);
        }

        /// <summary>
        /// X轴暂停中
        /// </summary>
        public bool IsXPaused()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, XPausedAddress, VarType.Int, 1) != 0;
        }

        /// <summary>
        /// X轴就绪
        /// </summary>
        public bool IsXReady()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, XReadyAddress, VarType.Int, 1) != 0;
        }

        /// <summary>
        /// 获取X轴当前加速度
        /// </summary>
        public float GetXCurrentAcceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XCurrentAccelerationAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取X轴当前减速度
        /// </summary>
        public float GetXCurrentDeceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XCurrentDecelerationAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取X轴位置设置最大值
        /// </summary>
        public float GetXMaxPosition()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XMaxPositionAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取X轴位置设置最小值
        /// </summary>
        public float GetXMinPosition()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XMinPositionAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取X轴速度设置最大值
        /// </summary>
        public float GetXMaxSpeed()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XMaxSpeedAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取X轴速度设置最小值
        /// </summary>
        public float GetXMinSpeed()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XMinSpeedAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取X轴加速度设置最大值
        /// </summary>
        public float GetXMaxAcceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XMaxAccelerationAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取X轴加速度设置最小值
        /// </summary>
        public float GetXMinAcceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XMinAccelerationAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取X轴减速度设置最大值
        /// </summary>
        public float GetXMaxDeceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XMaxDecelerationAddress, VarType.Real, 1);
        }

        /// <summary>
        /// X轴减速度设置最小值
        /// </summary>
        public float GetXMinDeceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, XMinDecelerationAddress, VarType.Real, 1);
        }

        #endregion

        #region Y轴设定与控制


        /// <summary>
        /// X轴清除所有命令
        /// </summary>
        public void YClearAllCommands()
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YJogPAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, YJogNAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, YStartPositioningAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, YRestartAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, YStopAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, YPauseAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, YReferencePointCalibratedAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, YStartToHomeAddress, (Int16)0);
            myPlc.Write(DataType.DataBlock, DBAddress, YAlarmResetedAddress, (Int16)0);
        }

        /// <summary>
        /// Y轴电机使能
        /// </summary>
        public void YEnable(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YEnableAddress, value.ToInt16());
        }

        ///// <summary>
        ///// Y轴电机不使能
        ///// </summary>
        //public void YDisable()
        //{
        //    myPlc.Write(DataType.DataBlock, DBAddress, YEnableAddress, 0);
        //}

        /// <summary>
        /// Y轴正向启动地址
        /// </summary>
        public void YJogP(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YJogPAddress, value.ToInt16());
        }

        /// <summary>
        /// Y轴负向启动地址
        /// </summary>
        public void YJogN(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YJogNAddress, value.ToInt16());
        }

        /// <summary>
        /// Y轴启动定位地址
        /// </summary>
        public void YStart(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YStartPositioningAddress, value.ToInt16());
        }

        /// <summary>
        /// Y轴回原点启动
        /// </summary>
        public void YStartToHome(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YStartToHomeAddress, value.ToInt16());
        }

        /// <summary>
        /// Y轴启动零点校准
        /// </summary>
        public void YStartReferencePointCalibrating(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YStartCalibratingReferencePointAddress, value.ToInt16());
        }

        ///// <summary>
        ///// X轴清除运动命令
        ///// </summary>
        //public void YClearMovingCmds()
        //{
        //    myPlc.Write(DataType.DataBlock, DBAddress, YJogPAddress, (Int16)0);
        //    myPlc.Write(DataType.DataBlock, DBAddress, YJogNAddress, (Int16)0);
        //    myPlc.Write(DataType.DataBlock, DBAddress, YStartPositioningAddress, (Int16)0);
        //    myPlc.Write(DataType.DataBlock, DBAddress, YRestartAddress, (Int16)0);
        //}

        /// <summary>
        /// Y轴设定定位位置
        /// </summary>
        public void YSetPosition(float position)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YSetPositionAddress, position);

        }
        /// <summary>
        /// 获取Y轴的设定位置
        /// </summary>
        /// <returns></returns>
        public float GetYSetPosition()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YSetPositionAddress, VarType.Real, 1);
        }
        /// <summary>
        /// 获取Y轴的设定速度
        /// </summary>
        /// <returns></returns>
        public float GetYSetSpeed()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YSetSpeedAddress, VarType.Real, 1);
        }

        /// <summary>
        /// Y轴设定定位速度
        /// </summary>
        public void YSetSpeed(float speed)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YSetSpeedAddress, speed);
        }

        /// <summary>
        /// Y轴报警清除
        /// </summary>
        public void YResetAlarm(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YResetAlarmAddress, value.ToInt16());
        }

        /// <summary>
        /// Y轴重新启动
        /// </summary>
        public void YRestart(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YRestartAddress, value.ToInt16());
        }

        /// <summary>
        /// Y轴设定Jog速度
        /// </summary>
        public void YSetJogSpeed(float jogSpeed)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YSetJogSpeedAddress, jogSpeed);
        }

        /// <summary>
        /// Y轴暂停
        /// </summary>
        public void YPause(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YPauseAddress, value.ToInt16());
        }

        /// <summary>
        /// Y轴停止
        /// </summary>
        public void YStop(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YStopAddress, value.ToInt16());
        }

        /// <summary>
        /// Y轴加速度设置
        /// </summary>
        public void YSetAcceleration(float acceleration)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YSetAccelerationAddress, acceleration);
        }

        /// <summary>
        /// Y轴减速度设置
        /// </summary>
        public void YSetDcceleration(float deceleration)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, YSetDecelerationAddress, deceleration);
        }

        #endregion

        #region Y轴状态
        public bool IsYError()
        {
            return (bool)myPlc.Read(DataType.DataBlock, DBAddress, 236, VarType.Byte, 1, 0);
        }
        public UInt16 YErrorID()
        {
            return (UInt16)myPlc.Read(DataType.DataBlock, DBAddress, 238, VarType.Word, 1);
        }
        public UInt16 YErrorInfo()
        {
            return (UInt16)myPlc.Read(DataType.DataBlock, DBAddress, 240, VarType.Word, 1);
        }
        /// <summary>
        /// Y轴报警清除完成
        /// </summary>
        public bool IsYAlarmResetState()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, YAlarmResetedAddress, VarType.Int, 1) != 0;
        }

        /// <summary>
        /// Y轴上电使能中
        /// </summary>
        public bool IsYEnabled()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, YEnabledAddress, VarType.Int, 1) != 0;
        }

        /// <summary>
        /// Y轴零点校准完成
        /// </summary>
        public bool IsYReferencePointCalibrated()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, YReferencePointCalibratedAddress, VarType.Int, 1) != 0;
        }

        /// <summary>
        /// 获取Y轴当前位置
        /// </summary>
        public float GetYCurrentPosition()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YCurrentPositionAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取Y轴当前速度
        /// </summary>
        public float GetYCurrentSpeed()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YCurrentSpeedAddress, VarType.Real, 1);
        }

        /// <summary>
        /// Y轴暂停中
        /// </summary>
        public bool IsYPaused()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, YPausedAddress, VarType.Int, 1) != 0;
        }

        /// <summary>
        /// Y轴就绪
        /// </summary>
        public bool IsYReady()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, YReadyAddress, VarType.Int, 1) != 0;
        }

        /// <summary>
        /// 获取Y轴当前加速度
        /// </summary>
        public float GetYCurrentAcceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YCurrentAccelerationAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取Y轴当前减速度
        /// </summary>
        public float GetYCurrentDeceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YCurrentDecelerationAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取Y轴位置设置最大值
        /// </summary>
        public float GetYMaxPosition()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YMaxPositionAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取Y轴位置设置最小值
        /// </summary>
        public float GetYMinPosition()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YMinPositionAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取Y轴速度设置最大值
        /// </summary>
        public float GetYMaxSpeed()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YMaxSpeedAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取Y轴速度设置最小值
        /// </summary>
        public float GetYMinSpeed()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YMinSpeedAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取Y轴加速度设置最大值
        /// </summary>
        public float GetYMaxAcceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YMaxAccelerationAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取Y轴加速度设置最小值
        /// </summary>
        public float GetYMinAcceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YMinAccelerationAddress, VarType.Real, 1);
        }

        /// <summary>
        /// 获取Y轴减速度设置最大值
        /// </summary>
        public float GetYMaxDeceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YMaxDecelerationAddress, VarType.Real, 1);
        }

        /// <summary>
        /// Y轴减速度设置最小值
        /// </summary>
        public float GetYMinDeceleration()
        {
            return (float)myPlc.Read(DataType.DataBlock, DBAddress, YMinDecelerationAddress, VarType.Real, 1);
        }

        #endregion

        #region 切割阀控制

        /// <summary>
        /// 点火氧阀开
        /// </summary>
        public void IgnitionOxiginValveOn(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, IgnitionOxiginValve, value.ToInt16());
        }


        /// <summary>
        /// 燃气阀开
        /// </summary>
        public void GasValveOn(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, GasValve, value.ToInt16());
        }

        public bool IsGasValveOn()
        {
            return (bool)myPlc.Read("Q2.0");
        }

        public bool IsCuttingOxyginValveOn()
        {
            return (bool)myPlc.Read("Q2.1");
        }
        public bool IsPreheatOxyginValveOn()
        {
            return (bool)myPlc.Read("Q0.7");
        }

        public bool IsIgnitionValveOn()
        {
            return (bool)myPlc.Read("Q1.1");
        }

        public bool IsIgnitionDeviceGasValveOn()
        {
            return (bool)myPlc.Read("Q2.2");
        }

        public bool IsCameraBoardOpen()
        {
            return (bool)myPlc.Read("Q1.0");
        }

        /// <summary>
        /// 点火燃气阀开
        /// </summary>
        public void IgnitionGasValveOn(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, IgnitionGasValve, value.ToInt16());
        }

        /// <summary>
        /// 切割氧阀开
        /// </summary>
        public void CuttingOxiginValveOn(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, CuttingOxiginValve, value.ToInt16());
        }

        /// <summary>
        /// 点火装置燃气阀开
        /// </summary>
        public void IgnitionDeviceGasValveOn(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, IgnitionDeviceGasValve, value.ToInt16());
        }


        /// <summary>
        /// 预热氧阀开
        /// </summary>
        public void PreHeatingOxiginValveOn(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, PreHeatingOxiginValve, value.ToInt16());
        }

        /// <summary>
        /// 点火阀开
        /// </summary>
        public void IgnitionValveOn(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, IgnitionValve, value.ToInt16());
        }

        #endregion

        #region 其他
        /// <summary>
        /// 相机挡板打开
        /// </summary>
        public void CameraBoardOpen(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, CameraBoardValveAddress, value.ToInt16());
        }

        /// <summary>
        /// 设置机器人与桁架联动标志
        /// </summary>
        /// <param name="value"></param>
        public void SetLinkeageMoveState(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, LinkeageMoveStateAddress, value.ToInt16());
        }
        public bool GetLinkeageMoveState()
        {
            return (Int16)myPlc.Read(DataType.DataBlock, DBAddress, LinkeageMoveStateAddress, VarType.Int, 1) != 0;
        }
        /// <summary>
        /// 设置与机器人联动心跳信号
        /// </summary>
        /// <param name="value"></param>
        public void SetHeartBeatForLinkeageMove(Boolean value)
        {
            myPlc.Write(DataType.DataBlock, DBAddress, HeartBeatForLinkeageMove, value.ToInt16());
        }
        public bool GetWorkMode()
        {
            var b = (Boolean)myPlc.Read("DB19.DBX0.0");
            return b;
        }

        public void SetWorkModeToLocal()
        {
            myPlc.Write("DB19.DBX0.0", false);
        }

        public void SetWorkModeToRemote()
        {
            myPlc.Write("DB19.DBX0.0", true);
        }

        /// <summary>
        /// 心跳
        /// </summary>
        public void HeartBeats()
        {
            myPlc.Write(DataType.DataBlock, DBAddress, HeartBeatAddress, (Int16)1);
        }
        #endregion

        public void MoveTruss(float xPostion, float yPostion, float xSpeed, float ySpeed)
        {
            //XEnable(true);
            //YEnable(true);
            //int count = 0;
            //while (!IsXEnabled() || !IsYEnabled())
            //{
            //    Thread.Sleep(100);
            //    count++;
            //    if (count >= 20)
            //    {
            //        MessageBox.Show("桁架无法使能！");
            //    }
            //}
            XSetPosition(xPostion);
            YSetPosition(yPostion);
            XSetSpeed(xSpeed);
            YSetSpeed(ySpeed);
            XStart(true);
            YStart(true);
        }

        // public TrussPoint GetTrussPosition()
        // {
        //     return new TrussPoint(GetXCurrentPosition(), GetYCurrentPosition(),0);
        // }

        // public bool IsTrussMovingCompleted(float xTargetPosition, float yTargetPosition, float positionTolorance, float speedTolorance)
        // {
        //     bool xP = Math.Abs(GetXCurrentPosition() - xTargetPosition) < positionTolorance;
        //     bool yP = Math.Abs(GetYCurrentPosition() - yTargetPosition) < positionTolorance;
        //     bool xS = Math.Abs(GetXCurrentSpeed()) < speedTolorance;
        //     bool yS = Math.Abs(GetYCurrentSpeed()) < speedTolorance;
        //     return xP && yP && xS && yS;
        // }

        public UInt16 LaserDistance()
        {
            return (UInt16)myPlc.Read(DataType.DataBlock, 36, 20, VarType.Word, 1);
        }

        #region 升降装置
        public void GunMoveUp(bool value)
        {
            myPlc.Write("M10.1", value);
        }
        public void GunMoveDown(bool value)
        {
            myPlc.Write("M10.2",value);
        }
        public void GunClearFault(bool value)
        {
            myPlc.Write("M10.3", value);
        }
        public void GunStop(bool value)
        {
            myPlc.Write("M10.4", value);
        }
        public Int32 GunGetCurrentPos()
        {
            return (Int32)(myPlc.Read(DataType.DataBlock,34,18,VarType.DInt,1));
        }
        #endregion
        
        public void TemporarilyHandling(bool value)
        {
            Int16 temp = value ? (Int16)1 : (Int16)0;
            myPlc.Write(DataType.DataBlock, 23, 254, temp);
        }
        
    }
}