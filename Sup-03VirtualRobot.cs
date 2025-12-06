// Modified by Hujunhan.
// Log Code [213].
// 100% Correct by deepseek check.

using System;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

using OtherHelper;
using RobotHelper;

using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.MotionDomain;
using ABB.Robotics.Controllers.RapidDomain;

namespace MagicLittleBox
{
    public sealed class SupVirtualRobot : IDisposable
    {
        // 01 : 清理逻辑
        public void Dispose()
        {
            try
            {
                if (_arRobotVirtual != null)
                {
                    _arRobotVirtual = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Log.Information("[213]: 已释放仿真机器人实例");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 释放仿真机器人资源异常");
            }
        }
        
        // 02 : 初始化前的准备工作
        private ArRobotHelper _arRobotVirtual;
        private CancellationTokenSource _arRobotVirtualCts;
        public static SupVirtualRobot Instance { get; } = new SupVirtualRobot();
        private SupVirtualRobot()
        {
            Log.Information("[213]: 创建仿真机器人实例成功");
        }
        
        // 03 : 初始化
        public bool Init()
        {
            try
            {
                if (_arRobotVirtual != null && _arRobotVirtual.Online)
                {
                    Log.Information("[213]: 仿真机器人已存在，跳过初始化");
                    return true;
                }
                NetworkScanner scanner = new NetworkScanner();
                scanner.Scan();
                var controllers = scanner.Controllers;
                if (controllers == null || controllers.Count == 0)
                {
                    Log.Information("[213]: 未能扫描到任何仿真机器人控制器");
                    return false;
                }
                int index = 0;
                while (index < controllers.Count)
                {
                    ControllerInfo controller =  controllers[index];
                    _arRobotVirtual = new ArRobotHelper(controller);
                    if (_arRobotVirtual.IsVirtual)
                    // if (_arRobotVirtual.IsVirtual)
                    {
                        Log.Information("[213]: 仿真机器人初始化成功");
                        _arRobotVirtual.MotorsOn();
                        _arRobotVirtual.ResetProgramPointer();
                        _arRobotVirtual.Start();
                        return true;
                    }
                    _arRobotVirtual = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(100);
                    index++;
                }
                Log.Information("[213]: 未能在当前网段内寻找到仿真机器人");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 初始化仿真机器人过程中出现异常");
                return false;
            }
        }
        
        // 04 : 重启
        public bool Restart()
        {
            try
            {
                if (_arRobotVirtual != null && _arRobotVirtual.Online)
                {
                    Log.Information("[213]: 强制仿真机器人下电重启");
                    _arRobotVirtual.MotorsOff();
                    Thread.Sleep(100);
                    _arRobotVirtual.Restart();
                    Thread.Sleep(100);
                    // if (_arRobotVirtual.Online && !_arRobotVirtual.IsVirtual)
                    if (_arRobotVirtual.Online && _arRobotVirtual.IsVirtual)
                    {
                        Log.Information("[213]: 仿真机器人重启成功");
                        return true;
                    }
                    Log.Warning("[213]: 仿真机器人重启后状态异常");
                    return false;
                }
                Log.Information("[213]: 未检测到仿真机器人实例，调用初始化函数来重启");
                return Init();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 重启仿真机器人时发生异常");
                return false;
            }
        }
        
        // 04 : 重启（简化版 - 不真正重启控制器）
        public bool RestartSim()
        {
            try
            {
                Log.Information("[213]: 执行仿真机器人软重启（不重启控制器）");
        
                // 只是重置程序指针和状态，不重启控制器
                // _arRobotVirtual.MotorsOff();
                // Thread.Sleep(100);
                _arRobotVirtual.ResetProgramPointer();
                _arRobotVirtual.MotorsOn();
                _arRobotVirtual.Start();
        
                Log.Information("[213]: 仿真机器人软重启成功");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 重启仿真机器人时发生异常");
                return Init(); // 如果软重启失败，尝试重新初始化
            }
        }
        
        // 05 : 电机使能
        public bool MotorOn()
        {
            try
            {
                if (_arRobotVirtual == null || !_arRobotVirtual.Online)
                {
                    Log.Warning("[213]: 电机上电失败，仿真机器人不在线");
                    return false;
                }
                _arRobotVirtual.ResetProgramPointer();
                _arRobotVirtual.MotorsOn();
                Log.Information("[213]: 仿真机器人重置指针并上电完毕");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 仿真机器人使能失败");
                return false;
            }
        }

        // 06 : 急停
        public void EmergyStop()
        {
            try
            {
                _arRobotVirtualCts?.Cancel();
                _arRobotVirtual.MotorsOff();
                _arRobotVirtual.Stop();
                _arRobotVirtual.SetInstructionCode(AbbInstructionCode.DoNothing);
                Log.Information("[213]: 执行仿真机器人急停");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 仿真机器人急停时异常");
            }
        }
        
        // 07 : 暂停
        public void Pause()
        {
            try
            {
                _arRobotVirtual.Stop();
                Log.Information("[213]: 仿真机器人暂停动作");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 仿真机器人暂停时发生异常");
            }
        }
        
        // 08 : 恢复
        public void Resume()
        {
            try
            {
                _arRobotVirtual.Start();
                Log.Information("[213]: 仿真机器人恢复动作");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 仿真机器人恢复时发生异常");
            }
        }
        
        // 09 : 获取机器人控制器状态
        public int  GetVirRobotStatus()
        {
            try
            {
                if (_arRobotVirtual==null)
                {
                    return -1;
                }
                if (!_arRobotVirtual.Online)
                {
                    return 1;
                }
                if(_arRobotVirtual.State == ControllerState.GuardStop)
                {
                    return 2;
                }
                if (_arRobotVirtual.State == ControllerState.MotorsOff)
                {
                    return 3;
                }
                if (_arRobotVirtual.State == ControllerState.MotorsOn)
                {
                    return 4;
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }
        
        // 09 : 获取机器人控制器状态
        public int GetVirRobotStatus_Full()
        {
            try
            {
                if (_arRobotVirtual == null || !_arRobotVirtual.Online)
                {
                    // 未连接 / 离线
                    return -1;
                }

                switch (_arRobotVirtual.State)
                {
                    case ControllerState.Init:
                        // 初始化
                        return 0;

                    case ControllerState.MotorsOff:
                        // 电机下电 / 待机
                        return 1;

                    case ControllerState.MotorsOn:
                        // 电机上电 / 正常运行
                        return 3;

                    case ControllerState.GuardStop:
                        // 保护停止（安全栅栏/光栅等触发）
                        return 4;

                    case ControllerState.EmergencyStop:
                        // 急停
                        return 5;

                    case ControllerState.EmergencyStopReset:
                        // 急停已复位，尚未完全恢复
                        return 6;

                    case ControllerState.SystemFailure:
                        // 系统级故障
                        return 7;

                    case ControllerState.Unknown:
                        // 未知状态
                        return 99;

                    default:
                        return -1;
                }
            }
            catch
            {
                // 异常时统一按 -1 处理
                return -1;
            }
        }
        
        // 10 : 获取机器人坐标和姿态，该函数若报错不能log，不然会输出很多！！
        public (float X, float Y, float Z, 
            float J1, float J2, float J3, 
            float J4, float J5, float J6) GetRobotPose()
        {
            try
            {
                if (_arRobotVirtual != null && _arRobotVirtual.Online)
                {
                    var target = _arRobotVirtual.GetRobTarget(CoordinateSystemType.WorkObject);
                    var pos = _arRobotVirtual.GetCurrentPosition();
                    
                    float x = (float)Math.Round(target.Trans.X, 2);
                    float y = (float)Math.Round(target.Trans.Y, 2);
                    float z = (float)Math.Round(target.Trans.Z, 2);
                    float j1 = (float)Math.Round(pos.RobAx.Rax_1, 2);
                    float j2 = (float)Math.Round(pos.RobAx.Rax_2, 2);
                    float j3 = (float)Math.Round(pos.RobAx.Rax_3, 2);
                    float j4 = (float)Math.Round(pos.RobAx.Rax_4, 2);
                    float j5 = (float)Math.Round(pos.RobAx.Rax_5, 2);
                    float j6 = (float)Math.Round(pos.RobAx.Rax_6, 2);

                    return (
                        x, y, z, j1, j2, j3, j4, j5, j6
                    );
                }
                return (
                    0000.00f, 0000.00f, 0000.00f, 
                    000.00f, 000.00f, 000.00f, 
                    000.00f, 000.00f, 000.00f
                );
            }
            catch
            {
                return (
                    0000.00f, 0000.00f, 0000.00f, 
                    000.00f, 000.00f, 000.00f, 
                    000.00f, 000.00f, 000.00f
                );
            }
        }
        
        // 11 : 机器人位姿法移动
        public async Task<bool> VirRobotGoByPosAsync(
            float x, float y, float z, 
            float q1, float q2, float q3, float q4,
            float speedT = 150, float speedR = 120,
            bool force = true)
        {
            try
            {
                _arRobotVirtualCts?.Dispose();
                _arRobotVirtualCts = new CancellationTokenSource();
                var ct = _arRobotVirtualCts.Token;

                if (_arRobotVirtual == null || !_arRobotVirtual.Online)
                {
                    Log.Warning("[213]: 仿真机器人未连接，无法执行移动");
                    return false;
                }
                
                RobTarget robTarget = new RobTarget();
                robTarget.Trans.X = x;
                robTarget.Trans.Y = y;
                robTarget.Trans.Z = z;
                robTarget.Rot.Q1 = q1;
                robTarget.Rot.Q2 = q2;
                robTarget.Rot.Q3 = q3;
                robTarget.Rot.Q4 = q4;

                if (force)
                {
                    _arRobotVirtual.ResetMovingCompletedFlag();
                }
                
                _arRobotVirtual.SetCurrentSpeed(speedT, speedR);
                _arRobotVirtual.SendOneCartisianTarget(robTarget);
                _arRobotVirtual.SetInstructionCode(AbbInstructionCode.MoveOneCartisian);

                while (!_arRobotVirtual.IsMovingCompleted)
                {
                    ct.ThrowIfCancellationRequested();
                    await System.Threading.Tasks.Task.Delay(200, ct).ConfigureAwait(false);
                }
                Log.Information($"[213]: 机器人移动至坐标:[{x},{y},{z} : {q1},{q2},{q3},{q4}]");
                return true;
            }
            catch (OperationCanceledException oe)
            {
                Log.Warning(oe, "[213]: 移动被取消/暂停");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[213]: 机器人移动至坐标:[{x},{y},{z} : {q1},{q2},{q3},{q4}]故障");
                return false;
            }
            finally
            {
                if (_arRobotVirtualCts != null)
                {
                    _arRobotVirtualCts.Dispose();
                    _arRobotVirtualCts = null;
                }
            }
        }

        // 12 : 机器人六轴法移动
        public async Task<bool> VirRobotGoBySixAsync(
            float j1, float j2, float j3, 
            float j4, float j5, float j6,
            float speedT = 150, float speedR = 120,
            bool force = true)
        {
            try
            {
                _arRobotVirtualCts?.Dispose();
                _arRobotVirtualCts = new CancellationTokenSource();
                var ct = _arRobotVirtualCts.Token;

                if (_arRobotVirtual == null || !_arRobotVirtual.Online)
                {
                    Log.Warning("[213]: 仿真机器人未连接，无法执行移动");
                    return false;
                }
                
                JointTarget jointTarget = new JointTarget();
                jointTarget.RobAx.Rax_1 = j1;
                jointTarget.RobAx.Rax_2 = j2;
                jointTarget.RobAx.Rax_3 = j3;
                jointTarget.RobAx.Rax_4 = j4;
                jointTarget.RobAx.Rax_5 = j5;
                jointTarget.RobAx.Rax_6 = j6;
                
                if (force)
                {
                    _arRobotVirtual.ResetMovingCompletedFlag();
                }
                
                _arRobotVirtual.SetCurrentSpeed(speedT, speedR);
                _arRobotVirtual.SendJointTarget("jposOneJointTarget", jointTarget);
                _arRobotVirtual.SetInstructionCode(AbbInstructionCode.MoveOneJoint);

                while (!_arRobotVirtual.IsMovingCompleted)
                {
                    ct.ThrowIfCancellationRequested();
                    await System.Threading.Tasks.Task.Delay(200, ct).ConfigureAwait(false);
                }
                Log.Error($"[213]: 机器人移动至位置:[{j1},{j2},{j3},{j4},{j5},{j6}]");
                return true;
            }
            catch (OperationCanceledException oe)
            {
                Log.Warning(oe, "[213]: 移动被取消/暂停");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[213]: 机器人移动至位置:[{j1},{j2},{j3},{j4},{j5},{j6}]故障");
                return false;
            }
            finally
            {
                if (_arRobotVirtualCts != null)
                {
                    _arRobotVirtualCts.Dispose();
                    _arRobotVirtualCts = null;
                }
            }
        }

        // 13 : 机器人刷新
        public bool Refresh()
        {
            try
            {
                if (_arRobotVirtual != null && _arRobotVirtual.Online)
                {
                    _arRobotVirtual.ResetMovingCompletedFlag();
                    _arRobotVirtual.ResetProgramPointer();
                    _arRobotVirtual.MotorsOn();
                    Log.Information("[213]: 机器人刷新，重置指针并上电");
                    return true;
                }
                Log.Information("[213]: 未检测到仿真机器人实例，调用初始化函数来刷新");
                return Init();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[213]: 仿真机器人刷新时发生异常");
                return false;
            }
        }
        
        // 14 : 获取瞬间姿态
        public (float X, float Y, float Z, 
            float Q1, float Q2, float Q3, float Q4,
            float J1, float J2, float J3, 
            float J4, float J5, float J6) GetCurrentPose()
        {
            try
            {
                if (_arRobotVirtual != null && _arRobotVirtual.Online)
                {
                    var target = _arRobotVirtual.GetRobTarget(CoordinateSystemType.WorkObject);
                    var pos = _arRobotVirtual.GetCurrentPosition();

                    // 直接用 Math.Round 再强转 float，避免区域性格式化问题
                    float x = (float)Math.Round(target.Trans.X, 2);
                    float y = (float)Math.Round(target.Trans.Y, 2);
                    float z = (float)Math.Round(target.Trans.Z, 2);
                    float q1 = (float)Math.Round(target.Rot.Q1, 2);
                    float q2 = (float)Math.Round(target.Rot.Q2, 2);
                    float q3 = (float)Math.Round(target.Rot.Q3, 2);
                    float q4 = (float)Math.Round(target.Rot.Q4, 2);
                    float j1 = (float)Math.Round(pos.RobAx.Rax_1, 2);
                    float j2 = (float)Math.Round(pos.RobAx.Rax_2, 2);
                    float j3 = (float)Math.Round(pos.RobAx.Rax_3, 2);
                    float j4 = (float)Math.Round(pos.RobAx.Rax_4, 2);
                    float j5 = (float)Math.Round(pos.RobAx.Rax_5, 2);
                    float j6 = (float)Math.Round(pos.RobAx.Rax_6, 2);
                    return (
                        x, y, z, q1, q2, q3, q4, j1, j2, j3, j4, j5, j6
                    );
                }
                return (
                    0.00f, 0.00f, 0.00f, 
                    0.00f, 0.00f, 0.00f, 0.00f, 
                    0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f
                );
            }
            catch
            {
                return (
                    0.00f, 0.00f, 0.00f, 
                    0.00f, 0.00f, 0.00f, 0.00f, 
                    0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f
                );
            }
        }
        
        // 15 : 发送坐标数据
        public void SendToolData(string targetName, ToolData toolTata)
        {
            try
            {
                _arRobotVirtual.SendToolData(targetName, toolTata);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 仿真机器人发送坐标系时异常");
            }
        }

        public void JieJunChou()
        {
            RestartSim();
            Thread.Sleep(5000);
            _arRobotVirtual.SetDo("DO1",true);
            Thread.Sleep(2000);
            _arRobotVirtual.SetDo("DO8",true);
            Thread.Sleep(2000);
            _arRobotVirtual.SetDo("DO9",true);
            Thread.Sleep(2000);
            _arRobotVirtual.SetDo("DO11",true);
        }
        
        public void NewRestart()
        {
            try
            {
                Log.Information("[213]: 强制仿真机器人下电重启");
                _arRobotVirtual.MotorsOff();
                Thread.Sleep(100);
                _arRobotVirtual.Restart();
                Thread.Sleep(100);
                if (_arRobotVirtual.Online && _arRobotVirtual.IsVirtual)
                {
                    Log.Information("[213]: 仿真机器人重启成功");
                }
                Thread.Sleep(100);
                _arRobotVirtual.ResetMovingCompletedFlag();
                _arRobotVirtual.ResetProgramPointer();
                _arRobotVirtual.MotorsOn();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 重启仿真机器人时发生异常");
            }
        }
        public bool NewInit()
        {
            try
            {
                _arRobotVirtual = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(100);
                
                NetworkScanner scanner = new NetworkScanner();
                scanner.Scan();
                var controllers = scanner.Controllers;
                if (controllers == null || controllers.Count == 0)
                {
                    Log.Information("[213]: 未能扫描到任何仿真机器人控制器");
                    return false;
                }
                int index = 0;
                while (index < controllers.Count)
                {
                    ControllerInfo controller =  controllers[index];
                    _arRobotVirtual = new ArRobotHelper(controller);
                    if (_arRobotVirtual.IsVirtual)
                    {
                        Log.Information("[213]: 仿真机器人初始化成功");
                        _arRobotVirtual.MotorsOn();
                        _arRobotVirtual.ResetProgramPointer();
                        _arRobotVirtual.Start();
                        return true;
                    }
                    _arRobotVirtual = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(100);
                    index++;
                }
                Log.Information("[213]: 未能在当前网段内寻找到仿真机器人");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[213]: 初始化仿真机器人过程中出现异常");
                return false;
            }
        }
        
        

    }
}