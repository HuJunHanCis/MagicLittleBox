// Modified by Hujunhan.
// Log Code [214].
// 100% Correct by deepseek check.

using System;
using System.IO;
using System.Xml;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

using Serilog;

using OtherHelper;
using PlcHelper;

namespace MagicLittleBox
{
    public sealed class SupVirtualTruss : IDisposable
    {
        // 01 : 清理逻辑
        public void Dispose()
        {
            try
            {
                if (_apTrussVirtual != null)
                {
                    _apTrussVirtual.Disconnect();
                    _apTrussVirtual = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Log.Information("[214]: 已释放仿真控制器实例");
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[214]: 释放仿真控制器资源异常");
            }
        }
        
        // 02 : 初始化前的准备工作
        private ApHelper _apTrussVirtual;
        private CancellationTokenSource _apTrussVirtualCts;
        public static SupVirtualTruss Instance { get; } = new SupVirtualTruss();
        private SupVirtualTruss()
        {
            Log.Information("[214]: 创建仿真控制器实例成功");
        }
        
        // 03 : 初始化
        public bool Init()
        {
            try
            {
                if (_apTrussVirtual != null && _apTrussVirtual.Online)
                {
                    Log.Information("[214]: 仿真控制器已存在，跳过初始化");
                    return true;
                }
                
                string plcIp = PlcLoadParameters(_configFilePath);
                _apTrussVirtual?.Disconnect();
                Thread.Sleep(100);
                _apTrussVirtual = new ApHelper(plcIp);
                if (_apTrussVirtual.Online)
                {
                    HeartBeats();
                    SetToRemote();
                    ResetAlarm();
                    EnableBoth();
                    Log.Information("[214]: 仿真控制器初始化成功");
                    return true;
                }
                Log.Information("[214]: 未能在当前IP及端口连接到仿真控制器");
                return false;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[214]: 初始化仿真控制器过程中出现异常");
                return false;
            }
        }
        
        // 04 : 心跳
        public void HeartBeats()
        {
            try
            {
                _apTrussVirtual.HeartBeats();
                Log.Information("[214]: 仿真控制器赋予心跳");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[214]: 仿真控制器赋予心跳时异常");
            }
        }

        // 05 : 切远程
        private void SetToRemote()
        {
            try
            {
                _apTrussVirtual.SetWorkModeToRemote();
                Log.Information("[214]: 仿真控制器切换远程");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[214]: 仿真控制器切换远程时异常");
            }
        }
        
        // 06 : 仅清除警报
        public void ResetAlarm(bool debug=true)
        {
            try
            {
                _apTrussVirtual.XResetAlarm(true);
                _apTrussVirtual.YResetAlarm(true);
                if (debug)
                {
                    Log.Information("[214]: 清除双轴警报");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[214]: 清除双轴警报时发生异常");
            }
        }

        // 07 : 单独控制器状态
        public int GetVirPlcStatus()
        {
            try
            {
                if (_apTrussVirtual == null)
                {
                    return -1;
                }

                if (!_apTrussVirtual.Online)
                {
                    return 1;
                }

                if (!_apTrussVirtual.GetWorkMode())
                {
                    return 2;
                }

                if (_apTrussVirtual.GetWorkMode())
                {
                    // 在线，远程模式
                    return 3;
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }
        
        // 07 : 单独控制器状态
        public int GetVirPlcStatus_Full()
        {
            try
            {
                // 控制器对象不存在
                if (_apTrussVirtual == null)
                {
                    return -1;
                }

                // 控制器不在线
                if (!_apTrussVirtual.Online)
                {
                    return 1;
                }

                // Online 状态下，区分工作模式
                bool isRemoteMode = _apTrussVirtual.GetWorkMode();

                if (isRemoteMode)
                {
                    // 在线，远程调试 / 远程自动模式
                    return 3;
                }
                else
                {
                    // 在线，现场手动控制模式
                    return 2;
                }
            }
            catch
            {
                // 任何异常都视为异常状态
                return -1;
            }
        }
        
        // 08 : 单独X轴状态
        public int GetVirXTrussStatus()
        {
            try
            {
                if (_apTrussVirtual.IsXAlarmResetState())
                {
                    if (!_apTrussVirtual.IsXEnabled())
                    {
                        return 1;
                    }
                    if (_apTrussVirtual.IsXEnabled())
                    {
                        return 2;
                    }
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }
        
        // 09 : 单独X轴状态
        public int GetVirYTrussStatus()
        {
            try
            {
                if (_apTrussVirtual.IsYAlarmResetState())
                {
                    if (!_apTrussVirtual.IsYEnabled())
                    {
                        return 1;
                    }
                    if (_apTrussVirtual.IsYEnabled())
                    {
                        return 2;
                    }
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }
        
        // 10 : 桁架当前位置
        public (float X, float Y,
            float SpeedX, float SpeedY) GetTrussPose()
        {
            try
            {
                if (_apTrussVirtual != null && _apTrussVirtual.Online)
                {
                    float x = (float)Math.Round(_apTrussVirtual.GetXCurrentPosition(),3);
                    float y = (float)Math.Round(_apTrussVirtual.GetYCurrentPosition(),3);
                    float speedX = (float)Math.Round(_apTrussVirtual.GetXCurrentSpeed(),2);
                    float speedY = (float)Math.Round(_apTrussVirtual.GetYCurrentSpeed(),2);
                    return (x, y, speedX, speedY);
                }

                return (
                    0000.000f, 0000.000f,
                    000.00f, 000.00f
                );
            }
            catch
            {
                return (
                    0000.000f, 0000.000f,
                    000.00f, 000.00f
                );
            }
        }
        
        // 11 : 判断XY在指定位置
        private bool PlcIsTrussInPosition(float xTargetPosition, float yTargetPosition, float positionTolerance=1, float speedTolerance=1)
        {
            try
            {
                bool xP = Math.Abs(_apTrussVirtual.GetXCurrentPosition() - xTargetPosition) < positionTolerance;
                bool yP = Math.Abs(_apTrussVirtual.GetYCurrentPosition() - yTargetPosition) < positionTolerance;
                bool xS = Math.Abs(_apTrussVirtual.GetXCurrentSpeed()) < speedTolerance;
                bool yS = Math.Abs(_apTrussVirtual.GetYCurrentSpeed()) < speedTolerance;
                return xP && yP && xS && yS;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[214]: 判断桁架是否停止时发生异常，未完成判断");
                return false;
            }
        }
        
        // 12 : 急停，注意由于是仿真控制器所以后续不需要添加其他东西
        public void EmergyStop()
        {
            try
            {
                _apTrussVirtualCts?.Cancel();
                _apTrussVirtual.XStop(false);
                _apTrussVirtual.YStop(false);
                _apTrussVirtual.XStop(true);
                _apTrussVirtual.YStop(true);
                _apTrussVirtual.XEnable(false);
                _apTrussVirtual.YEnable(false);
                Log.Information("[214]: 执行仿真控制器急停");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[214]: 仿真控制器急停时异常");
            }
        }
        
        // 13 : 双轴暂停
        public void Pause()
        {
            try
            {
                // 清除Stop位，准备重新置位
                _apTrussVirtual.XStop(false);
                _apTrussVirtual.YStop(false);
                _apTrussVirtual.XStop(true);
                _apTrussVirtual.YStop(true);
                _apTrussVirtual.XStart(false);
                _apTrussVirtual.YStart(false);
                Log.Information("[214]: 双轴正式暂停，设置Start锁");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[214]: 双轴动作暂停时发生异常");
            }
        }
        
        // 14 : 双轴恢复
        public void Resume()
        {
            try
            {
                // 重新使能 + 设置Start
                _apTrussVirtual.XEnable(true);
                _apTrussVirtual.YEnable(true);
                _apTrussVirtual.XStart(true);
                _apTrussVirtual.YStart(true);
                _apTrussVirtual.XStop(false);
                _apTrussVirtual.YStop(false);
                Log.Information("[214]: 双轴正式恢复，设置Stop锁");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[214]: 双轴动作恢复时发生异常");
            }
        }
        
        // 15 : 前往XY位置
        public async Task<bool> PlcGotoPosition(float xTarget, float yTarget, float xSpeed = 250, float ySpeed = 250, float timeoutExtraSeconds = 3.0f, bool force=true)
        {
            try
            {
                _apTrussVirtualCts?.Dispose();
                _apTrussVirtualCts = new CancellationTokenSource();
                var ct = _apTrussVirtualCts.Token;
                
                if (_apTrussVirtual == null || !_apTrussVirtual.Online)
                {
                    Log.Warning("[214]: 仿真控制器未连接，无法执行移动");
                    return false;
                }

                float currentX = _apTrussVirtual.GetXCurrentPosition();
                float currentY = _apTrussVirtual.GetYCurrentPosition();

                float dx = Math.Abs(xTarget - currentX);
                float dy = Math.Abs(yTarget - currentY);

                float xTime = xSpeed > 0 ? dx / xSpeed : 0;
                float yTime = ySpeed > 0 ? dy / ySpeed : 0;

                float estimatedSeconds = Math.Max(xTime, yTime) + timeoutExtraSeconds;
                int timeoutMs = (int)(estimatedSeconds * 1000);

                if (force)
                {
                    ResetAlarm();
                    _apTrussVirtual.XEnable(true);
                    _apTrussVirtual.YEnable(true);
                    _apTrussVirtual.XStart(true);
                    _apTrussVirtual.YStart(true);
                    _apTrussVirtual.XStop(false);
                    _apTrussVirtual.YStop(false);
                }

                _apTrussVirtual.MoveTruss(xTarget, yTarget, xSpeed, ySpeed);

                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (PlcIsTrussInPosition(xTarget, yTarget))
                    {
                        return true;
                    }
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(200, ct).ConfigureAwait(false);
                }
                Log.Warning("[214]: 桁架移动超时");
                return false;
            }
            catch (OperationCanceledException oe)
            {
                Log.Warning(oe, "[214]: 移动被取消/暂停");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[214]: 机器人移动至坐标: ({xTarget:F1}, {yTarget:F1})故障");
                return false;
            }
            finally
            {
                if (_apTrussVirtualCts != null)
                {
                    _apTrussVirtualCts.Dispose();
                    _apTrussVirtualCts = null;
                }
            }
        }
        
        // 15.x        
        public Task<bool> PlcGotoPositionQuick(
            bool debug,
            float xTarget, float yTarget,
            float xSpeed, float ySpeed,
            bool force = true)
        {
            try
            {
                if (_apTrussVirtual == null || !_apTrussVirtual.Online)
                {
                    Log.Warning("[214]: 仿真控制器未连接，无法执行移动");
                    return Task.FromResult(false);
                }

                if (force)
                {
                    ResetAlarm(debug);
                    _apTrussVirtual.XEnable(true);
                    _apTrussVirtual.YEnable(true);
                    _apTrussVirtual.XStart(true);
                    _apTrussVirtual.YStart(true);
                    _apTrussVirtual.XStop(false);
                    _apTrussVirtual.YStop(false);
                }

                _apTrussVirtual.MoveTruss(xTarget, yTarget, xSpeed, ySpeed);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[214]: 桁架快速移动至坐标: ({xTarget:F1}, {yTarget:F1}) 故障");
                return Task.FromResult(false);
            }
        }
        
        // 16 : 刷新
        public bool Refresh()
        {
            try
            {
                if (_apTrussVirtual != null && _apTrussVirtual.Online)
                {
                    HeartBeats();
                    SetToRemote();
                    ResetAlarm();
                    EnableBoth();
                    Log.Information("[214]: 桁架刷新，心跳警报并使能");
                    return true;
                }
                Log.Information("[214]: 未检测到仿真控制器实例，调用初始化函数来刷新");
                return Init();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[214]: 仿真控制器刷新时发生异常");
                return false;
            }
        }
        
        // 17 : 加载参数
        private readonly string _configFilePath = "Paraments.xml";
        private string PlcLoadParameters(string defaultConfigFilePath)
        {
            try
            {
                string configFilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "MagicTool",
                    "Configs",
                    defaultConfigFilePath);
                
                if (!File.Exists(configFilePath))
                {
                    Log.Warning("[214]: 参数配置文件不存在，加载默认参数: 192.168.125.161:8888");
                    return "192.168.125.161";
                }
                XmlDocument doc = new XmlDocument();
                doc.Load(configFilePath);
                XmlNode currentConfig = doc.SelectSingleNode("/Configs/Current");
                string plcIp = currentConfig?.SelectSingleNode("Section1/VirtualPlcIp")?.InnerText.Trim() ?? "";
                Log.Information($"[214]: 读取PLC参数成功: {plcIp}");
                return plcIp;
            }
            catch (Exception ex)
            {
                Log.Error(ex,"[214]: 参数配置文件加载时异常，加载默认参数: 192.168.125.161:8888");
                return "192.168.125.161";
            }
        }
        
        // 18 : 仅双轴使能
        public void EnableBoth()
        {
            try
            {
                // 重新使能 + 设置Start
                _apTrussVirtual.XEnable(true);
                _apTrussVirtual.YEnable(true);
                Log.Information("[214]: 双轴使能");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[214]: 双轴使能时发生异常");
            }
        }

        public void UnlockHandling()
        {
            _apTrussVirtual.TemporarilyHandling(true);
            _apTrussVirtual.EnsureTrussEnabled(1000);
            Log.Information("已经清除桁架警报并使能");
        }
        public void LockHandling()
        {
            _apTrussVirtual.TemporarilyHandling(false);
        }
    }
}