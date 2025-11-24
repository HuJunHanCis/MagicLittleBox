using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using ABB.Robotics.Controllers.RapidDomain;

using ABB.Robotics.Controllers.IOSystemDomain;
using System.Threading;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.MotionDomain;
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers.EventLogDomain;
using OtherHelper;
using static OtherHelper.AaMath;
using static OtherHelper.AaAbbSolver;

namespace RobotHelper
{
    public class ArRobotHelper
    {
        public Controller controller = null;
        private ABB.Robotics.Controllers.RapidDomain.Task[] tasks = null;
        private RapidData nCurrentPointIndex;
        public Boolean Online { get; set; }
        
        public delegate void NumChanged(string variableName, Num num);
        public event NumChanged OnNumChanged;
        
        public bool? TryGetOneDigitalSignal(string signalName)
        {
            try { return ((DigitalSignal)controller.IOSystem.GetSignal(signalName)).Get() == 1; }
            catch { return null; }
        }

        public bool IsCycleOn => TryGetOneDigitalSignal("DO2") == true;

        public ControllerState State
        {
            get
            {
                try
                {
                    return controller.State;
                }
                catch (Exception)
                {
                    return ControllerState.Unknown;
                }
            }
        }
        
        public bool IsVirtual
        {
            get => controller.IsVirtual;
        }
        
        public ControllerOperatingMode OperatingMode
        {
            get
            {
                try
                {
                    return controller.OperatingMode;
                }
                catch (Exception)
                {
                    return ControllerOperatingMode.NotApplicable;
                }
            }
        }
        
        public bool GetBool(string name)
        {
            try
            {
                //nCuttingPointsCount
                var rd = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData(name);
                Bool bool1 = (Bool)rd.Value;
                return bool1.Value;
            }
            catch
            {
                return false;
            }
        }
        
        public bool IsMovingCompleted
        {
            get
            {
                return GetBool("moveCompleted");
            }
        }
        
        private void rd_ValueChanged(object sender, DataValueChangedEventArgs e)
        {
            OnNumChanged?.Invoke("nCurrentPointIndex", (Num)nCurrentPointIndex.Value);
        }
        
        // 构建+析构
        public ArRobotHelper(ControllerInfo controllerInfo)
        {
            try
            {
                if (controllerInfo.Availability == Availability.Available)
                {
                    if (this.controller != null)
                    {
                        this.controller.Logoff();
                        this.controller.Dispose();
                        this.controller = null;
                    }
                    controller = Controller.Connect(controllerInfo, ConnectionType.Standalone);
                    controller.Logon(UserInfo.DefaultUser);
                    //controller.EventLog.MessageWritten += new EventHandler<MessageWrittenEventArgs>( Log_MessageWritten);
            
                    Online = true;
                    //数据Event
                    nCurrentPointIndex = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData("nCurrentPointIndex");
                    nCurrentPointIndex.ValueChanged += new EventHandler<DataValueChangedEventArgs>(rd_ValueChanged);
                    nCurrentPointIndex.Subscribe(rd_ValueChanged, EventPriority.High);
                }
                else
                {
                    Online = false;
                    Console.WriteLine("Robot is not online");
                }
            }
            catch (Exception e2)
            {
                // MessageBox.Show(e2.Message);
                Console.WriteLine(e2.Message);
            }
        }
        ~ArRobotHelper()
        {
            if (controller != null)
            {
                controller.Dispose();
            }
            if (nCurrentPointIndex != null)
            {
                nCurrentPointIndex.Dispose();
                nCurrentPointIndex = null;
            }
            controller = null;
        }
        
        
        public int LoadModuleToTask(string taskName, string rapidFileName)
        {
            var result = 0;
            try
            {
                if (OperatingMode != ControllerOperatingMode.Auto)
                {
                    // MessageBox.Show("必须在自动模式下才能装载模块！", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    result = -1;
                }
                var task = controller.Rapid.GetTask(taskName);
                var modulesLoaded = task.GetModules();
                using (var m = Mastership.Request(controller))
                {
                    foreach (var mod in modulesLoaded)
                    {
                        if (mod.Name != "BASE" && mod.Name != "user")
                        {
                            task.DeleteModule(mod.Name);
                        }
                    }
                    result = task.LoadModuleFromFile(rapidFileName, RapidLoadMode.Replace) ? 0 : -1;


                }
            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }
        
        public int SendJointTarget(string name, JointTarget jointTarget)
        {
            var result = 0;
            try
            {
                var rd = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData(name);
                using (Mastership.Request(controller))
                {
                    rd.Value = jointTarget;
                }
            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }
        public int SendJointTarget(string name, float j1, float j2, float j3, float j4, float j5, float j6)
        {
            var result = 0;
            try
            {
                var rd = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData(name);
                JointTarget target = new JointTarget();
                target.RobAx.Rax_1 = j1;
                target.RobAx.Rax_2 = j2;
                target.RobAx.Rax_3 = j3;
                target.RobAx.Rax_4 = j4;
                target.RobAx.Rax_5 = j5;
                target.RobAx.Rax_6 = j6;
                using (Mastership.Request(controller))
                {
                    rd.Value = target;
                }
            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }
        
        public int SendOnNum(string name, int number)
        {
            var result = 0;
            try
            {
                //nCuttingPointsCount
                var rd = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData(name);
                Num num1 = new Num();
                num1.Value = number;
                using (var m = Mastership.Request(controller))
                {
                    rd.Value = num1;
                }
            }
            catch (Exception e1)
            {
                //MessageBox.Show(e1.Message);
                result = -1;
            }
            return result;
        }
        public int SendBool(string name, bool flag)
        {
            var result = 0;
            try
            {
                //nCuttingPointsCount
                var rd = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData(name);
                Bool bool1 = new Bool();
                bool1.Value = flag;
                using (var m = Mastership.Request(controller))
                {
                    rd.Value = bool1;
                }
            }
            catch (Exception e1)
            {
                result = -1;
            }
            return result;
        }
        
        public void SetInstructionCode(AbbInstructionCode code)
        {
            SendOnNum("numInstructionCode", (Int32)code);
        }
        public int SetCurrentSpeed(float tSpeed, float rSpeed)
        {
            var result = 0;
            try
            {
                var rd = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData("numSpeedArray");
                Num num = new Num();
                using (var m = Mastership.Request(controller))
                {
                    num.Value = tSpeed;
                    rd.WriteItem(num, 0);
                    rd.WriteItem(num, 2);
                    num.Value = rSpeed;
                    rd.WriteItem(num, 1);
                    rd.WriteItem(num, 3);
                }
                SetInstructionCode(AbbInstructionCode.SetCurrentSpeed);
            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }
        public int SetOneCuttingSpeed(int index, float tSpeed, float rSpeed)
        {
            var result = 0;
            try
            {
                var rd = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData("numSpeedArray");
                Num num = new Num();
                using (var m = Mastership.Request(controller))
                {
                    num.Value = tSpeed;
                    rd.WriteItem(num, 0);
                    rd.WriteItem(num, 2);
                    num.Value = rSpeed;
                    rd.WriteItem(num, 1);
                    rd.WriteItem(num, 3);
                }
                SendOnNum("numSpeedIndex", index + 1);
                SetInstructionCode(AbbInstructionCode.SetOneCuttingSpeed);
            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }
        
        public JointTarget GetCurrentPosition()
        {
            JointTarget position = controller.MotionSystem.ActiveMechanicalUnit.GetPosition();
            return position;
        }
        public RobTarget GetRobTarget(CoordinateSystemType coordinateSystemType)
        {
            RobTarget CurrPos = controller.MotionSystem.ActiveMechanicalUnit.GetPosition(coordinateSystemType);

            return CurrPos;
        }
        
        public int MotorsOn()
        {
            var result = 0;
            //if(State!= ControllerState.MotorsOn)
            {
                try
                {
                    using (Mastership.Request(controller))
                    {
                        controller.State = ControllerState.MotorsOn;
                    }
                }
                catch (Exception)
                {
                    result = -1;
                }
            }
            return result;
        }
        public int MotorsOff()
        {
            var result = 0;
            if (State == ControllerState.MotorsOn)
            {
                try
                {
                    using (Mastership.Request(controller))
                    {
                        controller.State = ControllerState.MotorsOff;
                    }
                }
                catch (Exception e1)
                {
                    result = -1;
                }
            }
            return result;
        }
        
        public int Restart()
        {
            int result = 0;
            try
            {
                if (controller.State == ControllerState.MotorsOn)
                {
                    // MessageBox.Show("运行状态不能重启");
                    result = -2;
                }
                using (Mastership m = Mastership.Request(controller))
                {
                    //tasks[0].ResetProgramPointer();
                    controller.Restart(ControllerStartMode.Warm);

                }
            }
            catch (Exception e2)
            {
                // MessageBox.Show(e2.Message);
                result = -1;
            }
            return result;
        }
        public int Start()
        {
            int result = 0;
            try
            {

                if (IsCycleOn)
                {
                    return 0;
                }
                if (controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    if (controller.State != ControllerState.MotorsOn)
                    {
                        // MessageBox.Show("机器人电机没有上电！");
                        return 0;
                    }
                    //tasks = controller.Rapid.GetTasks();
                    using (Mastership m = Mastership.Request(controller))
                    {
                        //tasks[0].ResetProgramPointer();
                        controller.Rapid.Start();
                    }
                }
                else
                {
                    // MessageBox.Show("ABB机器人需要在自动状态");
                    result = -3;
                }
            }
            catch (System.InvalidOperationException ex)
            {
                // MessageBox.Show("ABB机器人控制被其他进程所占用" + ex.Message);
                result = -4;
            }
            catch (System.Exception ex)
            {
                // MessageBox.Show("ABB机器人其他错误: " + ex.Message);
                result = -4;
            }
            return result;
        }
        public void Stop(StopMode stopMode = StopMode.Immediate)
        {
            try
            {

                if (!IsCycleOn)
                {
                    return;
                }
                if (controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    tasks = controller.Rapid.GetTasks();
                    using (Mastership m = Mastership.Request(controller))
                    {
                        controller.Rapid.Stop(stopMode);
                    }
                }
                else
                {
                    //MessageBox.Show(
                    //    "ABB机器人需要在自动状态");
                }
            }
            catch (System.InvalidOperationException ex)
            {
                //MessageBox.Show("ABB机器人控制被其他进程所占用" + ex.Message);
            }
            catch (System.Exception ex)
            {
                //MessageBox.Show("ABB机器人其他错误: " + ex.Message);
            }
        }
        
        public int ResetProgramPointer()
        {
            var result = 0;
            try
            {
                if (controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    if (controller.State != ControllerState.MotorsOn)
                    {
                        // MessageBox.Show("机器人没有上电！");
                        result = -2;
                    }
                    if (IsCycleOn)
                    {
                        Stop();
                        Thread.Sleep(200);
                    }
                    tasks = controller.Rapid.GetTasks();

                    using (Mastership m = Mastership.Request(controller))
                    {
                        for (int i = 0; i < tasks.Length; i++)
                        {
                            tasks[i].ResetProgramPointer();
                        }
                        //m.Release();
                    }
                }
                else
                {
                    // MessageBox.Show("机器人没在自动状态！");
                    result = -3; // 机器人没在自动状态
                }
            }
            catch (System.InvalidOperationException ex)
            {
                // MessageBox.Show("ABB机器人控制被其他进程所占用" + ex.Message);
                result = -4;
            }
            catch (System.Exception ex)
            {
                // MessageBox.Show("ABB机器人其他错误: " + ex.Message);
                result = -1;
            }
            return result;
        }
        
        public void ResetMovingCompletedFlag()
        {
            SendBool("moveCompleted", false);
        }
        
        public int EnsureCycleOn(int delayInSecond)
        {
            int result = 0;
            int count = 0;
            while (!IsCycleOn)
            {
                try
                {
                    MotorsOn();
                    ResetProgramPointer();
                    SetInstructionCode((AbbInstructionCode.DoNothing));
                    Start();
                }
                catch (Exception)
                {

                }
                Thread.Sleep(100);
                if (count++ > 10 * delayInSecond)
                {
                    result = -1;
                }
            }
            return result;
        }
        
        private int SendOneRobotTarget(string targetName, RobTarget robTarget)
        {
            var result = 0;
            try
            {
                var rd1 = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData(targetName);
                using (var m = Mastership.Request(controller))
                {
                    rd1.Value = robTarget;
                }

            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }
        
        public int SendToolData(string targetName, ToolData toolTata)
        {
            var result = 0;
            try
            {
                var rd1 = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData(targetName);
                using (Mastership.Request(controller))
                {
                    rd1.Value = toolTata;
                }
            }
            catch (Exception)
            {
                result = -1;
            }
            return result;
        }
        
        public void SendOneCartisianTarget(RobTarget robTarget)
        {
            SendOneRobotTarget("jposOneCartisianTarget", robTarget);
        }

        // public void SendOneCuttingPathPoint(RobotPointTrans robotPointTrans, float dX = 0, float dY = 0, float dZ = 0)
        // {
        //     RobTarget rt = new RobTarget();
        //     rt.Trans.X = robotPointTrans.Rx.ToFloat() + dX;
        //     rt.Trans.Y = robotPointTrans.Ry.ToFloat() + dY;
        //     rt.Trans.Z = robotPointTrans.Rz.ToFloat() + dZ;
        //     rt.Rot.Q1 = robotPointTrans.Q0;//pathData.Path[i].Q0.ToFloat();
        //     rt.Rot.Q2 = robotPointTrans.Qx; //pathData.Path[i].Qx.ToFloat();
        //     rt.Rot.Q3 = robotPointTrans.Qy;// pathData.Path[i].Qy.ToFloat();
        //     rt.Rot.Q4 = robotPointTrans.Qz;// pathData.Path[i].Qz.ToFloat();
        //     SendOneCartisianTarget(rt);
        // }
        public void SendOneCuttingPathPoint(RobotPointTrans robotPointTrans, float dX = 0, float dY = 0, float dZ = 0)
        {
            // 1) 取位姿
            var x = robotPointTrans.Rx.ToFloat() + dX;
            var y = robotPointTrans.Ry.ToFloat() + dY;
            var z = robotPointTrans.Rz.ToFloat() + dZ;

            // RobotPointTrans: Q0=qw(标量), Qx, Qy, Qz
            double qw = robotPointTrans.Q0;
            double qx = robotPointTrans.Qx;
            double qy = robotPointTrans.Qy;
            double qz = robotPointTrans.Qz;

            // 2) 归一化（强烈建议）
            double norm = Math.Sqrt(qw * qw + qx * qx + qy * qy + qz * qz);
            if (norm < 1e-9)
            {
                // 回退为单位四元数（无旋转）
                qw = 1.0; qx = 0.0; qy = 0.0; qz = 0.0;
            }
            else
            {
                qw /= norm; qx /= norm; qy /= norm; qz /= norm;
            }

            // 3) 组装 RobTarget（ABB 顺序：Q1=qx, Q2=qy, Q3=qz, Q4=qw）
            RobTarget rt = new RobTarget();
            rt.Trans.X = x;
            rt.Trans.Y = y;
            rt.Trans.Z = z;
            rt.Rot.Q1 = (float)qx;  // x
            rt.Rot.Q2 = (float)qy;  // y
            rt.Rot.Q3 = (float)qz;  // z
            rt.Rot.Q4 = (float)qw;  // w

            // 可选：配置/外轴按需填充（否则保持默认）
            // rt.RobConf.cf1 = 0; ...

            // 4) 下发
            SendOneCartisianTarget(rt);
        }

        
        public ToolData GetToolData()
        {
            var rapidData = controller.Rapid.GetTask("T_ROB1").GetModule("MainModule").GetRapidData("toolCuttingGunTest");
            if (rapidData.Value is ToolData)
            {
                return (ToolData)rapidData.Value;
            }
            else
            {
                return new ToolData();
            }
        }
        
        public void SetDi (string diName,bool value)
        {
            var sig = controller.IOSystem.GetSignal(diName);
            DigitalSignal dg = (DigitalSignal)sig;
            if(value) { dg.Value = 1; }
            else { dg.Value = 0; }
     
        }
        
        public void SetDo(string doName, bool value)
        {
            try
            {
                var sig = controller.IOSystem.GetSignal(doName);

                // 写 DO 必须是 DigitalOutputSignal
                if (sig is DigitalSignal dg)
                {
                    dg.Value = value ? 1 : 0;
                }
                else
                {
                    throw new Exception($"[IO] {doName} 不是数字输出信号（DO），无法写入。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写 DO[{doName}] 失败: {ex.Message}");
            }
        }


    }
}
