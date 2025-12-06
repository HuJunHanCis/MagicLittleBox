using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Abb.Egm;
using Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Abb.Egm;
using Serilog;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Google.Protobuf;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace MagicLittleBox
{
    public partial class MainWindow
    {
        // 双系统实例化
        private readonly SupVirtualRobot _virtualRobot = SupVirtualRobot.Instance;
        private readonly SupVirtualTruss _virtualTruss = SupVirtualTruss.Instance;
        
        // 警示四色
        private static readonly SolidColorBrush BrushRed = new SolidColorBrush(Color.FromArgb(0xFF, 0xC9, 0x4F, 0x4F));
        private static readonly SolidColorBrush BrushYel = new SolidColorBrush(Color.FromArgb(0xFF, 0xD9, 0xB7, 0x2B));
        private static readonly SolidColorBrush BrushGre = new SolidColorBrush(Color.FromArgb(0xFF, 0x57, 0x96, 0x5C));
        private static readonly SolidColorBrush BrushGry = new SolidColorBrush(Color.FromArgb(0xFF, 0x91, 0x91, 0x91));

        #region WPF相关功能: 100%

            private void OnWindowDragMove(object sender, MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    DragMove();
                }
            }
            protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
            {
                var element = FocusManager.GetFocusedElement(this);
                if (element != null && !element.IsMouseOver)
                {
                    FocusManager.SetFocusedElement(this, this);
                }
                base.OnPreviewMouseDown(e);
            }

        #endregion

        #region CHECK检查区域: 100%

            // 1. 检查频率：4~1000 且自动调整为最近的 4 的倍数
            private string CheckValidFreq(TextBox textBox)
            {
                if (textBox == null)
                {
                    return null;
                }

                string raw = textBox.Text;
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return null;
                }

                int value;
                if (!int.TryParse(raw, out value))
                {
                    // 不是整数
                    return null;
                }

                if (value < 4 || value > 1000)
                {
                    // 不在合法范围
                    return null;
                }

                int remainder = value % 4;
                if (remainder != 0)
                {
                    int down = value - remainder;
                    int up = value + (4 - remainder);

                    // 选离原始值最近的 4 的倍数
                    int adjusted;
                    if (value - down <= up - value)
                    {
                        adjusted = down;
                    }
                    else
                    {
                        adjusted = up;
                    }

                    // 再做一次边界保护
                    if (adjusted < 4) adjusted = 4;
                    if (adjusted > 1000) adjusted = 1000;

                    value = adjusted;
                    textBox.Text = value.ToString();
                }

                // 返回 TextBox 当前内容
                return textBox.Text;
            }

            // 2. 检查端口：1~65534 的整数
            private string CheckValidPort(TextBox textBox)
            {
                if (textBox == null)
                {
                    return null;
                }

                string raw = textBox.Text;
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return null;
                }

                int port;
                if (!int.TryParse(raw, out port))
                {
                    return null;
                }

                // 简单数值范围判断即可
                if (port <= 0 || port >= 65535)
                {
                    return null;
                }

                return textBox.Text;
            }

            // 3. 检查地址：用正则判断 IPv4
            private string CheckValidAddr(TextBox textBox)
            {
                if (textBox == null)
                {
                    return null;
                }

                string raw = textBox.Text;
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return null;
                }

                string addr = raw.Trim();

                // 标准 IPv4 正则
                string pattern = @"^((25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\.){3}"
                                 + @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";

                if (Regex.IsMatch(addr, pattern))
                {
                    return addr;
                }

                return null;
            }
            
            private bool IsRobotAtHome(double homeTolerance=0.1)
            {
                if (_currentEightAxes == null || _currentEightAxes.Length < 6)
                    return false;

                return Math.Abs(_currentEightAxes[0] - 0) < homeTolerance &&
                       Math.Abs(_currentEightAxes[1] - 0) < homeTolerance &&
                       Math.Abs(_currentEightAxes[2] - 0) < homeTolerance &&
                       Math.Abs(_currentEightAxes[3] - 0) < homeTolerance &&
                       Math.Abs(_currentEightAxes[4] - 0) < homeTolerance &&
                       Math.Abs(_currentEightAxes[5] - 0) < homeTolerance;
            }

            private bool IsTrussAtHome(double homeTolerance=0.1)
            {
                if (_currentEightAxes == null || _currentEightAxes.Length < 8)
                    return false;

                return Math.Abs(_currentEightAxes[6] - 0) < homeTolerance &&
                       Math.Abs(_currentEightAxes[7] - 0) < homeTolerance;
            }

        #endregion

        #region 界面按钮功能区

            // RlListener 按钮是否处于监听状态
            private bool _rlListenerEnabled = false;
            // EGM 是否处于运行状态
            private bool _egmRunning = false;
            
            private void RlListenerToggle(object sender, RoutedEventArgs e)
            {
                // ToggleButton 本身的状态
                bool isChecked = RlListener.IsChecked == true;

                _rlListenerEnabled = isChecked;   // 更新字段（你要求的内容）

                if (isChecked)
                {
                    Log.Information("[RLL]: RL监听已开启");
                }
                else
                {
                    _virtualTruss.EmergyStop();
                    Log.Information("[RLL]: RL监听已关闭");
                }
            }

            private void EgmStart(object sender, RoutedEventArgs e)
            {
                try
                {
                    if (_egmRunning)
                    {
                        Log.Warning("[EGM]: 已经处于运行状态，忽略重复启动");
                        return;
                    }
                    
                    _virtualRobot.RestartSim();
                    
                    Thread.Sleep(200);

                    Log.Information("[EGM]: 开始启动流程");

                    // 1. 确保 UDP 监听已启动（如果端口无效，会在内部弹窗并返回）
                    UdpListener();
                    if (_udpListenerClient == null)
                    {
                        Log.Error("[EGM]: UDP 监听未成功启动，EGM 启动中止");
                        return;
                    }
                    
                    // 2. 监听成功后再锁 UI
                    LockEgmRunningStatus();
                    _rlListenerEnabled = true;

                    // 3. 启动数据发送线程
                    StartDataSendThread();
                    
                   

                    // 4. 打开RL监听
                    if (RlListener.IsChecked != true)
                    {
                        RlListener.IsChecked = true;
                    }
                    
                    _virtualTruss.EnableBoth();

                    _egmRunning = true;

                    Log.Information("[EGM]: 启动流程完成");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[EGM]: 启动失败");
                }
            }
            
            private void EgmStop(object sender, RoutedEventArgs e)
            {
                try
                {
                    if (!_egmRunning)
                    {
                        Log.Warning("[EGM]: 当前不在运行状态，忽略停止请求");
                        return;
                    }

                    Log.Information("[EGM]: 开始停止流程");

                    LockInitStatus();

                    // 1. 停止数据发送线程
                    StopDataSendThread();
                    
                    // 2. 停止 UDP 监听（真正关掉 socket）
                    StopUdpListener();

                    // 3. 关闭 RL 监听按钮（只是逻辑，不再解析 RL JSON）
                    if (RlListener.IsChecked == true)
                    {
                        RlListener.IsChecked = false;
                    }

                    // 4. 清理 EGM 相关状态
                    _egmRunning = false;
                    _robotEndpoint = null;   // 下次重新建立 EGM 通信时，从头开始
                    
                    _virtualTruss.EmergyStop();
                    
                    Log.Information("[EGM]: 停止流程完成");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[EGM]: 停止失败");
                    MessageBox.Show($"EGM 停止失败: {ex.Message}",
                        "EGM 错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            
            private void ResetTruss(object sender, RoutedEventArgs e)
            {
                
            }
            
            private void ResetRobot(object sender, RoutedEventArgs e)
            {
                
            }

            private void EmergyStop(object sender, RoutedEventArgs e)
            {
                
            }
            
            private void EnableMotors(object sender, RoutedEventArgs e)
            {
                
            }

            private async void F3GoHome(object sender, RoutedEventArgs e)
            {
                try
                {
                    // 1. 关闭 RL 监听并锁定按钮
                    Dispatcher.Invoke(() =>
                    {
                        RlListener.IsChecked = false;
                        RlListener.IsEnabled = false;
                    });
                    _rlListenerEnabled = false;

                    Log.Information("[F3]: 开始八轴回零");
                    
                    double[] robotHomeJoints = new double[6] { 0, 0, 0, 0, 0, 0 };
                    double trussXHome = 0f;
                    double trussYHome = 0f;
                    
                    var robotTask = Task.Run(async () =>
                        {
                            try
                            {
                                if (_egmRunning && _robotEndpoint != null && _currentRobotStatus == 3)
                                {
                                    Log.Information("[F3]: 机器人开始回零");
                                    while (_currentEightAxes[0]!=0||_currentEightAxes[1]!=0||_currentEightAxes[2]!=0||
                                           _currentEightAxes[3]!=0||_currentEightAxes[4]!=0||_currentEightAxes[5]!=0)
                                    {
                                        await SendJointMessageToRobot(robotHomeJoints);
                                        Thread.Sleep(50);
                                    }
                                    Log.Information("[F3]: 机器人回零完成");
                                }
                                else
                                {
                                    Log.Warning("[F3]: 机器人不在可回零状态，跳过");
                                }
                            }
                            catch (TaskCanceledException)
                            {
                                // 正常：新POSE或EgmStop触发取消
                                // Log.Debug("[POS]: 发送任务被取消");
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "[POS]: 机器人回零过程中发生异常");
                            }
                        });
                    
                    var trussTask = Task.Run(async () =>
                    {
                        try
                        {
                            if (_currentTrussStatus == 3)
                            {
                                Log.Information("[F3]: 桁架开始回零");
                                while (_currentEightAxes[6]!=0||_currentEightAxes[7]!=0)
                                {
                                    await _virtualTruss.PlcGotoPositionQuick(
                                        false,
                                        (float)0,
                                        (float)0,
                                        (float)500,   // 较慢的安全速度
                                        (float)500);
                                    Thread.Sleep(1000);
                                }
                                
                                Log.Information("[F3]: 桁架回零完成");
                            }
                            else
                            {
                                Log.Warning("[F3]: 桁架不在可回零状态，跳过");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "[POS]: 桁架回零过程中发生异常");
                        }
                    });
                    
                    try
                    {
                        await Task.WhenAll(robotTask, trussTask);
                        Log.Information("[F3]: 回零任务执行完成");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[F3]: 等待回零任务时发生异常");
                    }

                    // 5. 重新启用RL监听
                    if (IsRobotAtHome() && IsTrussAtHome())
                    {
                        Dispatcher.Invoke(() =>
                        {
                            RlListener.IsEnabled = true;
                            // RlListener.IsChecked = true;
                        });
                        // _rlListenerEnabled = true;
                        Log.Information("[F3]: RL监听已重新启用");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[F3]: 八轴回零时发生异常");
                }
            }

            private void F4Refresh(object sender, RoutedEventArgs e)
            {
                try
                {
                    Log.Information("[F4]: 手动刷新数据更新线程");
        
                    // 先停止现有的数据更新线程
                    StopDataUpdateThread();
                    
                    EgmStop(sender, e);

                    _virtualTruss.Init();
                    
                    _virtualRobot.Restart();
        
                    // 等待一小段时间确保线程完全停止
                    Thread.Sleep(50);
                    
                    // 重新启动数据更新线程
                    StartDataUpdateThread();

                    Log.Information("[F4]: 数据更新线程刷新完成");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[F4]: 刷新数据更新线程时发生异常");
                }
            }

        #endregion

        #region UDP监听区域: 100%

            // UDP 监听用到的字段
            private UdpClient _udpListenerClient;
            private CancellationTokenSource _udpListenerCts;

            /// <summary>
            /// 根据界面上的 PortListener TextBox 的值开启 UDP 监听服务
            /// </summary>
            private void UdpListener(bool box = true)
            {
                // 已经在监听，但允许重复启动
                
                if (_udpListenerClient != null)
                {
                    _udpListenerClient.Close();
                    _udpListenerClient = null;

                    if (_udpListenerCts != null)
                    {
                        _udpListenerCts.Cancel();
                        _udpListenerCts.Dispose();
                        _udpListenerCts = null;
                    }
                }

                // 使用你前面写好的检查函数
                string portText = CheckValidPort(PortListener);
                if (portText == null)
                {
                    Log.Error("[UDP]: 监听端口不合法，无法启动 UDP 监听");
                    if (box)
                    {
                        MessageBox.Show(
                            "监听端口不合法，请输入 1~65534 之间的整数端口号。",
                            "端口错误",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }

                    // 要求阻塞 UI：MessageBox.Show 本身就是阻塞的
                    return;
                }

                int port = int.Parse(portText);

                try
                {
                    _udpListenerClient = new UdpClient(port);
                    _udpListenerCts = new CancellationTokenSource();

                    var client = _udpListenerClient;
                    var cts = _udpListenerCts;

                    // 开启后台监听循环
                    Task.Run(() => UdpListenLoop(client, cts.Token));

                    Log.Information("[UDP]: 已启动监听端口 {Port}", port);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[UDP]: 启动 UDP 监听失败");

                    // 出错时把资源清理掉，避免留下半死不活的 client / cts
                    if (_udpListenerClient != null)
                    {
                        _udpListenerClient.Close();
                        _udpListenerClient = null;
                    }

                    if (_udpListenerCts != null)
                    {
                        _udpListenerCts.Cancel();
                        _udpListenerCts.Dispose();
                        _udpListenerCts = null;
                    }
                }
            }

            /// <summary>
            /// 停止 UDP 监听
            /// </summary>
            private void StopUdpListener()
            {
                try
                {
                    if (_udpListenerCts != null)
                    {
                        _udpListenerCts.Cancel();
                        _udpListenerCts.Dispose();
                        _udpListenerCts = null;
                    }

                    if (_udpListenerClient != null)
                    {
                        _udpListenerClient.Close();
                        _udpListenerClient = null;
                    }
                }
                catch
                {
                    // 静默处理即可
                }
            }

            /// <summary>
            /// UDP 监听循环
            /// </summary>
            private void UdpListenLoop(UdpClient client, CancellationToken token)
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        // 阻塞接收
                        byte[] data = client.Receive(ref remote);

                        // ==== 1. 优先尝试解析为 EGM 消息 ====
                        try
                        {
                            EgmRobot robotMessage = EgmRobot.Parser.ParseFrom(data);
                            // 你的 EGM 处理逻辑（和 MainListenLoop 一样）
                            ProcessEgmMessage(robotMessage, remote);
                            // EGM 解析成功就不再往下尝试 JSON 解析了
                            continue;
                        }
                        catch
                        {
                            // 不是 EGM 消息，忽略这个异常，继续尝试 JSON
                        }

                        // ==== 2. 再尝试解析为 JSON（RL 的 POSE / CONTROL）====
                        try
                        {
                            if (_rlListenerEnabled)
                            {
                                string jsonString = Encoding.UTF8.GetString(data);
                                var jsonMessage = JsonConvert.DeserializeObject<dynamic>(jsonString);
                                // Console.WriteLine(jsonString);

                                if (jsonMessage?.Header != null)
                                {
                                    string header = jsonMessage.Header.ToString();
                                    string type = jsonMessage.Type?.ToString();

                                    if (header == "RL" && type == "POSE")
                                    {
                                        ProcessPoseMessage(jsonMessage, jsonMessage.TimeStamp?.ToString());
                                        // === 界面闪绿灯 500ms ===
                                        Dispatcher.Invoke(() =>
                                        {
                                            // 点亮绿灯
                                            RlConnection.Fill =
                                                (SolidColorBrush)(new BrushConverter().ConvertFrom("#4CAF50"));
                                        });

                                        Task.Delay(500).ContinueWith(_ =>
                                        {
                                            try
                                            {
                                                Dispatcher.Invoke(() =>
                                                {
                                                    RlConnection.Fill =
                                                        (SolidColorBrush)(new BrushConverter().ConvertFrom(
                                                            "#999999"));
                                                });
                                            }
                                            catch
                                            {
                                                // 窗口已经关闭之类情况，静默处理
                                            }
                                        });
                                    }

                                    if (header == "RL" && type == "CONTROL")
                                    {
                                        ProcessCtrlMessage(jsonMessage, jsonMessage.TimeStamp?.ToString());
                                        Dispatcher.Invoke(() =>
                                        {
                                            // 点亮绿灯
                                            RlConnection.Fill =
                                                (SolidColorBrush)(new BrushConverter().ConvertFrom("#C94F4F"));
                                        });
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // 既不是 EGM 也不是合法 JSON，直接丢弃
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    // 正常：关闭 client 时会触发，直接吃掉即可
                    Log.Error("[UDP]: 监听循环被强制关闭");
                }
                catch (SocketException)
                {
                    // 一个封锁操作被对 WSACancelBlockingCall 的调用中断 = 我们主动关掉了 socket
                    Log.Error("[UDP]: 监听循环被取消");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[UDP]: 监听循环异常");
                }
            }


        #endregion

        #region 后台定频更新
        
            private (string text, SolidColorBrush color) GetRobotStatusInfo(int status)
            {
                switch (status)
                {
                    case -1:
                        // 未连接／异常
                        return ("异常状态", BrushGry);

                    case 2:
                        // MotorsOff
                        return ("电机关闭", BrushYel);

                    case 3:
                        // MotorsOn
                        return ("正常运行", BrushGre);

                    case 6:
                        // EmergencyStopReset
                        return ("急停复位", BrushRed);

                    case 4:
                    case 5:
                    case 7:
                    case 99:
                        // GuardStop / EStop / SystemFailure / Unknown
                        return ("紧急停止", BrushRed);

                    default:
                        return ("未知状态", BrushGry);
                }
            }

            private static (string text, SolidColorBrush color) GetPlcStatusInfo(int status)
            {
                switch (status)
                {
                    case -1:
                        // 控制器对象为空、异常等
                        return ("异常状态", BrushGry);   // 灰色

                    case 1:
                        // 不在线
                        return ("离线状态", BrushRed);   // 红色

                    case 2:
                        // 在线，本地手动模式
                        return ("手动模式", BrushYel);   // 黄色

                    case 3:
                        // 在线，远程调试 / 远程自动模式
                        return ("远程模式", BrushGre);   // 绿色

                    default:
                        return ("未知状态", BrushGry);   // 灰色
                }
            }

            // 后台数据更新用到的字段
            private CancellationTokenSource _dataUpdateCts;
            private Task _dataUpdateTask;

            // 当前八轴数据：J1~J6 + TrussX + TrussY
            private double[] _currentEightAxes = new double[8];
            private int _currentRobotStatus;
            private int _currentTrussStatus;

            // 启动后台数据更新线程
            private void StartDataUpdateThread()
            {
                // 若已有旧任务在跑，先停掉
                StopDataUpdateThread();

                _dataUpdateCts = new CancellationTokenSource();
                var ct = _dataUpdateCts.Token;
                
                Log.Information("[RDU]: 数据更新任务已正常开始");
                _dataUpdateTask = Task.Run(async () => await RunDataUpdateLoop(ct));
            }

            // 后台数据更新循环（只负责采集 + 刷新界面）
            private async Task RunDataUpdateLoop(CancellationToken ct)
            {
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        double[] eightAxes = new double[8];

                        // 1. 获取状态码
                        _currentRobotStatus = _virtualRobot.GetVirRobotStatus_Full();
                        _currentTrussStatus = _virtualTruss.GetVirPlcStatus_Full();

                        var (robotText, robotColor) = GetRobotStatusInfo(_currentRobotStatus);
                        var (plcText, plcColor) = GetPlcStatusInfo(_currentTrussStatus);

                        // 2. 获取机器人当前关节
                        if (_currentRobotStatus == -1)
                        {
                            eightAxes[0] = 0;
                            eightAxes[1] = 0;
                            eightAxes[2] = 0;
                            eightAxes[3] = 0;
                            eightAxes[4] = 0;
                            eightAxes[5] = 0;
                        }
                        else
                        {
                            var (_, _, _, _, _, _, _,
                                j1, j2, j3, j4, j5, j6) = _virtualRobot.GetCurrentPose();

                            eightAxes[0] = j1;
                            eightAxes[1] = j2;
                            eightAxes[2] = j3;
                            eightAxes[3] = j4;
                            eightAxes[4] = j5;
                            eightAxes[5] = j6;
                        }

                        // 3. 获取桁架当前坐标
                        if (_currentTrussStatus == -1)
                        {
                            eightAxes[6] = 0;
                            eightAxes[7] = 0;
                        }
                        else
                        {
                            var (trussX, trussY, _, _) = _virtualTruss.GetTrussPose();
                            eightAxes[6] = trussX;
                            eightAxes[7] = trussY;
                        }

                        // 4. 写入当前八轴状态
                        _currentEightAxes = eightAxes;

                        // 5. 更新界面显示
                        try
                        {
                            Dispatcher.Invoke(() =>
                            {
                                AbbJ1.Text = eightAxes[0].ToString("F3");
                                AbbJ2.Text = eightAxes[1].ToString("F3");
                                AbbJ3.Text = eightAxes[2].ToString("F3");
                                AbbJ4.Text = eightAxes[3].ToString("F3");
                                AbbJ5.Text = eightAxes[4].ToString("F3");
                                AbbJ6.Text = eightAxes[5].ToString("F3");

                                PlcX.Text = eightAxes[6].ToString("F4");
                                PlcY.Text = eightAxes[7].ToString("F4");

                                AbbStatusText.Text = robotText;
                                AbbStatusText.Foreground = robotColor;

                                PlcStatusText.Text = plcText;
                                PlcStatusText.Foreground = plcColor;
                            });
                        }
                        catch
                        {
                            // 窗口关闭等情况，忽略
                        }

                        // 6. 高频更新 - 50ms 间隔（支持取消）
                        await Task.Delay(50, ct);
                    }
                }
                catch (OperationCanceledException)
                {
                    // 手动取消（StopDataUpdateThread / 程序退出）时的正常结束
                    Log.Information("[RDU]: 数据更新任务已正常取消");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[RDU]: 更新失败");
                }
            }

            // 停止后台数据更新
            private void StopDataUpdateThread()
            {
                try
                {
                    if (_dataUpdateCts != null)
                    {
                        _dataUpdateCts.Cancel();
                    }

                    if (_dataUpdateTask != null)
                    {
                        _dataUpdateTask.Wait(1000); // 等待 1 秒让任务结束
                    }

                    _dataUpdateCts?.Dispose();
                    _dataUpdateCts = null;
                    _dataUpdateTask = null;
                }
                catch (Exception ex)
                {
                    Log.Error($"[后台数据更新]: 停止失败: {ex.Message}");
                }
            }

        #endregion

        #region 仿真及学习的数据发送

            // 数据发送用到的字段
            private CancellationTokenSource _dataSendCts;
            private Task _dataSendTask;
            private UdpClient _rlUdpClient;
            private UdpClient _ueUdpClient;
            private IPEndPoint _rlEndpoint;
            private IPEndPoint _ueEndpoint;
            
            // 启动数据发送线程
            private void StartDataSendThread()
            {
                // 若已有旧任务在跑，先停掉
                StopDataSendThread();

                // 初始化 UDP 发送客户端
                InitializeUdpClients();

                // 如果没有合法的发送目标，直接返回
                if (_rlUdpClient == null && _ueUdpClient == null)
                {
                    Log.Warning("[SED]: 没有合法的发送目标，数据发送线程未启动");
                    return;
                }

                _dataSendCts = new CancellationTokenSource();
                var ct = _dataSendCts.Token;

                _dataSendTask = Task.Run(async () => await RunDataSendLoop(ct));
                Log.Information("[SED]: 数据发送线程已启动");
            }

            // 初始化发送客户端双端
            private void InitializeUdpClients()
            {
                // 1. 初始化 RL 发送客户端
                string rlIpText = CheckValidAddr(IpReinLearn);
                string rlPortText = CheckValidPort(PortReinLearn);
        
                if (rlIpText != null && rlPortText != null)
                {
                    try
                    {
                        _rlEndpoint = new IPEndPoint(IPAddress.Parse(rlIpText), int.Parse(rlPortText));
                        _rlUdpClient = new UdpClient();
                        Log.Information("[SED]: RL发送目标已设置: {IP}:{Port}", rlIpText, rlPortText);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[SED]: 创建RLUDP客户端失败");
                        _rlUdpClient = null;
                    }
                }
                else
                {
                    Log.Warning("[SED]: RLIP或端口不合法，跳过RL发送");
                    _rlUdpClient = null;
                }

                // 2. 初始化 UE 发送客户端
                string ueIpText = CheckValidAddr(IpUe);
                string uePortText = CheckValidPort(PortUe);
        
                if (ueIpText != null && uePortText != null)
                {
                    try
                    {
                        _ueEndpoint = new IPEndPoint(IPAddress.Parse(ueIpText), int.Parse(uePortText));
                        _ueUdpClient = new UdpClient();
                        Log.Information("[SED]: UE发送目标已设置: {IP}:{Port}", ueIpText, uePortText);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[SED]: 创建UEUDP客户端失败");
                        _ueUdpClient = null;
                    }
                }
                else
                {
                    Log.Warning("[SED]: UEIP或端口不合法，跳过UE发送");
                    _ueUdpClient = null;
                }
            }
            
            // 停止数据发送线程
            private void StopDataSendThread()
            {
                try
                {
                    if (_dataSendCts != null)
                    {
                        _dataSendCts.Cancel();
                        _dataSendCts.Dispose();
                        _dataSendCts = null;
                    }

                    if (_dataSendTask != null)
                    {
                        _dataSendTask.Wait(1000);
                        _dataSendTask = null;
                    }

                    // 清理 UDP 客户端
                    try
                    {
                        _rlUdpClient?.Close();
                        _ueUdpClient?.Close();
                        _rlUdpClient = null;
                        _ueUdpClient = null;
                        _rlEndpoint = null;
                        _ueEndpoint = null;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[SED]: 清理UDP客户端时发生异常");
                    }
            
                    Log.Information("[SED]: 数据发送线程已停止");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[SED]: 停止数据发送线程失败");
                }
            }
            
            // 数据发送循环
            private async Task RunDataSendLoop(CancellationToken ct)
            {
                try
                {
                    int sendInterval = 0;
                    // 获取发送频率
                    Dispatcher.Invoke(() =>
                    {
                        string freqText = CheckValidFreq(FreOutRlUe);
                        if (freqText == null)
                        {
                            Log.Error("[SED]: 发送频率不合法，发送线程退出");
                            return;
                        }
                        sendInterval = int.Parse(freqText);
                        Log.Information("[SED]: 开始发送循环，频率: {Interval}ms", sendInterval);
                    });

                    while (!ct.IsCancellationRequested)
                    {
                        try
                        {
                            // 收集数据并发送
                            var sendData = CollectDataSend();
                    
                            // 发送到RL
                            if (_rlUdpClient != null && _rlEndpoint != null && sendData != null && sendData.Length > 0)
                            {
                                await _rlUdpClient.SendAsync(sendData, sendData.Length, _rlEndpoint);
                            }
                    
                            // 发送到UE
                            if (_ueUdpClient != null && _ueEndpoint != null && sendData != null && sendData.Length > 0)
                            {
                                await _ueUdpClient.SendAsync(sendData, sendData.Length, _ueEndpoint);
                            }
                    
                            // 等待指定间隔
                            if (sendInterval == 0)
                            {
                                Log.Error("[SED]: 发送频率为0，发送线程退出");
                                return;
                            }
                            await Task.Delay(sendInterval, ct);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "[SED]: 发送循环内部错误");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Information("[SED]: 数据发送任务已正常取消");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[SED]: 发送循环异常退出");
                }
            }
            
            // 收集数据
            private byte[] CollectDataSend()
            {
                try
                {
                    // 收集当前八轴数据
                    var currentData = _currentEightAxes;
                    
                    // 2. 获取机器人当前关节
                    if (_currentRobotStatus == -1 || _currentTrussStatus == -1)
                    {
                        // 根据具体状态设置对应的状态文本
                        string robotStatusText = _currentRobotStatus == -1 ? "Stopped" : "Running";
                        string trussStatusText = _currentTrussStatus == -1 ? "Stopped" : "Running";
                        var sendData = new
                        {
                            Header = "ERROR",
                            Timestamp = DateTime.Now.ToString("yyMMddHHmmssfff"),
                            RobotStatus = robotStatusText,
                            TrussStatus = trussStatusText
                        };
                        string jsonString = JsonConvert.SerializeObject(sendData);
                        return Encoding.UTF8.GetBytes(jsonString);
                    }
                    else
                    {
                        var sendData = new
                        {
                            Header = "Normal",
                            Timestamp = DateTime.Now.ToString("yyMMddHHmmssfff"),
                            J1 = currentData[0],
                            J2 = currentData[1],
                            J3 = currentData[2],
                            J4 = currentData[3],
                            J5 = currentData[4],
                            J6 = currentData[5],
                            TrussX = currentData[6],
                            TrussY = currentData[7],
                            RobotStatus = "Running",
                            TrussStatus = "Running"
                        };
                        string jsonString = JsonConvert.SerializeObject(sendData);
                        return Encoding.UTF8.GetBytes(jsonString);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[SED]: 收集发送数据失败");
                    return new byte[0];
                }
            }
        
        #endregion

        #region 三大处理函数

            private IPEndPoint _robotEndpoint; // 机器人端点
            private uint _egmSequenceNumber; // EGM序列号
            private readonly double[] _egmPositions = { 0, 0, 0, 0, 0, 0 };
            private void ProcessEgmMessage(EgmRobot robotMessage, IPEndPoint sender)
            {
                try
                {
                    _robotEndpoint = sender;
                    if (robotMessage?.FeedBack?.Joints != null)
                    {
                        var joints = robotMessage.FeedBack.Joints;

                        // 更新关节位置到长度为6的数组
                        for (int i = 0; i < 6; i++)
                        {
                            _egmPositions[i] = joints.Joints[i];  // 直接取double值
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"[EGM]: 解析EGM消息失败: {ex.Message}");
                }
            }
            
            // private CancellationTokenSource _rlPoseSendCts;
            private double _poseJ1;
            private double _poseJ2;
            private double _poseJ3;
            private double _poseJ4;
            private double _poseJ5;
            private double _poseJ6;
            private double _poseTrussX;
            private double _poseTrussY;
            private int _poseRobotSteps;
            private int _poseTrussSteps;
            
            private CancellationTokenSource _poseSendCts;
            
            private readonly double[][] _axisLimits = 
            {
                new double[] { -179, 179 }, // J1 关节
                new double[] { -89, 69 }, // J2 关节  
                new double[] { -129, 72 }, // J3 关节
                new double[] { -169, 169 }, // J4 关节
                new double[] { -116, 89 }, // J5 关节
                new double[] { -139, 125 }, // J6 关节
                new double[] { -650, 4200 },   // 桁架X轴
                new double[] { -550, 2429 }     // 桁架Y轴
            };
            
            private void ProcessPoseMessage(dynamic jsonMessage, string timeStamp)
            {
                try
                {
                    int robotIntervalMs = 0;
                    int trussIntervalMs = 0;
                    
                    if (_egmRunning && _robotEndpoint != null && _currentRobotStatus ==3 && _currentTrussStatus ==3)
                    {
                        _poseJ1 = (double)jsonMessage.Rax1;
                        _poseJ2 = (double)jsonMessage.Rax2;
                        _poseJ3 = (double)jsonMessage.Rax3;
                        _poseJ4 = (double)jsonMessage.Rax4;
                        _poseJ5 = (double)jsonMessage.Rax5;
                        _poseJ6 = (double)jsonMessage.Rax6;
                        _poseTrussX = (double)jsonMessage.TrussX;
                        _poseTrussY = (double)jsonMessage.TrussY;
                        
                        Dispatcher.Invoke(() =>
                        {
                            string robotFreqText = CheckValidFreq(FreRobot);
                            string trussFreqText = CheckValidFreq(FreTruss);
                
                            if (robotFreqText == null || trussFreqText == null)
                            {
                                Log.Warning("[POSE]: 频率设置无效，忽略当前POSE消息");
                                return;  // 注意：这个return只退出匿名方法
                            }
                
                            robotIntervalMs = int.Parse(robotFreqText);
                            trussIntervalMs = int.Parse(trussFreqText);
                            
                            _poseRobotSteps = (int)Math.Ceiling(1050.0 / robotIntervalMs);
                            if (_poseRobotSteps <= 0) _poseRobotSteps = 1;

                            _poseTrussSteps = (int)Math.Ceiling(1050.0 / trussIntervalMs);
                            if (_poseTrussSteps <= 0) _poseTrussSteps = 1;
                        });
                        
                        double[] axes = _currentEightAxes;
                        if (axes == null || axes.Length < 8)
                        {
                            Log.Warning("[POSE]: 当前八轴数据无效，忽略本次POSE");
                            return;
                        }

                        double[] currentAxesSnapshot = new double[8];
                        Array.Copy(axes, currentAxesSnapshot, 8);
                        
                        List<double> j1List = ComputeSingleJointDisplacement(currentAxesSnapshot[0], _poseJ1, _poseRobotSteps, robotIntervalMs,_axisLimits[0][0],_axisLimits[0][1]);
                        List<double> j2List = ComputeSingleJointDisplacement(currentAxesSnapshot[1], _poseJ2, _poseRobotSteps, robotIntervalMs,_axisLimits[1][0],_axisLimits[1][1]);
                        List<double> j3List = ComputeSingleJointDisplacement(currentAxesSnapshot[2], _poseJ3, _poseRobotSteps, robotIntervalMs,_axisLimits[2][0],_axisLimits[2][1]);
                        List<double> j4List = ComputeSingleJointDisplacement(currentAxesSnapshot[3], _poseJ4, _poseRobotSteps, robotIntervalMs,_axisLimits[3][0],_axisLimits[3][1]);
                        List<double> j5List = ComputeSingleJointDisplacement(currentAxesSnapshot[4], _poseJ5, _poseRobotSteps, robotIntervalMs,_axisLimits[4][0],_axisLimits[4][1]);
                        List<double> j6List = ComputeSingleJointDisplacement(currentAxesSnapshot[5], _poseJ6, _poseRobotSteps, robotIntervalMs,_axisLimits[5][0],_axisLimits[5][1]);

                        // List<double> txList = ComputeSingleJointDisplacement(currentAxesSnapshot[6], _poseTrussX, _poseTrussSteps, trussIntervalMs);
                        // List<double> tyList = ComputeSingleJointDisplacement(currentAxesSnapshot[7], _poseTrussY, _poseTrussSteps, trussIntervalMs);

                        double trussxTarget = currentAxesSnapshot[6] + 1.05 * _poseTrussX;
                        if (trussxTarget > _axisLimits[6][1])
                            trussxTarget =  _axisLimits[6][1];
                        else if (trussxTarget < _axisLimits[6][0])
                            trussxTarget = _axisLimits[6][0];
                        double trussyTarget = currentAxesSnapshot[7] + 1.05 * _poseTrussY;
                        if (trussxTarget > _axisLimits[7][1])
                            trussxTarget =  _axisLimits[7][1];
                        else if (trussxTarget < _axisLimits[7][0])
                            trussxTarget = _axisLimits[7][0];
                        
                        // 调用egm发送+plc发送（均异步）
                        _poseSendCts?.Cancel();
                        _poseSendCts = new CancellationTokenSource();
                        var token = _poseSendCts.Token;
                        
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                int robotIndex = 0;
                                DateTime lastRobotSendTime = DateTime.Now;

                                while (!token.IsCancellationRequested && robotIndex < j1List.Count)
                                {
                                    DateTime now = DateTime.Now;

                                    if ((now - lastRobotSendTime).TotalMilliseconds >= robotIntervalMs)
                                    {
                                        // 状态检查，防止机器人不在远程运行时还乱发
                                        if (_egmRunning && _robotEndpoint != null && _currentRobotStatus == 3)
                                        {
                                            double[] targetJoints = new double[6];
                                            targetJoints[0] = j1List[robotIndex];
                                            targetJoints[1] = j2List[robotIndex];
                                            targetJoints[2] = j3List[robotIndex];
                                            targetJoints[3] = j4List[robotIndex];
                                            targetJoints[4] = j5List[robotIndex];
                                            targetJoints[5] = j6List[robotIndex];

                                            await SendJointMessageToRobot(targetJoints);
                                        }

                                        lastRobotSendTime = now;
                                        robotIndex++;
                                    }

                                    await Task.Delay(1, token); // 降一点 CPU 占用
                                }
                            }
                            catch (TaskCanceledException)
                            {
                                // 正常：新POSE或EgmStop触发取消
                                // Log.Debug("[POS]: 发送任务被取消");
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "[POS]: 机器人发送过程中发生异常");
                            }
                        }, token);

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _virtualTruss.PlcGotoPositionQuick(
                                    false,
                                    (float)trussxTarget,
                                    (float)trussyTarget,
                                    (float)_poseTrussX,   // 速度用当前 pose 里的值
                                    (float)_poseTrussY);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "[POS]: 桁架发送过程中发生异常");
                            }
                        });
                        
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            
            private async Task SendJointMessageToRobot(double[] joints)
            {
                if (_robotEndpoint == null || joints.Length != 6) 
                {
                    Log.Warning("[EGM发送]: 机器人端点不存在，检查RS是否打开");
                    return;
                }

                using (var memoryStream = new MemoryStream())
                {
                    try
                    {
                        var sensorMessage = new EgmSensor();
                        CreateJointMessage(sensorMessage, joints);
                        sensorMessage.WriteTo(memoryStream);

                        var data = memoryStream.ToArray();
                        await _udpListenerClient.SendAsync(data, data.Length, _robotEndpoint);
                 
                        // Log.Debug($"[EGM发送]: 已向 {_robotEndpoint} 发送关节指令");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[EGM发送]: 发送失败: {ex.Message}");
                    }
                }
            }
            
            private void CreateJointMessage(EgmSensor message, double[] joints)
            {
                var header = new EgmHeader
                {
                    Seqno = _egmSequenceNumber++,
                    Tm = (uint)DateTime.Now.Ticks,
                    Mtype = EgmHeader.Types.MessageType.MsgtypeCorrection
                };

                message.Header = header;

                var planned = new EgmPlanned();
                var egmJoints = new EgmJoints();

                // 添加6个关节值
                for (int i = 0; i < 6; i++)
                {
                    egmJoints.Joints.Add(joints[i]);
                }

                planned.Joints = egmJoints;
                message.Planned = planned;
            }
            
            /// <summary>
            /// 按照给定的规划列表和频率，在 1050ms 内依次发送桁架目标位置（开环可覆写）
            /// </summary>
            private async Task SendTrussPoseAsync(
                List<double> txList,
                List<double> tyList,
                int trussIntervalMs)
            {
                try
                {
                    if (txList == null || tyList == null ||
                        txList.Count == 0 || tyList.Count == 0)
                    {
                        return;
                    }

                    int trussIndex = 0;
                    DateTime lastTrussSendTime = DateTime.Now;

                    // 添加 _rlListenerEnabled 检查
                    while (_egmRunning && _rlListenerEnabled && trussIndex < txList.Count)
                    {
                        DateTime now = DateTime.Now;

                        if ((now - lastTrussSendTime).TotalMilliseconds >= trussIntervalMs)
                        {
                            if (_currentTrussStatus == 3)
                            {
                                double trussX = txList[trussIndex];
                                double trussY = tyList[trussIndex];

                                await _virtualTruss.PlcGotoPositionQuick(
                                    false,
                                    (float)trussX,
                                    (float)trussY,
                                    (float)_poseTrussX,   // 仍然用当前 POSE 的速度
                                    (float)_poseTrussY);
                            }

                            lastTrussSendTime = now;
                            trussIndex++;
                        }

                        // 不用 token，只要 EGM 在跑就继续
                        await Task.Delay(1);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[POSE发送-桁架]: 发送过程中发生异常");
                }
            }

        
            private void ProcessCtrlMessage(dynamic jsonMessage, string timeStamp)
            {
                try
                {
                    if (jsonMessage.Message.ToString() == "STOP")
                    {
                        EgmStop(null,null);
                    }
                    if (jsonMessage.Message.ToString() == "RESTART")
                    {
                        F4Refresh(null,null);
                        Thread.Sleep(5000);
                        EgmStart(null,null);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "[RLC]: 读取控制指令时发生异常");
                }
            }

        #endregion

        #region 积分计算相关

            private List<double[]> ComputeJointDisplacements(double[] velocities, int steps, int intervalMs)
            {
                var result = new List<double[]>(steps);
                if (velocities == null || velocities.Length < 6 || steps <= 0 || intervalMs <= 0)
                {
                    return result;
                }

                double dt = intervalMs / 1000.0; // 每一步对应的时间（秒）

                for (int i = 0; i < steps; i++)
                {
                    var stepDelta = new double[6];
                    for (int j = 0; j < 6; j++)
                    {
                        stepDelta[j] = velocities[j] * dt;  // v * dt = Δθ
                    }
                    result.Add(stepDelta);
                }

                return result;
            }
            
            private List<double> ComputeSingleJointDisplacement(double currentValue, double velocity, int steps, 
                int intervalMs,double minLimit, double maxLimit)
            {
                var result = new List<double>(steps);
                if (steps <= 0 || intervalMs <= 0)
                    return result;

                double dt = intervalMs / 1000.0;      // 每步时间（秒）
                double deltaPerStep = velocity * dt;  // 每步增量
                double value = currentValue;

                for (int i = 0; i < steps; i++)
                {
                    value += deltaPerStep;   // 每一步在上一步基础上往前走
                    if (value > maxLimit)
                        value = maxLimit;
                    else if (value < minLimit)
                        value = minLimit;
                    result.Add(value);       // 记录的是“这一时刻的绝对值”
                }

                return result;
            }

        #endregion

        #region WPF界面控件锁

            private void LockInitStatus()
            {
                IpReinLearn.IsEnabled = true;
                PortReinLearn.IsEnabled = true;
                IpEgm.IsEnabled = true;
                PortEgm.IsEnabled = true;
                IpUe.IsEnabled = true;
                PortUe.IsEnabled = true;
                FreRobot.IsEnabled = true;
                FreTruss.IsEnabled = true;
                FreOutRlUe.IsEnabled = true;
                PortListener.IsEnabled = true;
                    
                RlListener.IsEnabled = false;
                
                EgmStartButton.IsEnabled = true;
                EgmStopButton.IsEnabled = false;
            }
            private void LockEgmRunningStatus()
            {
                IpReinLearn.IsEnabled = false;
                PortReinLearn.IsEnabled = false;
                IpEgm.IsEnabled = false;
                PortEgm.IsEnabled = false;
                IpUe.IsEnabled = false;
                PortUe.IsEnabled = false;
                FreRobot.IsEnabled = false;
                FreTruss.IsEnabled = false;
                FreOutRlUe.IsEnabled = false;
                PortListener.IsEnabled = false;
                    
                RlListener.IsEnabled = true;
                
                EgmStartButton.IsEnabled = false;
                EgmStopButton.IsEnabled = true;
            }

        #endregion
        
        public MainWindow()
        {
            InitializeComponent();

            _virtualRobot.Init();
            _virtualTruss.Init();
            
            LockInitStatus();
            
            StartDataUpdateThread();
            
            UdpListener(false);
        }

    }
}

