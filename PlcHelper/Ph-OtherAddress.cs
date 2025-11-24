namespace PlcHelper
{
    public partial class ApHelper
    {
        /// <summary>
        /// DB 地址
        /// </summary>
        readonly int DBAddress = 23;

        #region X轴电机控制地址
        /// <summary>
        /// X轴电机使能地址
        /// </summary>
        readonly int XEnableAddress = 16;

        /// <summary>
        /// X轴正向启动地址
        /// </summary>
        readonly int XJogPAddress = 18;

        /// <summary>
        /// X轴负向启动地址
        /// </summary>
        readonly int XJogNAddress = 20;

        /// <summary>
        /// X轴启动定位地址
        /// </summary>
        readonly int XStartPositioningAddress = 22;

        /// <summary>
        /// X轴回原点启动
        /// </summary>
        readonly int XStartToHomeAddress = 24;

        /// <summary>
        /// X轴启动零点校准
        /// </summary>
        readonly int XStartCalibratingReferencePointAddress = 26;

        /// <summary>
        /// X轴设定定位位置
        /// </summary>
        readonly int XSetPositionAddress = 28;

        /// <summary>
        /// X轴设定定位速度
        /// </summary>
        readonly int XSetSpeedAddress = 32;

        /// <summary>
        /// X轴报警清除
        /// </summary>
        readonly int XResetAlarmAddress = 36;

        /// <summary>
        /// X轴重新启动
        /// </summary>
        readonly int XRestartAddress = 38;

        /// <summary>
        /// X轴设定Jog速度
        /// </summary>
        readonly int XSetJogSpeedAddress = 40;

        /// <summary>
        /// X轴暂停
        /// </summary>
        readonly int XPauseAddress = 116;

        /// <summary>
        /// X轴停止
        /// </summary>
        readonly int XStopAddress = 120;

        /// <summary>
        /// X轴加速度设置
        /// </summary>
        readonly int XSetAccelerationAddress = 134;

        /// <summary>
        /// X轴减速度设置
        /// </summary>
        readonly int XSetDecelerationAddress = 138;

        #endregion

        #region X轴状态地址

        /// <summary>
        /// X轴报警清除完成
        /// </summary>
        readonly int XAlarmResetedAddress = 72;

        /// <summary>
        /// X轴上电使能中
        /// </summary>
        readonly int XEnabledAddress = 74;

        /// <summary>
        /// X轴零点校准完成
        /// </summary>
        readonly int XReferencePointCalibratedAddress = 76;

        /// <summary>
        /// X轴当前位置
        /// </summary>
        readonly int XCurrentPositionAddress = 78;

        /// <summary>
        /// X轴当前速度
        /// </summary>
        readonly int XCurrentSpeedAddress = 82;

        /// <summary>
        /// X轴暂停中
        /// </summary>
        readonly int XPausedAddress = 126;

        /// <summary>
        /// X轴就绪
        /// </summary>
        readonly int XReadyAddress = 130;

        /// <summary>
        /// X轴当前加速度
        /// </summary>
        readonly int XCurrentAccelerationAddress = 150;

        /// <summary>
        /// X轴当前减速度
        /// </summary>
        readonly int XCurrentDecelerationAddress = 154;

        /// <summary>
        /// X轴位置设置最大值
        /// </summary>
        readonly int XMaxPositionAddress = 166;

        /// <summary>
        /// X轴位置设置最小值
        /// </summary>
        readonly int XMinPositionAddress = 170;

        /// <summary>
        /// X轴速度设置最大值
        /// </summary>
        readonly int XMaxSpeedAddress = 182;

        /// <summary>
        /// X轴速度设置最小值
        /// </summary>
        readonly int XMinSpeedAddress = 186;

        /// <summary>
        /// X轴加速度设置最大值
        /// </summary>
        readonly int XMaxAccelerationAddress = 198;

        /// <summary>
        /// X轴加速度设置最小值
        /// </summary>
        readonly int XMinAccelerationAddress = 202;

        /// <summary>
        /// X轴减速度设置最大值
        /// </summary>
        readonly int XMaxDecelerationAddress = 206;

        /// <summary>
        /// X轴减速度设置最小值
        /// </summary>
        readonly int XMinDecelerationAddress = 210;

        #endregion

        #region Y轴电机控制地址
        /// <summary>
        /// Y轴电机使能地址
        /// </summary>
        readonly int YEnableAddress = 44;

        /// <summary>
        /// Y轴正向启动地址
        /// </summary>
        readonly int YJogPAddress = 46;

        /// <summary>
        /// Y轴负向启动地址
        /// </summary>
        readonly int YJogNAddress = 48;

        /// <summary>
        /// Y轴启动定位地址
        /// </summary>
        readonly int YStartPositioningAddress = 50;

        /// <summary>
        /// Y轴回原点启动
        /// </summary>
        readonly int YStartToHomeAddress = 52;

        /// <summary>
        /// X轴启动零点校准
        /// </summary>
        readonly int YStartCalibratingReferencePointAddress = 54;

        /// <summary>
        /// Y轴设定定位位置
        /// </summary>
        readonly int YSetPositionAddress = 56;

        /// <summary>
        /// Y轴设定定位速度
        /// </summary>
        readonly int YSetSpeedAddress = 60;

        /// <summary>
        /// Y轴报警清除
        /// </summary>
        readonly int YResetAlarmAddress = 64;

        /// <summary>
        /// Y轴重新启动
        /// </summary>
        readonly int YRestartAddress = 66;

        /// <summary>
        /// Y轴设定Jog速度
        /// </summary>
        readonly int YSetJogSpeedAddress = 68;

        /// <summary>
        /// Y轴停止
        /// </summary>
        readonly int YStopAddress = 122;

        /// <summary>
        /// Y轴暂停
        /// </summary>
        readonly int YPauseAddress = 118;

        /// <summary>
        /// Y轴加速度设置
        /// </summary>
        readonly int YSetAccelerationAddress = 142;
        /// <summary>
        /// Y轴减速度设置
        /// </summary>
        readonly int YSetDecelerationAddress = 146;

        #endregion

        #region Y轴状态地址

        /// <summary>
        /// Y轴当前位置
        /// </summary>
        readonly int YCurrentPositionAddress = 86;

        /// <summary>
        /// Y轴当前速度
        /// </summary>
        readonly int YCurrentSpeedAddress = 90;

        /// <summary>
        /// Y轴报警清除完成
        /// </summary>
        readonly int YAlarmResetedAddress = 94;

        /// <summary>
        /// Y轴上电使能中
        /// </summary>
        readonly int YEnabledAddress = 96;

        /// <summary>
        /// Y轴零点校准完成
        /// </summary>
        readonly int YReferencePointCalibratedAddress = 98;


        /// <summary>
        /// Y轴暂停中
        /// </summary>
        readonly int YPausedAddress = 128;

        /// <summary>
        /// Y轴就绪
        /// </summary>
        readonly int YReadyAddress = 132;

        /// <summary>
        /// Y轴当前加速度
        /// </summary>
        readonly int YCurrentAccelerationAddress = 158;

        /// <summary>
        /// Y轴当前减速度
        /// </summary>
        readonly int YCurrentDecelerationAddress = 162;

        /// <summary>
        /// Y轴位置设置最大值
        /// </summary>
        readonly int YMaxPositionAddress = 174;

        /// <summary>
        /// Y轴位置设置最小值
        /// </summary>
        readonly int YMinPositionAddress = 178;


        /// <summary>
        /// Y轴速度设置最大值
        /// </summary>
        readonly int YMaxSpeedAddress = 190;

        /// <summary>
        /// Y轴速度设置最小值
        /// </summary>
        readonly int YMinSpeedAddress = 194;

        /// <summary>
        /// Y轴加速度设置最大值
        /// </summary>
        readonly int YMaxAccelerationAddress = 214;

        /// <summary>
        /// Y轴加速度设置最小值
        /// </summary>
        readonly int YMinAccelerationAddress = 218;

        /// <summary>
        /// Y轴减速度设置最大值
        /// </summary>
        readonly int YMaxDecelerationAddress = 222;

        /// <summary>
        /// Y轴减速度设置最小值
        /// </summary>
        readonly int YMinDecelerationAddress = 226;

        #endregion

        #region 火焰系统
        /// <summary>
        /// 点火氧阀地址
        /// </summary>
        readonly int IgnitionOxiginValve = 100;
        /// <summary>
        /// 燃气阀地址
        /// </summary>
        readonly int GasValve = 102;

        /// <summary>
        /// 点火燃气
        /// </summary>
        readonly int IgnitionGasValve = 106;
        /// <summary>
        /// 切割氧阀
        /// </summary>
        readonly int CuttingOxiginValve = 108;
        /// <summary>
        /// 点火装置燃气阀
        /// </summary>
        readonly int IgnitionDeviceGasValve = 110;

        /// <summary>
        /// 预热氧阀
        /// </summary>
        readonly int PreHeatingOxiginValve = 112;

        /// <summary>
        /// 点火阀
        /// </summary>
        readonly int IgnitionValve = 114;
        #endregion

        #region 其他

        /// <summary>
        /// 心跳
        /// </summary>
        readonly int HeartBeatAddress = 124;

        /// <summary>
        /// 相机挡板阀
        /// </summary>
        readonly int CameraBoardValveAddress = 104;

        readonly int LinkeageMoveStateAddress = 242;

        readonly int HeartBeatForLinkeageMove = 244;

        #endregion
    }
}