// Legacy backup of MainWindow code as of v0.1.2. This file keeps the
// previous implementation commented out so future refactors of
// MainWindow can reference historical logic without pulling older
// branches.
//
// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Diagnostics;
// using Abb.Egm;
// using Serilog;
// using System.IO;
// using System.Net;
// using System.Text;
// using System.Windows;
// using Google.Protobuf;
// using Newtonsoft.Json;
// using System.Threading;
// using System.Net.Sockets;
// using System.Runtime.Remoting.Messaging;
// using System.Windows.Media;
// using System.Windows.Input;
// using System.Threading.Tasks;
// using System.Windows.Controls;
// using System.Text.RegularExpressions;
//
// namespace MagicLittleBox
// {
//     public partial class MainWindow
//     {
//         private bool _debug = true;
//
//         #region 整理
//         
//         private bool _errorReported = false;  // 是否已经报告过错误
//         
         // private readonly double[][] _axisLimits = 
         // {
         //     new double[] { -179, 179 }, // J1 关节
         //     new double[] { -89, 69 }, // J2 关节  
         //     new double[] { -129, 72 }, // J3 关节
         //     new double[] { -169, 169 }, // J4 关节
         //     new double[] { -116, 89 }, // J5 关节
         //     new double[] { -139, 125 }, // J6 关节
         //     new double[] { -650, 4200 },   // 桁架X轴
         //     new double[] { -550, 2429 }     // 桁架Y轴
         // };
//         
//         // private readonly double[][] _axisLimits = new double[8][]
//         // {
//         //     new double[] { -175, 175 }, // J1 关节
//         //     new double[] { -85, 65 }, // J2 关节  
//         //     new double[] { -125, 70 }, // J3 关节
//         //     new double[] { -165, 165 }, // J4 关节
//         //     new double[] { -115, 85 }, // J5 关节
//         //     new double[] { -135, 230 }, // J6 关节
//         //     new double[] { -650, 4200 },   // 桁架X轴
//         //     new double[] { -550, 2429 }     // 桁架Y轴
//         // };
//         
//         // ==== EGM 错误暂停相关 ====
//         private bool _egmPausedByError = false;   // 是否因为错误暂停了EGM控制
//         private bool _devicesNormalLast = true;   // 上一轮循环的设备状态（是否正常）
//
//         private Stopwatch _integrationStopwatch = new Stopwatch();
//         private DateTime _lastIntegrationTime = DateTime.Now;
//         
//         // 最重要的UDP监听字段
//         private UdpClient _localListener;
//         private CancellationTokenSource _localCts;
//
//         // 强化学习报文机制
//         private string _lastRlTimestamp = "";          // 最后处理的RL时间戳
//         private bool _rlDataValid;             // RL数据是否在有效期内
//         private System.Timers.Timer _rlValidityTimer;  // 数据有效期定时器
//         
//         private UdpClient _udpRlSender;
//         private UdpClient _udpUeSender;
//         private IPEndPoint _rlEndpoint;
//         private IPEndPoint _ueEndpoint;
//         
//         private Timer _periodicSendTimer;
//         private readonly object _sendLock = new object();
//         private bool _isSendingEnabled = false;
//         
//         private readonly SupVirtualRobot _virtualRobot = SupVirtualRobot.Instance;
//         private readonly SupVirtualTruss _virtualTruss = SupVirtualTruss.Instance;
//         
//         private string _abbStatus = "";
//         private string _plcStatus = "";
//         
//         private static readonly SolidColorBrush BrushRed = new SolidColorBrush(Color.FromArgb(0xFF, 0xC9, 0x4F, 0x4F));
//         private static readonly SolidColorBrush BrushYel = new SolidColorBrush(Color.FromArgb(0xFF, 0xD9, 0xB7, 0x2B));
//         private static readonly SolidColorBrush BrushGre = new SolidColorBrush(Color.FromArgb(0xFF, 0x57, 0x96, 0x5C));
//         private static readonly SolidColorBrush BrushGry = new SolidColorBrush(Color.FromArgb(0xFF, 0x91, 0x91, 0x91));
//
//
//         #endregion
//         
//         
//         
//         private System.Timers.Timer _rlBlinkTimer;  // RL指示灯闪烁定时器
//         
//         private readonly double[] _currentJointPositions = new double[6] { 0, 0, 0, 0, 0, 0 };
//
//         // EGM启动后所有textbox和界面上的按钮都要设置锁（除了egmend）
//         // 在start里设置为true后界面再锁，在end里重新设置为false
//         private bool _egmStart = false; 
//         private bool _isRlListen = false; 
//         
//         // 控制状态管理
//         private CancellationTokenSource _controlCts;
//
//         // 速度控制核心
//         private double[] _targetVelocities = new double[] { 0, 0, 0, 0, 0, 0 };      // RL指令的目标速度
//         private double _targetTrussX;
//         private double _targetTrussY;
//         
//         private double _currentTrussX;
//         private double _currentTrussY;
//         
//         // 新增：保存 RL 给的桁架速度
//         private double _targetTrussVx;
//         private double _targetTrussVy;
//         
//         private double[] _integratedPositions = new double[6];   // 积分得到的目标位置
//
//         // 双缓冲机制
//         private readonly object _bufferLock = new object();
//         private readonly double[][] _positionBuffers = new double[2][];   // 双缓冲数组
//         private int _currentWriteBuffer;                     // 当前写缓冲区索引
//         private int _currentReadBuffer = 1;                      // 当前读缓冲区索引
//         private bool _newDataAvailable;                  // 新数据可用标志
//
//         // 机器人通信
//         private IPEndPoint _robotEndpoint;                       // 机器人端点地址
//         private uint _egmSequenceNumber = 0;                     // EGM序列号
//
//         // 性能监控（可选，用于调试）
//         private int _sendCount = 0;                              // 发送计数
//         
//         // 归零功能状态记录
//         private bool _wasEgmRunning = false;  // 记录归零前EGM是否在运行
//         private UdpClient _zeroingListener;   // 归零专用的UDP监听
//         private CancellationTokenSource _zeroingCts; // 归零专用的取消令牌
//         
//         #region 界面自身相关
//
//             protected override void OnClosed(EventArgs e)
//             {
//                 try
//                 {
//                     StopListener();
//                     
//                     _udpRlSender?.Close();
//                     _udpRlSender?.Dispose();
//                     _udpRlSender = null;
//                     _rlEndpoint = null;
//                     
//                     _udpUeSender?.Close();
//                     _udpUeSender?.Dispose();
//                     _udpUeSender = null;
//                     _ueEndpoint = null;
//                     
//                     base.OnClosed(e);
//                 }
//                 catch (Exception exception)
//                 {
//                     Console.WriteLine(exception);
//                     throw;
//                 }
//             }
//             protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
//             {
//                 var element = FocusManager.GetFocusedElement(this);
//                 if (element != null && !element.IsMouseOver)
//                 {
//                     FocusManager.SetFocusedElement(this, this);
//                 }
//                 base.OnPreviewMouseDown(e);
//             }
//              private void OnWindowDragMove(object sender, MouseButtonEventArgs e)
//              {
//                  if (e.ChangedButton == MouseButton.Left)
//                  {
//                      DragMove();
//                  }
//              }
//             private void LockInterface(bool isLocked)
//             {
//                 Dispatcher.Invoke(() =>
//                 {
//                     if (isLocked)
//                     {
//                         // EGM运行时的锁定状态：只锁定输入框，按钮保持可用
//                         EgmStartButton.IsEnabled = false;    // 开始按钮禁用
//                         EgmStopButton.IsEnabled = true;      // 停止按钮保持可用
//                         RlListener.IsEnabled = true;         // RL监听按钮保持可用
//
//                         // 锁定所有输入框
//                         IpReinLearn.IsEnabled = false;
//                         PortReinLearn.IsEnabled = false;
//                         IpEgm.IsEnabled = false;
//                         PortEgm.IsEnabled = false;
//                         IpUe.IsEnabled = false;
//                         PortUe.IsEnabled = false;
//                         FreRobot.IsEnabled = false;
//                         FreTruss.IsEnabled = false;
//                         FreUe.IsEnabled = false;
//                         PortListener.IsEnabled = false;
//                     }
//                     else
//                     {
//                         // 完全解锁：所有按钮和输入框都可用
//                         EgmStartButton.IsEnabled = true;
//                         EgmStopButton.IsEnabled = true;
//                         RlListener.IsEnabled = true;
//                         
//                         // 归零按钮先不停
//                         // ZeroRefreshButton.IsEnabled = true;
//
//                         IpReinLearn.IsEnabled = true;
//                         PortReinLearn.IsEnabled = true;
//                         IpEgm.IsEnabled = true;
//                         PortEgm.IsEnabled = true;
//                         IpUe.IsEnabled = true;
//                         PortUe.IsEnabled = true;
//                         FreRobot.IsEnabled = true;
//                         FreTruss.IsEnabled = true;
//                         FreUe.IsEnabled = true;
//                         PortListener.IsEnabled = true;
//                     }
//                 });
//             }
//             private void LockAll(bool isLocked)
//             {
//                 Dispatcher.Invoke(() =>
//                 {
//                     if (isLocked)
//                     {
//                         // EGM运行时的锁定状态：只锁定输入框，按钮保持可用
//                         EgmStartButton.IsEnabled = false;    // 开始按钮禁用
//                         EgmStopButton.IsEnabled = false;      // 停止按钮保持可用
//                         RlListener.IsEnabled = false;         // RL监听按钮保持可用
//
//                         // 锁定所有输入框
//                         IpReinLearn.IsEnabled = false;
//                         PortReinLearn.IsEnabled = false;
//                         IpEgm.IsEnabled = false;
//                         PortEgm.IsEnabled = false;
//                         IpUe.IsEnabled = false;
//                         PortUe.IsEnabled = false;
//                         FreRobot.IsEnabled = false;
//                         FreTruss.IsEnabled = false;
//                         FreUe.IsEnabled = false;
//                         PortListener.IsEnabled = false;
//                     }
//                     else
//                     {
//                         // 完全解锁：所有按钮和输入框都可用
//                         EgmStartButton.IsEnabled = true;
//                         EgmStopButton.IsEnabled = true;
//                         RlListener.IsEnabled = true;
//                         
//                         // 归零按钮先不停
//                         // ZeroRefreshButton.IsEnabled = true;
//
//                         IpReinLearn.IsEnabled = true;
//                         PortReinLearn.IsEnabled = true;
//                         IpEgm.IsEnabled = true;
//                         PortEgm.IsEnabled = true;
//                         IpUe.IsEnabled = true;
//                         PortUe.IsEnabled = true;
//                         FreRobot.IsEnabled = true;
//                         FreTruss.IsEnabled = true;
//                         FreUe.IsEnabled = true;
//                         PortListener.IsEnabled = true;
//                     }
//                 });
//             }
//             private void InitializeRlValidityTimer()
//             {
//                 _rlValidityTimer = new System.Timers.Timer(1050); // 防止波动,改成1050ms
//                 _rlValidityTimer.Elapsed += (s, e) =>
//                 {
//                     if (_isRlListen)
//                     {
//                         _rlDataValid = false;
//         
//                         // 数据过期时，将所有目标速度置零
//                         lock (_bufferLock)
//                         {
//                             Array.Clear(_targetVelocities, 0, _targetVelocities.Length);
//                             _targetTrussX = 0;
//                             _targetTrussY = 0;
//                         }
//         
//                         if (_debug)
//                         {
//                             Console.WriteLine("[VDT]: RL端超时未发送新指令，已将目标速度和桁架命令清零。");
//                         }
//                     }
//                 };
//                 _rlValidityTimer.AutoReset = false; // 只执行一次
//             }
//             
//             // 初始化RL发送器
//             private bool InitializeRlSender()
//             {
//                 try
//                 {
//                     // 验证IP和端口
//                     string ip = IpReinLearn.Text?.Trim();
//                     if (!IsValidIP(ip))
//                     {
//                         Log.Error($"[RL发送器]: 无效的IP地址: {ip}");
//                         MessageBox.Show($"[RL发送器]: 无效的IP地址: {ip}");
//                         return false;
//                     }
//
//                     int? port = IsValidPort(PortReinLearn);
//                     if (port == null)
//                     {
//                         return false;
//                     }
//
//                     // 创建端点
//                     _rlEndpoint = new IPEndPoint(IPAddress.Parse(ip), port.Value);
//
//                     // 创建UDP客户端
//                     _udpRlSender = new UdpClient();
//                     _udpRlSender.Connect(_rlEndpoint);
//
//                     Log.Information($"[RL发送器]: 初始化成功 - {_rlEndpoint}");
//                     return true;
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[RL发送器]: 初始化失败: {ex.Message}");
//                     MessageBox.Show($"[RL发送器]: 初始化失败: {ex.Message}");
//                     return false;
//                 }
//             }
//
//             // 初始化UE发送器
//             private bool InitializeUeSender()
//             {
//                 try
//                 {
//                     // 验证IP和端口
//                     string ip = IpUe.Text?.Trim();
//                     if (!IsValidIP(ip))
//                     {
//                         Log.Error($"[UE发送器]: 无效的IP地址: {ip}");
//                         MessageBox.Show($"[UE发送器]: 无效的IP地址: {ip}");
//                         return false;
//                     }
//
//                     int? port = IsValidPort(PortUe);
//                     if (port == null)
//                     {
//                         return false;
//                     }
//
//                     // 创建端点
//                     _ueEndpoint = new IPEndPoint(IPAddress.Parse(ip), port.Value);
//
//                     // 创建UDP客户端
//                     _udpUeSender = new UdpClient();
//                     _udpUeSender.Connect(_ueEndpoint);
//
//                     Log.Information($"[UE发送器]: 初始化成功 - {_ueEndpoint}");
//                     return true;
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[UE发送器]: 初始化失败: {ex.Message}");
//                     MessageBox.Show($"[UE发送器]: 初始化失败: {ex.Message}");
//                     return false;
//                 }
//             }
//             
//             private void InitializePeriodicSender()
//             {
//                 try
//                 {
//                     // 先停止已有的定时器
//                     StopPeriodicSender();
//         
//                     // 创建定时器，但不立即启动
//                     _periodicSendTimer = new System.Threading.Timer(
//                         SendPeriodicData,    // 回调方法
//                         null,                // 状态对象
//                         Timeout.Infinite,    // 初始延迟（不启动）
//                         Timeout.Infinite     // 周期（不启动）
//                     );
//         
//                     Log.Information("[定时发送器]: 定时器已初始化（未启动）");
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[定时发送器]: 初始化失败: {ex.Message}");
//                 }
//             }
//             
//             // 启动定时发送
//             private void StartPeriodicSending()
//             {
//                 lock (_sendLock)
//                 {
//                     if (_periodicSendTimer == null)
//                     {
//                         Log.Warning("[定时发送器]: 定时器未初始化，无法启动");
//                         return;
//                     }
//         
//                     _isSendingEnabled = true;
//                     // 立即开始，然后每100ms执行一次
//                     
//                     int sendfreq = GetnumberFrom(FreUe);
//                     _periodicSendTimer.Change(0, sendfreq);
//                     Log.Information($"[定时发送器]: 已启动，间隔{sendfreq}ms");
//                 }
//             }
//
//             // 停止定时发送
//             private void StopPeriodicSender()
//             {
//                 lock (_sendLock)
//                 {
//                     _isSendingEnabled = false;
//         
//                     if (_periodicSendTimer != null)
//                     {
//                         _periodicSendTimer.Change(Timeout.Infinite, Timeout.Infinite);
//                         Log.Information("[定时发送器]: 已停止");
//                     }
//                 }
//             }
//             
//             // 释放定时器资源
//             private void DisposePeriodicSender()
//             {
//                 lock (_sendLock)
//                 {
//                     _isSendingEnabled = false;
//         
//                     if (_periodicSendTimer != null)
//                     {
//                         _periodicSendTimer.Dispose();
//                         _periodicSendTimer = null;
//                         Log.Information("[定时发送器]: 资源已释放");
//                     }
//                 }
//             }
//
//             // 定时发送回调方法
//             // private void SendPeriodicData(object state)
//             // {
//             //     // 检查是否允许发送
//             //     if (!_isSendingEnabled) return;
//             //
//             //     try
//             //     {
//             //         // 使用异步方法但不等待，避免阻塞定时器线程
//             //         _ = Task.Run(async () =>
//             //         {
//             //             try
//             //             {
//             //                 await SendCurrentDataToTargets();
//             //             }
//             //             catch (Exception ex)
//             //             {
//             //                 Log.Error($"[定时发送]: 发送数据时异常: {ex.Message}");
//             //             }
//             //         });
//             //     }
//             //     catch (Exception ex)
//             //     {
//             //         Log.Error($"[定时发送]: 定时器回调异常: {ex.Message}");
//             //     }
//             // }
//             private void SendPeriodicData(object state)
//             {
//                 if (!_isSendingEnabled) return;
//
//                 try
//                 {
//                     _ = Task.Run(async () =>
//                     {
//                         try
//                         {
//                             await SendCurrentDataToTargets();
//                         }
//                         catch (Exception ex)
//                         {
//                             Log.Error($"[定时发送]: 发送数据时异常: {ex.Message}");
//                             // 记录错误但不停止定时器
//                         }
//                     });
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[定时发送]: 定时器回调异常: {ex.Message}");
//                     // 即使发生异常，也不要停止定时器，让它继续运行
//                 }
//             }
//
//             // 原来的无参版本：内部调用 CollectCurrentData 再发送
//             private async Task SendCurrentDataToTargets()
//             {
//                 // 这里发送您需要的数据，例如当前关节位置、状态等
//                 string dataToSend = CollectCurrentData();
//
//                 await SendCurrentDataToTargets(dataToSend);
//             }
//             // 新增：直接发送指定的 JSON 字符串（用于 ERROR / READY 等特殊报文）
//             private async Task SendCurrentDataToTargets(string dataToSend)
//             {
//                 if (string.IsNullOrEmpty(dataToSend))
//                 {
//                     return;
//                 }
//
//                 // 发送到RL
//                 if (_udpRlSender != null && _rlEndpoint != null)
//                 {
//                     await SendToRl(Encoding.UTF8.GetBytes(dataToSend));
//                 }
//
//                 // 发送到UE
//                 if (_udpUeSender != null && _ueEndpoint != null)
//                 {
//                     await SendToUe(Encoding.UTF8.GetBytes(dataToSend));
//                 }
//             }
//
//
//             
//             public MainWindow()
//             {
//                 InitializeComponent();
//                 InitializeRlBlinkTimer(); // 添加这行
//                 InitializePeriodicSender(); // 添加这行
//                 
//                 _virtualTruss.Init();
//                 _virtualRobot.Init();
//             }
//
//         #endregion
//         
//         #region 整理后
//
//             // 1. 监听开启
//             public void InitializeListener()
//             {
//                 // 判断合法性，获取端口值，无效就退出
//                 int? port = IsValidPort(PortListener);
//                 if (port == null) return;
//
//                 // 先停止旧的监听，避免端口占用
//                 StopListener();
//
//                 try
//                 {
//                     _localCts = new CancellationTokenSource();
//
//                     // 创建底层Socket，允许地址复用
//                     var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
//                     {
//                         ExclusiveAddressUse = false
//                     };
//                     socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
//                     socket.Bind(new IPEndPoint(IPAddress.Any, port.Value));
//
//                     // 交给UdpClient托管
//                     _localListener = new UdpClient() { Client = socket };
//
//                     // 启动监听循环
//                     _ = Task.Run(() => ListenLoopAsync(_localCts.Token));
//                     
//                     // 等待100ms，让它彻底开启
//                     Thread.Sleep(100);
//                     Log.Information($"[UDP]: UDP监听端口已启动: {port.Value}");
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[UDP]: 启动监听失败: {ex.Message}");
//                     // 清理资源
//                     try { _localListener?.Close(); _localListener?.Dispose(); } catch { }
//                     _localListener = null;
//                     try { _localCts?.Cancel(); _localCts?.Dispose(); } catch { }
//                     _localCts = null;
//                 }
//             }
//             
//             // 2. RL监听启动
//             private void RlStart()
//             {
//                 try
//                 {
//                     _isRlListen = true;
//                     Log.Information("[RL]: RL监听已启动");
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[RL]: 启动RL监听失败: {ex.Message}");
//                     _isRlListen = false;
//                 }
//             }
//             
//             // 3. 监听关闭
//             public void StopListener()
//             {
//                 try
//                 {
//                     _localCts?.Cancel();
//                 }
//                 catch { /* ignore */ }
//                 finally
//                 {
//                     try
//                     {
//                         _localListener?.Close();
//                         _localListener?.Dispose();
//                     }
//                     catch { /* ignore */ }
//                     finally
//                     {
//                         _localListener = null;
//                     }
//
//                     try
//                     {
//                         _localCts?.Dispose();
//                     }
//                     catch { /* ignore */ }
//                     finally
//                     {
//                         _localCts = null;
//                     }
//                 }
//
//                 Log.Information("[UDP]: UDP监听已停止且已释放端口");
//             }
//             
//             // 4. 监听事件循环
//             private async Task ListenLoopAsync(CancellationToken token)
//             {
//                 while (!token.IsCancellationRequested)
//                 {
//                     try
//                     {
//                         var result = await _localListener.ReceiveAsync();
//                         var sender = result.RemoteEndPoint;
//                         var data = result.Buffer;
//
//                         _ = Task.Run(() => ProcessReceivedData(data, sender));
//                     }
//                     catch (ObjectDisposedException)
//                     {
//                         break; // 正常退出
//                     }
//                     catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
//                     {
//                         break; // 取消令牌触发的中断
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Error($"[UDP]: UDP监听异常: {ex.Message}");
//                         await Task.Delay(100, token);
//                     }
//                 }
//                 Log.Information("[UDP]: 监听循环已退出");
//             }
//             
//             // 5. 具体UDP包的信号处理
//             private void ProcessReceivedData(byte[] data, IPEndPoint sender)
//             {
//                 try
//                 {
//                     try
//                     {
//                         // 如果能被解析那就是egm
//                         EgmRobot robotMessage = EgmRobot.Parser.ParseFrom(data);
//                         ProcessEgmMessage(robotMessage, sender);
//                         return;
//                     }
//                     catch { /* */ }
//                 
//                     try
//                     {
//                         if (!_isRlListen) 
//                         {
//                             // 如果RL监听未开启，直接跳过JSON解析
//                             return;
//                         }
//                         
//                         // 仅有RL开启后才解析,且仅解析RL
//                         string jsonString = Encoding.UTF8.GetString(data);
//                         var jsonMessage = JsonConvert.DeserializeObject<dynamic>(jsonString);
//
//                         if (jsonMessage != null && jsonMessage.Header != null)
//                         {
//                             if (jsonMessage.Header.ToString() == "RL" && jsonMessage.Type.ToString()=="POSE")
//                             {
//                                 TriggerRlBlink();  // 触发闪烁效果
//                                 ProcessPoseMessage(jsonMessage, jsonMessage.TimeStamp?.ToString());
//                             }
//                             if (jsonMessage.Header.ToString() == "RL" && jsonMessage.Type.ToString()=="CONTROL")
//                             {
//                                 Dispatcher.Invoke(() =>
//                                 {
//                                     RlConnection.Fill = new SolidColorBrush(Colors.Red); // 变绿色
//                                 });
//                                 ProcessControlMessage(jsonMessage, jsonMessage.TimeStamp?.ToString());
//                             }
//                             return; // 如果是JSON消息，处理完后直接返回
//                         }
//                     }
//                     catch { /* */ }
//
//                     // 如果既不是EGM消息也不是JSON消息
//                     Log.Warning($"[UDP]: 收到无法识别的消息格式，来自 {sender.Address}:{sender.Port}，数据长度: {data.Length}字节");
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[UDP]: 处理接收数据时发生异常: {ex.Message}");
//                 }
//             }
//             
//             // 6. 姿态数据处理
//             private void ProcessPoseMessage(dynamic message, string timeStampStr)
//             {
//                 try
//                 {
//                     // 1. 时间戳检查
//                     if (string.Compare(timeStampStr, _lastRlTimestamp, StringComparison.Ordinal) <= 0)
//                     {
//                         if (_debug)
//                         {
//                             Console.WriteLine($"[重复]: 忽略重复或旧的时间戳: {timeStampStr} (上次: {_lastRlTimestamp})");
//                         }
//                         return;
//                     }
//
//                     _lastRlTimestamp = timeStampStr;
//
//                     // 2. 更新目标速度 + 桁架位置
//                     lock (_bufferLock)
//                     {
//                         // 解析关节速度指令
//                         for (int i = 1; i <= 6; i++)
//                         {
//                             string raxKey = $"Rax{i}";
//                             if (message[raxKey] != null)
//                             {
//                                 _targetVelocities[i - 1] = (double)message[raxKey];
//                             }
//                             else
//                             {
//                                 _targetVelocities[i - 1] = 0.0; // 没给就当 0
//                             }
//                         }
//
//                         // 解析桁架速度并计算目标位置
//                         double trussVelocityX = message["TrussX"] != null ? (double)message["TrussX"] : 0.0;
//                         double trussVelocityY = message["TrussY"] != null ? (double)message["TrussY"] : 0.0;
//             
//                         // 保存速度（用于后续作为速度指令）
//                         _targetTrussVx = trussVelocityX;
//                         _targetTrussVy = trussVelocityY;
//                         
//                         // 计算1050ms内的目标位置
//                         double deltaTime = 1.05; // 1050ms = 1.05秒
//                         _targetTrussX = _currentTrussX + trussVelocityX * deltaTime;
//                         _targetTrussY = _currentTrussY + trussVelocityY * deltaTime;
//             
//                         // 应用桁架位置限制
//                         _targetTrussX = Math.Max(_axisLimits[6][0], Math.Min(_axisLimits[6][1], _targetTrussX));
//                         _targetTrussY = Math.Max(_axisLimits[7][0], Math.Min(_axisLimits[7][1], _targetTrussY));
//                     }
//
//                     // 3. 重启 RL 有效期计时器（1050ms 超时清零）
//                     _rlDataValid = true;
//
//                     // 确保定时器已初始化
//                     if (_rlValidityTimer != null)
//                     {
//                         _rlValidityTimer.Stop();  // 重新计时
//                         _rlValidityTimer.Start();
//                     }
//                     else
//                     {
//                         Log.Warning("[POS]: RL有效期定时器未初始化");
//                     }
//
//                     Log.Information(
//                         $"[POS]: 新指令(时间戳:{timeStampStr}): " +
//                         $"J1:{_targetVelocities[0]:F4}, J2:{_targetVelocities[1]:F4}, " +
//                         $"J3:{_targetVelocities[2]:F4}, J4:{_targetVelocities[3]:F4}, " +
//                         $"J5:{_targetVelocities[4]:F4}, J6:{_targetVelocities[5]:F4}, " +
//                         $"TrussX:{_targetTrussX:F4}, TrussY:{_targetTrussY:F4}");
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[POS]: 解析POSE消息失败: {ex.Message}");
//                 }
//             }
//             
//             // 7. 对EGM的消息拆解
//             private void ProcessEgmMessage(EgmRobot robotMessage, IPEndPoint sender)
//             {
//                 try
//                 {
//                     _robotEndpoint = sender;
//                     if (robotMessage?.FeedBack?.Joints != null)
//                     {
//                         var joints = robotMessage.FeedBack.Joints;
//         
//                         // 更新关节位置到长度为6的数组
//                         for (int i = 0; i < 6; i++)
//                         {
//                             _currentJointPositions[i] = joints.Joints[i];  // 直接取double值
//                         }
//
//                         if (_debug)
//                         {
//                             Console.WriteLine($"[EGM]: 当前关节位置: J1={_currentJointPositions[0]:F2}, J2={_currentJointPositions[1]:F2}, " +
//                                               $"J3={_currentJointPositions[2]:F2}, J4={_currentJointPositions[3]:F2}, " +
//                                               $"J5={_currentJointPositions[4]:F2}, J6={_currentJointPositions[5]:F2}");
//                         }
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Warning($"[EGM]: 解析EGM消息失败: {ex.Message}");
//                 }
//             }
//             
//             // 8. 核心发送控制
//             // private async Task RunControlLoop()
//             // {
//             //     try
//             //     {
//             //         // 一次性在UI线程获取频率值
//             //         int egmfreq = await Dispatcher.InvokeAsync(() => GetnumberFrom(FreRobot)).Task;
//             //         int trussfreq = await Dispatcher.InvokeAsync(() => GetnumberFrom(FreTruss)).Task;
//             //
//             //         _controlCts = new CancellationTokenSource();
//             //         _egmStart = true;
//             //
//             //         // 启动前重置错误相关状态
//             //         _egmPausedByError = false;
//             //         _devicesNormalLast = true;
//             //         _errorReported = false;
//             //
//             //         Log.Information(string.Format("[LOP]: 开始运行，机器人频率: {0}ms，桁架频率: {1}ms", egmfreq, trussfreq));
//             //
//             //         // 初始化双缓冲与积分初值
//             //         InitializeDoubleBuffer();
//             //         lock (_bufferLock)
//             //         {
//             //             Array.Copy(_currentJointPositions, _integratedPositions, 6);
//             //         }
//             //
//             //         DateTime lastTrussControlTime = DateTime.Now;
//             //
//             //         // 主控制循环
//             //         while (_egmStart && !_controlCts.Token.IsCancellationRequested)
//             //         {
//             //             var stopwatch = Stopwatch.StartNew();
//             //
//             //             try
//             //             {
//             //                 int robotStatus = 0;
//             //                 int plcStatus = 0;
//             //
//             //                 // 异步获取设备状态，避免阻塞积分
//             //                 Task<bool> devicesTask = Task.Run<bool>(
//             //                     delegate
//             //                     {
//             //                         robotStatus = _virtualRobot.GetVirRobotStatus();
//             //                         plcStatus = _virtualTruss.GetVirPlcStatus();
//             //                         return (robotStatus == 4 && plcStatus == 3);
//             //                     });
//             //
//             //                 // 并行执行积分计算
//             //                 Task updateTask = Task.Run(
//             //                     delegate
//             //                     {
//             //                         UpdateIntegratedPositions(egmfreq);
//             //                     });
//             //
//             //                 await Task.WhenAll(devicesTask, updateTask);
//             //
//             //                 bool devicesNormal = devicesTask.Result;
//             //
//             //                 // === 新增：从“正常”变为“异常”的瞬间，触发错误暂停逻辑 ===
//             //                 if (!devicesNormal && _devicesNormalLast && !_egmPausedByError)
//             //                 {
//             //                     _egmPausedByError = true;
//             //
//             //                     Log.Warning(string.Format(
//             //                         "[EGM]: 检测到设备异常，机器人状态={0}，桁架状态={1}，暂停EGM发送并上报 ERROR",
//             //                         robotStatus, plcStatus));
//             //
//             //                     // 停止控制循环，但保留监听和定时上报
//             //                     _egmStart = false;
//             //                     if (_controlCts != null)
//             //                     {
//             //                         _controlCts.Cancel();
//             //                     }
//             //
//             //                     try
//             //                     {
//             //                         // 调用一次 CollectCurrentData，利用原有逻辑生成 Error 报文
//             //                         string errorJson = CollectCurrentData();
//             //                         if (!string.IsNullOrEmpty(errorJson))
//             //                         {
//             //                             await SendCurrentDataToTargets(errorJson);
//             //                         }
//             //                     }
//             //                     catch (Exception ex)
//             //                     {
//             //                         Log.Error("[EGM]: 发送错误状态报文失败: " + ex.Message);
//             //                     }
//             //
//             //                     // 跳出循环，停止给机器人和桁架发控制指令
//             //                     break;
//             //                 }
//             //
//             //                 // 记录本轮状态，用于下一轮做边沿判断
//             //                 _devicesNormalLast = devicesNormal;
//             //
//             //                 // 只有在设备正常且有机器人连接时才发送指令
//             //                 if (devicesNormal && _robotEndpoint != null)
//             //                 {
//             //                     double[] targetPositions = GetLatestTargetPositions();
//             //                     // 不等待发送完成，继续下一循环
//             //                     _ = SendJointMessageToRobot(targetPositions);
//             //                 }
//             //
//             //                 // 桁架控制
//             //                 DateTime currentTime = DateTime.Now;
//             //                 if (devicesNormal && (currentTime - lastTrussControlTime).TotalMilliseconds >= trussfreq)
//             //                 {
//             //                     _ = SendTrussMessageToPlc();
//             //                     lastTrussControlTime = currentTime;
//             //                 }
//             //
//             //                 // 简单性能监控
//             //                 long elapsed = stopwatch.ElapsedMilliseconds;
//             //                 if (elapsed > egmfreq && _sendCount % 100 == 0)
//             //                 {
//             //                     Log.Warning(string.Format("[性能]: 发送耗时 {0}ms 超过预期周期 {1}ms", elapsed, egmfreq));
//             //                 }
//             //
//             //                 _sendCount++;
//             //             }
//             //             catch (Exception ex)
//             //             {
//             //                 Log.Error("[LOP]: 发送指令时异常: " + ex.Message);
//             //             }
//             //
//             //             // 计算需要等待的时间，保证接近 egmfreq 周期
//             //             long elapsedMs = stopwatch.ElapsedMilliseconds;
//             //             int remainingTime = egmfreq - (int)elapsedMs;
//             //             if (remainingTime > 0)
//             //             {
//             //                 await Task.Delay(remainingTime);
//             //             }
//             //             else
//             //             {
//             //                 // 至少让出一点时间，避免完全占满CPU
//             //                 await Task.Delay(1);
//             //             }
//             //         }
//             //
//             //         Log.Information("[LOP]: 控制循环已结束");
//             //     }
//             //     catch (Exception ex)
//             //     {
//             //         Log.Error("[LOP]: 控制循环异常退出: " + ex.Message);
//             //     }
//             // }
//             
//             private async Task RunControlLoop()
// {
//     try
//     {
//         // 一次性在UI线程获取频率值
//         int egmfreq = await Dispatcher.InvokeAsync(() => GetnumberFrom(FreRobot)).Task;
//         int trussfreq = await Dispatcher.InvokeAsync(() => GetnumberFrom(FreTruss)).Task;
//
//         _controlCts = new CancellationTokenSource();
//         _egmStart = true;
//
//         Log.Information(string.Format("[LOP]: 开始运行，机器人频率: {0}ms，桁架频率: {1}ms", egmfreq, trussfreq));
//
//         // 初始化积分缓存
//         InitializeDoubleBuffer();
//         Array.Copy(_currentJointPositions, _integratedPositions, 6);
//
//         DateTime lastTrussControlTime = DateTime.Now;
//         Stopwatch stopwatch = Stopwatch.StartNew();
//
//         while (_egmStart && !_controlCts.Token.IsCancellationRequested)
//         {
//             stopwatch.Restart();
//
//             try
//             {
//                 // —— 1. 同步获取设备状态 ——
//                 int robotStatus = _virtualRobot.GetVirRobotStatus();
//                 int plcStatus = _virtualTruss.GetVirPlcStatus();
//
//                 bool robotOk = (robotStatus == 4);
//                 bool plcOk = (plcStatus == 3);
//
//                 // —— 2. 积分关节位置（传入频率参数）——
//                 UpdateIntegratedPositions(egmfreq);  // ← 传入频率值
//
//                 // —— 3. 给机器人发 EGM ——
//                 if (robotOk && _robotEndpoint != null)
//                 {
//                     double[] targetPositions = GetLatestTargetPositions();
//                     await SendJointMessageToRobot(targetPositions);
//                 }
//
//                 // —— 4. 控制桁架 ——
//                 DateTime currentTime = DateTime.Now;
//                 if (plcOk && (currentTime - lastTrussControlTime).TotalMilliseconds >= trussfreq)
//                 {
//                     await SendTrussMessageToPlc();
//                     lastTrussControlTime = currentTime;
//                 }
//
//                 // —— 5. 性能监控 ——
//                 long elapsed = stopwatch.ElapsedMilliseconds;
//                 if (elapsed > egmfreq && _sendCount % 100 == 0)
//                 {
//                     Log.Warning(string.Format("[性能]: 发送耗时 {0}ms 超过预期周期 {1}ms", elapsed, egmfreq));
//                 }
//
//                 _sendCount++;
//             }
//             catch (Exception ex)
//             {
//                 Log.Error(string.Format("[LOP]: 发送指令时异常: {0}", ex.Message));
//             }
//
//             // —— 6. 等到下一个周期 —— 
//             long elapsedMs = stopwatch.ElapsedMilliseconds;
//             int remainingTime = egmfreq - (int)elapsedMs;
//             if (remainingTime > 0)
//             {
//                 try
//                 {
//                     await Task.Delay(remainingTime, _controlCts.Token);
//                 }
//                 catch (TaskCanceledException)
//                 {
//                     // 正常退出
//                 }
//             }
//             else
//             {
//                 await Task.Delay(1);
//             }
//         }
//     }
//     catch (OperationCanceledException)
//     {
//         Log.Information("[LOP]: 控制循环被正常取消");
//     }
//     catch (Exception ex)
//     {
//         Log.Error(string.Format("[LOP]: 控制循环异常退出: {0}", ex.Message));
//     }
//     finally
//     {
//         _egmStart = false;
//     }
// }
//             
//             private async Task HandleDeviceStatusChange(int currentRobotStatus, int currentPlcStatus, 
//                 int lastRobotStatus, int lastPlcStatus)
//             {
//                 try
//                 {
//                     // 只有在状态从正常变为异常时才处理
//                     bool wasNormal = (lastRobotStatus == 4 && lastPlcStatus == 3);
//                     bool isAbnormal = (currentRobotStatus != 4 || currentPlcStatus != 3);
//         
//                     if (wasNormal && isAbnormal && !_egmPausedByError)
//                     {
//                         _egmPausedByError = true;
//             
//                         Log.Warning($"[状态监控]: 设备状态异常 - 机器人:{currentRobotStatus}, 桁架:{currentPlcStatus}");
//             
//                         // 发送ERROR报文
//                         var errorData = new
//                         {
//                             Header = "ERROR",
//                             Timestamp = DateTime.Now.ToString("yyMMddHHmmssfff"),
//                             RobotStatus = currentRobotStatus,
//                             TrussStatus = currentPlcStatus,
//                             Message = "设备状态异常，等待恢复指令"
//                         };
//             
//                         string errorJson = JsonConvert.SerializeObject(errorData);
//                         await SendCurrentDataToTargets(errorJson);
//             
//                         // 停止控制循环但保持监听
//                         _egmStart = false;
//                         _controlCts?.Cancel();
//             
//                         Log.Information("[状态监控]: 已暂停EGM发送，等待CONTROL恢复指令");
//                     }
//         
//                     // 状态从异常恢复为正常
//                     bool wasAbnormal = (lastRobotStatus != 4 || lastPlcStatus != 3);
//                     bool isNormal = (currentRobotStatus == 4 && currentPlcStatus == 3);
//         
//                     if (wasAbnormal && isNormal && _egmPausedByError)
//                     {
//                         _egmPausedByError = false;
//                         Log.Information("[状态监控]: 设备状态已恢复正常");
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[状态监控]: 处理状态变化异常: {ex.Message}");
//                 }
//             }
//
//
//             
//             // 9. 积分更新机制
//             // private void UpdateIntegratedPositions(int cycleTimeMs)
//             // {
//             //     try
//             //     {
//             //         double dt = cycleTimeMs / 1000.0; // 转换为秒
//             //
//             //         lock (_bufferLock)
//             //         {
//             //             // 对6个关节进行速度积分并应用位置限制
//             //             for (int i = 0; i < 6; i++)
//             //             {
//             //                 _integratedPositions[i] += _targetVelocities[i] * dt;
//             //                 
//             //                 // 应用位置限制
//             //                 _integratedPositions[i] = Math.Max(_axisLimits[i][0]+3, 
//             //                                                   Math.Min(_axisLimits[i][1]-3, _integratedPositions[i]));
//             //             }
//             //
//             //             // 桁架位置积分（如果桁架也是通过速度控制的话）
//             //             // 注意：根据你的实际控制方式调整
//             //             // 如果桁架是直接位置控制，则不需要积分
//             //             
//             //             // 将积分后的位置写入当前写缓冲区
//             //             Array.Copy(_integratedPositions, _positionBuffers[_currentWriteBuffer], 6);
//             //
//             //             // 交换缓冲区，使新数据可用
//             //             _currentWriteBuffer = 1 - _currentWriteBuffer;
//             //             _newDataAvailable = true;
//             //
//             //             // 调试输出和越界警告
//             //             if (_debug)
//             //             {
//             //                 bool anyJointAtLimit = false;
//             //                 StringBuilder limitWarning = new StringBuilder();
//             //                 
//             //                 // 检查关节限制
//             //                 for (int i = 0; i < 6; i++)
//             //                 {
//             //                     if (_integratedPositions[i] <= _axisLimits[i][0] + 1.0 || 
//             //                         _integratedPositions[i] >= _axisLimits[i][1] - 1.0)
//             //                     {
//             //                         anyJointAtLimit = true;
//             //                         limitWarning.Append($"J{i+1}:{_integratedPositions[i]:F2}; ");
//             //                     }
//             //                 }
//             //                 
//             //                 if (anyJointAtLimit)
//             //                 {
//             //                     Console.WriteLine($"[积分]: 关节接近极限位置 - {limitWarning}");
//             //                 }
//             //                 else
//             //                 {
//             //                     Console.WriteLine($"[积分]: 更新位置 - J1:{_integratedPositions[0]:F4}, J2:{_integratedPositions[1]:F4}, " +
//             //                              $"J3:{_integratedPositions[2]:F4}, J4:{_integratedPositions[3]:F4}, " +
//             //                              $"J5:{_integratedPositions[4]:F4}, J6:{_integratedPositions[5]:F4}");
//             //                 }
//             //             }
//             //         }
//             //     }
//             //     catch (Exception ex)
//             //     {
//             //         Log.Error($"[积分]: 更新积分位置时发生异常: {ex.Message}");
//             //     }
//             // }
//             
//             // private void UpdateIntegratedPositions(int cycleTimeMs)
//             // {
//             //     try
//             //     {
//             //         double dt = cycleTimeMs / 1000.0;
//             //
//             //         // 快速复制数据到局部变量
//             //         double[] localVelocities;
//             //         double[] localIntegrated;
//             //
//             //         lock (_bufferLock)
//             //         {
//             //             localVelocities = (double[])_targetVelocities.Clone();
//             //             localIntegrated = (double[])_integratedPositions.Clone();
//             //         }
//             //
//             //         bool hasMovement = false;
//             //
//             //         // 在锁外进行计算
//             //         for (int i = 0; i < 6; i++)
//             //         {
//             //             double newPosition = localIntegrated[i] + localVelocities[i] * dt;
//             //
//             //             // 检查是否有有效运动
//             //             if (Math.Abs(localVelocities[i]) > 0.001)
//             //             {
//             //                 hasMovement = true;
//             //             }
//             //
//             //             // 应用位置限制
//             //             newPosition = Math.Max(_axisLimits[i][0] + 3, 
//             //                 Math.Min(_axisLimits[i][1] - 3, newPosition));
//             //             localIntegrated[i] = newPosition;
//             //         }
//             //
//             //         // 快速更新缓冲区
//             //         lock (_bufferLock)
//             //         {
//             //             Array.Copy(localIntegrated, _integratedPositions, 6);
//             //             Array.Copy(localIntegrated, _positionBuffers[_currentWriteBuffer], 6);
//             //             _currentWriteBuffer = 1 - _currentWriteBuffer;
//             //             _newDataAvailable = true;
//             //         }
//             //
//             //         // 调试输出
//             //         if (_debug && hasMovement && _sendCount % 50 == 0)
//             //         {
//             //             Console.WriteLine($"[积分]: 更新位置 - J1:{localIntegrated[0]:F4}(v:{localVelocities[0]:F4}), " +
//             //                               $"J2:{localIntegrated[1]:F4}(v:{localVelocities[1]:F4}), " +
//             //                               $"J3:{localIntegrated[2]:F4}(v:{localVelocities[2]:F4})");
//             //         }
//             //     }
//             //     catch (Exception ex)
//             //     {
//             //         Log.Error($"[积分]: 更新积分位置时发生异常: {ex.Message}");
//             //     }
//             // }
//             
//             private void UpdateIntegratedPositions(int cycleTimeMs)
// {
//     try
//     {
//         // 使用传入的频率值，不再从UI获取
//         double fixedDt = cycleTimeMs / 1000.0; // 转换为秒
//
//         double[] localVelocities;
//         double[] localIntegrated;
//
//         lock (_bufferLock)
//         {
//             localVelocities = (double[])_targetVelocities.Clone();
//             localIntegrated = (double[])_integratedPositions.Clone();
//         }
//
//         bool hasMovement = false;
//
//         // 积分计算
//         for (int i = 0; i < 6; i++)
//         {
//             double positionChange = localVelocities[i] * fixedDt;
//             
//             if (Math.Abs(positionChange) > 0.0001)
//             {
//                 hasMovement = true;
//             }
//
//             double newPosition = localIntegrated[i] + positionChange;
//             
//             // 应用位置限制
//             newPosition = Math.Max(_axisLimits[i][0] + 1.0, 
//                                   Math.Min(_axisLimits[i][1] - 1.0, newPosition));
//             localIntegrated[i] = newPosition;
//         }
//
//         // 原子性地更新所有状态
//         lock (_bufferLock)
//         {
//             Array.Copy(localIntegrated, _integratedPositions, 6);
//             Array.Copy(localIntegrated, _positionBuffers[_currentWriteBuffer], 6);
//             
//             int newReadBuffer = _currentWriteBuffer;
//             _currentWriteBuffer = 1 - _currentWriteBuffer;
//             _currentReadBuffer = newReadBuffer;
//             _newDataAvailable = true;
//         }
//
//         // 调试输出
//         if (_debug && hasMovement && _sendCount % 20 == 0)
//         {
//             Console.WriteLine($"[积分]: 固定dt={fixedDt:F4}s | " +
//                             $"J1:{localIntegrated[0]:F2}°(v:{localVelocities[0]:F2}°/s) | " +
//                             $"J2:{localIntegrated[1]:F2}°(v:{localVelocities[1]:F2}°/s) | " +
//                             $"J3:{localIntegrated[2]:F2}°(v:{localVelocities[2]:F2}°/s)");
//             
//             // 检查是否接近极限位置
//             for (int i = 0; i < 6; i++)
//             {
//                 if (Math.Abs(localIntegrated[i] - _axisLimits[i][0]) < 2.0 ||
//                     Math.Abs(localIntegrated[i] - _axisLimits[i][1]) < 2.0)
//                 {
//                     Console.WriteLine($"[警告]: 关节{i+1}接近极限位置: {localIntegrated[i]:F2}° (限制: {_axisLimits[i][0]}~{_axisLimits[i][1]})");
//                 }
//             }
//         }
//     }
//     catch (Exception ex)
//     {
//         Log.Error($"[积分]: 更新积分位置时发生异常: {ex.Message}");
//     }
// }
//             
//             public static class SimpleMemoryStreamPool
//             {
//                 private static readonly Queue<MemoryStream> _pool = new Queue<MemoryStream>();
//                 private static readonly object _lock = new object();
//     
//                 public static MemoryStream Get()
//                 {
//                     lock (_lock)
//                     {
//                         if (_pool.Count > 0)
//                         {
//                             var stream = _pool.Dequeue();
//                             stream.SetLength(0);
//                             return stream;
//                         }
//                     }
//                     return new MemoryStream(1024); // 预分配大小
//                 }
//     
//                 public static void Return(MemoryStream stream)
//                 {
//                     if (stream.Capacity <= 4096) // 只缓存小尺寸流
//                     {
//                         stream.SetLength(0);
//                         lock (_lock)
//                         {
//                             _pool.Enqueue(stream);
//                         }
//                     }
//                 }
//             }
//
//             // // 使用方式：
//             // private async Task SendJointMessageToRobot(double[] joints)
//             // {
//             //     if (_robotEndpoint == null || joints.Length != 6) return;
//             //
//             //     MemoryStream memoryStream = null;
//             //     try
//             //     {
//             //         memoryStream = SimpleMemoryStreamPool.Get();
//             //         var sensorMessage = new EgmSensor();
//             //
//             //         CreateJointMessage(sensorMessage, joints);
//             //         sensorMessage.WriteTo(memoryStream);
//             //
//             //         var data = memoryStream.ToArray();
//             //         await _localListener.SendAsync(data, data.Length, _robotEndpoint);
//             //     }
//             //     catch (Exception ex)
//             //     {
//             //         Log.Error($"[EGM]: 发送关节控制消息失败: {ex.Message}");
//             //     }
//             //     finally
//             //     {
//             //         if (memoryStream != null)
//             //         {
//             //             SimpleMemoryStreamPool.Return(memoryStream);
//             //         }
//             //     }
//             // }
//             
//             private async Task SendToRl(byte[] data)
//             {
//                 try
//                 {
//                     if (_udpRlSender != null && _rlEndpoint != null)
//                     {
//                         await _udpRlSender.SendAsync(data, data.Length);
//                         Log.Debug($"[RL发送]: 已发送 {data.Length} 字节到 {_rlEndpoint}");
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[RL发送]: 发送失败: {ex.Message}");
//                 }
//             }
//             
//             private async Task SendToUe(byte[] data)
//             {
//                 try
//                 {
//                     if (_udpUeSender != null && _ueEndpoint != null)
//                     {
//                         await _udpUeSender.SendAsync(data, data.Length);
//                         Log.Debug($"[UE发送]: 已发送 {data.Length} 字节到 {_ueEndpoint}");
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[UE发送]: 发送失败: {ex.Message}");
//                 }
//             }
//             
//             // 修改状态文本方法，返回文本和颜色
//             private static (string text, SolidColorBrush color) GetRobotStatusInfo(int status)
//             {
//                 switch (status)
//                 {
//                     case -1: return ("异常状态", BrushRed);   // 红色
//                     case 1:  return ("离线状态", BrushGry);   // 灰色
//                     case 2:  return ("手动状态", BrushYel);   // 黄色
//                     case 3:  return ("电机下电", BrushGre);   // 绿色
//                     case 4:  return ("正常控制", BrushGre);   // 绿色
//                     default: return ("未知状态", BrushGry);
//                 }
//             }
//
//
//             private static (string text, SolidColorBrush color) GetPlcStatusInfo(int status)
//             {
//                 switch (status)
//                 {
//                     case -1: return ("异常状态", BrushRed);   // 红色
//                     case 1:  return ("离线状态", BrushGry);   // 灰色
//                     case 2:  return ("手动状态", BrushYel);   // 黄色
//                     case 3:  return ("正常控制", BrushGre);   // 绿色
//                     default: return ("未知状态", BrushGry);
//                 }
//             }
//             
//             // // 收集当前数据（根据您的需求实现）
//             // private string CollectCurrentData()
//             // {
//             //     try
//             //     {
//             //         int robotStatus = _virtualRobot.GetVirRobotStatus();
//             //         int plcStatus   = _virtualTruss.GetVirPlcStatus();
//             //
//             //         bool isRunning = robotStatus == 4 && plcStatus == 3;
//             //
//             //         if (isRunning)
//             //         {
//             //             var (trussX, trussY, speedX, speedY) = _virtualTruss.GetTrussPose();
//             //             _currentTrussX = trussX;
//             //             _currentTrussY = trussY;
//             //
//             //             var (
//             //                 otherx, othery, otherz,
//             //                 otherq1, otherq2, otherq3, otherq4,
//             //                 currentj1, currentj2, currentj3,
//             //                 currentj4, currentj5, currentj6
//             //             ) = _virtualRobot.GetCurrentPose();
//             //
//             //             var (robotText, robotColor) = GetRobotStatusInfo(robotStatus);
//             //             var (plcText, plcColor) = GetPlcStatusInfo(plcStatus);
//             //
//             //             Dispatcher.Invoke(() =>
//             //             {
//             //                 AbbJ1.Text = currentj1.ToString("F3");
//             //                 AbbJ2.Text = currentj2.ToString("F3");
//             //                 AbbJ3.Text = currentj3.ToString("F3");
//             //                 AbbJ4.Text = currentj4.ToString("F3");
//             //                 AbbJ5.Text = currentj5.ToString("F3");
//             //                 AbbJ6.Text = currentj6.ToString("F3");
//             //
//             //                 PlcX.Text = trussX.ToString("F3");
//             //                 PlcY.Text = trussY.ToString("F3");
//             //
//             //                 AbbStatusText.Text = robotText;
//             //                 AbbStatusText.Foreground = robotColor;
//             //
//             //                 PlcStatusText.Text = plcText;
//             //                 PlcStatusText.Foreground = plcColor;
//             //             });
//             //
//             //             var data = new
//             //             {
//             //                 Header    = "Normal",
//             //                 Timestamp = DateTime.Now.ToString("yyMMddHHmmssfff"),
//             //                 J1 = _currentJointPositions[0],
//             //                 J2 = _currentJointPositions[1],
//             //                 J3 = _currentJointPositions[2],
//             //                 J4 = _currentJointPositions[3],
//             //                 J5 = _currentJointPositions[4],
//             //                 J6 = _currentJointPositions[5],
//             //                 TrussX = trussX,
//             //                 TrussY = trussY,
//             //                 VirRobotStatus = robotStatus == 4 ? "Running" : "Stopped",
//             //                 VirPlcStatus   = plcStatus   == 3 ? "Running" : "Stopped"
//             //             };
//             //
//             //             _errorReported = false;
//             //             return JsonConvert.SerializeObject(data);
//             //         }
//             //
//             //         if (!_errorReported)
//             //         {
//             //             var data = new
//             //             {
//             //                 Header       = "Error",
//             //                 Timestamp    = DateTime.Now.ToString("yyMMddHHmmssfff"),
//             //                 RobotStatus  = robotStatus,
//             //                 TrussStatus  = plcStatus,
//             //                 ControlStatus = "Stopped"
//             //             };
//             //
//             //             _errorReported = true;
//             //             Log.Error($"机器人死了: {robotStatus}，桁架死了: {plcStatus}");
//             //             return JsonConvert.SerializeObject(data);
//             //         }
//             //
//             //         return null;
//             //     }
//             //     catch (Exception ex)
//             //     {
//             //         Log.Error($"[数据收集]: 收集数据失败: {ex.Message}");
//             //
//             //         var errorData = new
//             //         {
//             //             Header       = "Error",
//             //             Timestamp    = DateTime.Now.ToString("yyMMddHHmmssfff"),
//             //             ControlStatus = "Error"
//             //         };
//             //
//             //         _errorReported = true;
//             //         return JsonConvert.SerializeObject(errorData);
//             //     }
//             // }
//             
//             // 修改 CollectCurrentData 方法，增加更细粒度的异常处理
//             private string CollectCurrentData()
//             {
//                 try
//                 {
//                     int robotStatus = -1;
//                     int plcStatus = -1;
//                     double trussX = 0, trussY = 0, speedX = 0, speedY = 0;
//                     double currentj1 = 0, currentj2 = 0, currentj3 = 0, currentj4 = 0, currentj5 = 0, currentj6 = 0;
//
//                     // 分别获取设备状态，避免一个失败影响另一个
//                     try
//                     {
//                         robotStatus = _virtualRobot.GetVirRobotStatus();
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Error($"[数据收集]: 获取机器人状态失败: {ex.Message}");
//                         robotStatus = -1; // 标记为异常状态
//                     }
//
//                     try
//                     {
//                         plcStatus = _virtualTruss.GetVirPlcStatus();
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Error($"[数据收集]: 获取桁架状态失败: {ex.Message}");
//                         plcStatus = -1; // 标记为异常状态
//                     }
//
//                     bool isRunning = robotStatus == 4 && plcStatus == 3;
//
//                     if (isRunning)
//                     {
//                         // 分别获取桁架和机器人数据，避免相互影响
//                         try
//                         {
//                             (trussX, trussY, speedX, speedY) = _virtualTruss.GetTrussPose();
//                             _currentTrussX = trussX;
//                             _currentTrussY = trussY;
//                         }
//                         catch (Exception ex)
//                         {
//                             Log.Error($"[数据收集]: 获取桁架位置失败: {ex.Message}");
//                             // 使用上一次的值或默认值
//                         }
//
//                         try
//                         {
//                             var (otherx, othery, otherz, otherq1, otherq2, otherq3, otherq4, 
//                                  j1, j2, j3, j4, j5, j6) = _virtualRobot.GetCurrentPose();
//                             currentj1 = j1; currentj2 = j2; currentj3 = j3;
//                             currentj4 = j4; currentj5 = j5; currentj6 = j6;
//                         }
//                         catch (Exception ex)
//                         {
//                             Log.Error($"[数据收集]: 获取机器人位姿失败: {ex.Message}");
//                             // 使用当前关节位置作为备选
//                             lock (_bufferLock)
//                             {
//                                 currentj1 = _currentJointPositions[0];
//                                 currentj2 = _currentJointPositions[1];
//                                 currentj3 = _currentJointPositions[2];
//                                 currentj4 = _currentJointPositions[3];
//                                 currentj5 = _currentJointPositions[4];
//                                 currentj6 = _currentJointPositions[5];
//                             }
//                         }
//
//                         // 更新界面 - 无论数据获取是否成功都要更新状态显示
//                         var (robotText, robotColor) = GetRobotStatusInfo(robotStatus);
//                         var (plcText, plcColor) = GetPlcStatusInfo(plcStatus);
//
//                         Dispatcher.Invoke(() =>
//                         {
//                             AbbJ1.Text = currentj1.ToString("F3");
//                             AbbJ2.Text = currentj2.ToString("F3");
//                             AbbJ3.Text = currentj3.ToString("F3");
//                             AbbJ4.Text = currentj4.ToString("F3");
//                             AbbJ5.Text = currentj5.ToString("F3");
//                             AbbJ6.Text = currentj6.ToString("F3");
//
//                             PlcX.Text = trussX.ToString("F3");
//                             PlcY.Text = trussY.ToString("F3");
//
//                             AbbStatusText.Text = robotText;
//                             AbbStatusText.Foreground = robotColor;
//
//                             PlcStatusText.Text = plcText;
//                             PlcStatusText.Foreground = plcColor;
//                         });
//
//                         var data = new
//                         {
//                             Header    = "Normal",
//                             Timestamp = DateTime.Now.ToString("yyMMddHHmmssfff"),
//                             J1 = _currentJointPositions[0],
//                             J2 = _currentJointPositions[1],
//                             J3 = _currentJointPositions[2],
//                             J4 = _currentJointPositions[3],
//                             J5 = _currentJointPositions[4],
//                             J6 = _currentJointPositions[5],
//                             TrussX = trussX,
//                             TrussY = trussY,
//                             VirRobotStatus = robotStatus == 4 ? "Running" : "Stopped",
//                             VirPlcStatus   = plcStatus   == 3 ? "Running" : "Stopped"
//                         };
//
//                         _errorReported = false;
//                         return JsonConvert.SerializeObject(data);
//                     }
//
//                     // 设备不正常时的处理
//                     if (!_errorReported)
//                     {
//                         var (robotText, robotColor) = GetRobotStatusInfo(robotStatus);
//                         var (plcText, plcColor) = GetPlcStatusInfo(plcStatus);
//
//                         // 即使设备异常，也要更新界面状态显示
//                         Dispatcher.Invoke(() =>
//                         {
//                             AbbStatusText.Text = robotText;
//                             AbbStatusText.Foreground = robotColor;
//                             PlcStatusText.Text = plcText;
//                             PlcStatusText.Foreground = plcColor;
//                         });
//
//                         var data = new
//                         {
//                             Header       = "Error",
//                             Timestamp = DateTime.Now.ToString("yyMMddHHmmssfff"),
//                             J1 = _currentJointPositions[0],
//                             J2 = _currentJointPositions[1],
//                             J3 = _currentJointPositions[2],
//                             J4 = _currentJointPositions[3],
//                             J5 = _currentJointPositions[4],
//                             J6 = _currentJointPositions[5],
//                             TrussX = trussX,
//                             TrussY = trussY,
//                             VirRobotStatus = "Stopped",
//                             VirPlcStatus   = "Stopped"
//                         };
//
//                         _errorReported = true;
//                         Log.Error($"设备异常 - 机器人状态: {robotStatus}，桁架状态: {plcStatus}");
//                         return JsonConvert.SerializeObject(data);
//                     }
//
//                     return null;
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[数据收集]: 收集数据时发生未预期异常: {ex.Message}");
//
//                     // 确保在异常情况下也更新界面状态为错误
//                     Dispatcher.Invoke(() =>
//                     {
//                         AbbStatusText.Text = "采集异常";
//                         AbbStatusText.Foreground = BrushRed;
//                         PlcStatusText.Text = "采集异常"; 
//                         PlcStatusText.Foreground = BrushRed;
//                     });
//
//                     var errorData = new
//                     {
//                         Header       = "Error",
//                         Timestamp    = DateTime.Now.ToString("yyMMddHHmmssfff"),
//                         VirRobotStatus = "Stopped",
//                         VirPlcStatus   = "Stopped"
//                     };
//
//                     _errorReported = true;
//                     return JsonConvert.SerializeObject(errorData);
//                 }
//             }
//             
//             // 桁架控制发送方法
//             // private async Task SendTrussMessageToPlc()
//             // {
//             //     try
//             //     {
//             //         // 检查是否有有效的桁架目标位置
//             //         if (_rlDataValid && (_targetTrussX != 0 || _targetTrussY != 0))
//             //         {
//             //             // 发送桁架位置指令
//             //             await _virtualTruss.PlcGotoPosition((float)_targetTrussX, (float)_targetTrussY);
//             //
//             //             if (_debug)
//             //             {
//             //                 Console.WriteLine($"[桁架]: 发送目标位置 - X:{_targetTrussX:F4}, Y:{_targetTrussY:F4}");
//             //             }
//             //         }
//             //     }
//             //     catch (Exception ex)
//             //     {
//             //         Log.Error($"[桁架]: 发送桁架指令失败: {ex.Message}");
//             //     }
//             // }
//             // 桁架控制发送方法
//             private async Task SendTrussMessageToPlc()
//             {
//                 try
//                 {
//                     // 检查是否有有效的桁架目标位置
//                     if (_rlDataValid && (_targetTrussX != 0 || _targetTrussY != 0))
//                     {
//                         double vx, vy;
//                         double targetX, targetY;
//
//                         // 统一从缓冲区读取最新目标
//                         lock (_bufferLock)
//                         {
//                             vx = _targetTrussVx;
//                             vy = _targetTrussVy;
//                             targetX = _targetTrussX;
//                             targetY = _targetTrussY;
//                         }
//
//                         // 将速度转换为每轴速度（绝对值，避免负速度）
//                         float xSpeed = (float)Math.Abs(vx);
//                         float ySpeed = (float)Math.Abs(vy);
//
//                         // 给一点下限和上限，避免 0 或暴走
//                         const float minSpeed = 10f;   // 你可以按现场改
//                         const float maxSpeed = 1000f; // 看你桁架能接受多大
//                         if (xSpeed < minSpeed && Math.Abs(vx) > 0.0001) xSpeed = minSpeed;
//                         if (ySpeed < minSpeed && Math.Abs(vy) > 0.0001) ySpeed = minSpeed;
//                         xSpeed = Math.Min(xSpeed, maxSpeed);
//                         ySpeed = Math.Min(ySpeed, maxSpeed);
//
//                         // 发送桁架位置+速度指令
//                         _ = _virtualTruss.PlcGotoPositionQuick(
//                             (float)targetX,
//                             (float)targetY,
//                             xSpeed,
//                             ySpeed);
//
//                         if (_debug)
//                         {
//                             Console.WriteLine(
//                                 $"[桁架]: 发送目标位置 - X:{targetX:F4}, Y:{targetY:F4}, " +
//                                 $"SpeedX:{xSpeed:F2}, SpeedY:{ySpeed:F2}");
//                         }
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[桁架]: 发送桁架指令失败: {ex.Message}");
//                 }
//             }
//
//
//         #endregion
//         
//         
//
//         
//         // 新增：处理CONTROL类型消息（预留）
//         // private async void ProcessControlMessage(dynamic message, string timeStampStr)
//         // {
//         //     try
//         //     {
//         //         if (_debug)
//         //         {
//         //             Console.WriteLine("[CONTROL]: 收到控制指令(时间戳:" + timeStampStr + ")");
//         //         }
//         //
//         //         // 只有在“因为错误暂停了EGM”的情况下，才执行自动恢复逻辑
//         //         if (!_egmPausedByError)
//         //         {
//         //             return;
//         //         }
//         //
//         //         Log.Information("[CONTROL]: 检测到错误暂停状态，开始自动恢复流程");
//         //
//         //         // 1. 在UI线程上调用 RobRestart，相当于你手动点了一次“重启”按钮
//         //         await Dispatcher.InvokeAsync(
//         //             delegate
//         //             {
//         //                 try
//         //                 {
//         //                     RobRestart(this, new RoutedEventArgs());
//         //                 }
//         //                 catch (Exception ex)
//         //                 {
//         //                     Log.Error("[CONTROL]: RobRestart 调用失败: " + ex.Message);
//         //                 }
//         //             });
//         //
//         //         // 2. 重置错误相关状态
//         //         await Task.Delay(1000); // 等待重启过程完成
//         //         _egmPausedByError = false;
//         //         _errorReported = false;
//         //         _devicesNormalLast = true;
//         //
//         //         // 3. 重新启动控制循环（不重复初始化监听/UDP，只重启 RunControlLoop）
//         //         await Dispatcher.InvokeAsync(
//         //             delegate
//         //             {
//         //                 try
//         //                 {
//         //                     // 直接开一个新的控制循环
//         //                     _ = Task.Run(() => RunControlLoop());
//         //                 }
//         //                 catch (Exception ex)
//         //                 {
//         //                     Log.Error("[CONTROL]: 自动重新启动控制循环失败: " + ex.Message);
//         //                 }
//         //             });
//         //
//         //         // 4. 构造并发送 READY 报文
//         //         int robotStatus = -1;
//         //         int plcStatus = -1;
//         //
//         //         try
//         //         {
//         //             robotStatus = _virtualRobot.GetVirRobotStatus();
//         //         }
//         //         catch (Exception ex)
//         //         {
//         //             Log.Error("[CONTROL]: 获取机器人状态失败: " + ex.Message);
//         //         }
//         //
//         //         try
//         //         {
//         //             plcStatus = _virtualTruss.GetVirPlcStatus();
//         //         }
//         //         catch (Exception ex)
//         //         {
//         //             Log.Error("[CONTROL]: 获取桁架状态失败: " + ex.Message);
//         //         }
//         //
//         //         string virRobotStatus = robotStatus == 4 ? "Running" : "Stopped";
//         //         string virPlcStatus = plcStatus == 3 ? "Running" : "Stopped";
//         //
//         //         string readyJson = JsonConvert.SerializeObject(
//         //             new
//         //             {
//         //                 Header = "READY",
//         //                 Timestamp = DateTime.Now.ToString("yyMMddHHmmssfff"),
//         //                 VirRobotStatus = virRobotStatus,
//         //                 VirPlcStatus = virPlcStatus
//         //             });
//         //
//         //         await SendCurrentDataToTargets(readyJson);
//         //
//         //         Log.Information("[CONTROL]: 自动恢复完成，已发送 READY 报文");
//         //     }
//         //     catch (Exception ex)
//         //     {
//         //         Log.Error("[CONTROL]: 处理 CONTROL 消息失败: " + ex.Message);
//         //     }
//         // }
//         
//         private async void ProcessControlMessage(dynamic message, string timeStampStr)
//         {
//             try
//             {
//                 if (_debug)
//                 {
//                     Console.WriteLine($"[CONTROL]: 收到控制指令(时间戳:{timeStampStr})");
//                 }
//
//                 // 只有在错误暂停状态下才执行自动恢复
//                 if (_egmPausedByError)
//                 {
//                     Log.Information("[CONTROL]: 检测到错误暂停状态，开始自动恢复");
//                     
//                     // 1. 执行重启
//                     await Dispatcher.InvokeAsync(async () =>
//                     {
//                         try
//                         {
//                             RobRestart(this, new RoutedEventArgs());
//                             
//                             // 等待重启完成（RobRestart内部有15秒等待）
//                             await Task.Delay(16000); // 比15秒稍长
//                             
//                             // 重新初始化
//                             _virtualRobot.NewInit();
//                             await Task.Delay(2000);
//                         }
//                         catch (Exception ex)
//                         {
//                             Log.Error($"[CONTROL]: 重启过程异常: {ex.Message}");
//                         }
//                     });
//
//                     // 2. 检查状态是否恢复正常
//                     int robotStatus = _virtualRobot.GetVirRobotStatus();
//                     int plcStatus = _virtualTruss.GetVirPlcStatus();
//                     
//                     if (robotStatus == 4 && plcStatus == 3)
//                     {
//                         // 3. 重置错误状态
//                         _egmPausedByError = false;
//                         _errorReported = false;
//                         _devicesNormalLast = true;
//
//                         // 4. 重新启动控制循环
//                         _egmStart = true;
//                         _ = Task.Run(() => RunControlLoop());
//
//                         // 5. 发送READY报文
//                         var readyData = new
//                         {
//                             Header = "READY", 
//                             Timestamp = DateTime.Now.ToString("yyMMddHHmmssfff"),
//                             VirRobotStatus = "Running",
//                             VirPlcStatus = "Running",
//                             Message = "设备已恢复就绪"
//                         };
//                         
//                         string readyJson = JsonConvert.SerializeObject(readyData);
//                         await SendCurrentDataToTargets(readyJson);
//                         
//                         Log.Information("[CONTROL]: 自动恢复完成，已发送READY报文");
//                     }
//                     else
//                     {
//                         Log.Warning($"[CONTROL]: 重启后状态仍未正常 - 机器人:{robotStatus}, 桁架:{plcStatus}");
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"[CONTROL]: 处理CONTROL消息失败: {ex.Message}");
//             }
//         }
//
//         
//         
//         // 8.1 检查启动条件
//         private bool ValidateAndInitializeControl()
//         {
//             // 检查EGM通信是否建立
//             if (_robotEndpoint == null)
//             {
//                 MessageBox.Show("EGM通信未建立，请等待机器人连接");
//                 return false;
//             }
//                 
//             if (_egmStart)
//             {
//                 MessageBox.Show("EGM控制已经在运行中");
//                 return false;
//             }
//
//             int? egmfreq = IsValidFrequency(FreRobot);
//             if (egmfreq == null) return false;
//                 
//             _egmStart = true;
//             Log.Information($"[LOP]: 验证通过，准备启动控制，EGM频率: {egmfreq}ms");
//             
//             // 满足启动条件的同时就开始锁定界面
//             LockInterface(_egmStart);
//             
//             return true;
//         }
//         
//         
//         
//         // 8.2.1 初始化双循环池
//         private void InitializeDoubleBuffer()
//         {
//             lock (_bufferLock)
//             {
//                 _positionBuffers[0] = new double[6];
//                 _positionBuffers[1] = new double[6];
//         
//                 // 初始化为当前关节位置
//                 Array.Copy(_currentJointPositions, _positionBuffers[0], 6);
//                 Array.Copy(_currentJointPositions, _positionBuffers[1], 6);
//         
//                 // 同时初始化积分位置
//                 Array.Copy(_currentJointPositions, _integratedPositions, 6);
//         
//                 _currentWriteBuffer = 0;
//                 _currentReadBuffer = 1;
//                 _newDataAvailable = false;
//         
//                 // 重置速度指令
//                 Array.Clear(_targetVelocities, 0, _targetVelocities.Length);
//         
//                 Log.Information("[双缓冲]: 双缓冲和积分位置已初始化");
//             }
//         }
//         
//         // 8.2.2 被动式更新最新机器人姿态（全凭egm随缘接收，无固定频率）
//         private double[] GetLatestTargetPositions()
//         {
//             lock (_bufferLock)
//             {
//                 // 如果有新数据，切换到最新的缓冲区
//                 if (_newDataAvailable)
//                 {
//                     _currentReadBuffer = _currentWriteBuffer;
//                     _newDataAvailable = false;
//                 }
//         
//                 return _positionBuffers[_currentReadBuffer];
//             }
//         }
//         
//         // 8.2.3
//         private void HandlePerformanceMonitoring(long elapsedMs, int expectedFrequency)
//         {
//             _sendCount++;
//     
//             // 简单的性能监控
//             if (elapsedMs > expectedFrequency)
//             {
//                 Log.Warning($"[性能]: 发送耗时 {elapsedMs}ms 超过预期周期 {expectedFrequency}ms");
//             }
//     
//             // 每50000次（50s）发送记录一次平均性能
//             if (_sendCount % 50000 == 0)
//             {
//                 Log.Information($"[性能]: 已发送 {_sendCount/50000} 簇指令，一簇指令为5万个");
//             }
//         }
//         
//         // 9. 核心发送方法
//         // private async Task SendJointMessageToRobot(double[] joints)
//         // {
//         //     if (_robotEndpoint == null)
//         //     {
//         //         throw new InvalidOperationException("机器人端点未初始化");
//         //     }
//         //
//         //     if (joints.Length != 6)
//         //     {
//         //         throw new ArgumentException("关节数组必须包含6个值");
//         //     }
//         //
//         //     using (var memoryStream = new MemoryStream())
//         //     {
//         //         var sensorMessage = new EgmSensor();
//         //         CreateJointMessage(sensorMessage, joints);
//         //
//         //         sensorMessage.WriteTo(memoryStream);
//         //         var data = memoryStream.ToArray();
//         //
//         //         // 发送关节控制消息到机器人
//         //         int bytesSent = await _localListener.SendAsync(data, data.Length, _robotEndpoint);
//         //
//         //         if (bytesSent <= 0)
//         //         {
//         //             throw new Exception("[EGM]: 发送关节控制消息失败");
//         //         }
//         //
//         //         // 日志系统要爆了
//         //         // Log.Debug($"[EGM]: 已发送关节控制消息，序列号: {sensorMessage.Header.Seqno}, 字节数: {bytesSent}");
//         //     }
//         // }
//         
//         // private async Task SendJointMessageToRobot(double[] joints)
//         // {
//         //     if (_robotEndpoint == null || joints.Length != 6) return;
//         //
//         //     MemoryStream memoryStream = null;
//         //     try
//         //     {
//         //         memoryStream = SimpleMemoryStreamPool.Get();
//         //         var sensorMessage = new EgmSensor();
//         //
//         //         CreateJointMessage(sensorMessage, joints);
//         //         sensorMessage.WriteTo(memoryStream);
//         //
//         //         var data = memoryStream.ToArray();
//         //         await _localListener.SendAsync(data, data.Length, _robotEndpoint);
//         //     }
//         //     catch (Exception ex)
//         //     {
//         //         Log.Error($"[EGM]: 发送关节控制消息失败: {ex.Message}");
//         //     }
//         //     finally
//         //     {
//         //         if (memoryStream != null)
//         //         {
//         //             SimpleMemoryStreamPool.Return(memoryStream);
//         //         }
//         //     }
//         // }
//         
//         private async Task SendJointMessageToRobot(double[] joints)
//         {
//             if (_robotEndpoint == null || joints.Length != 6) 
//             {
//                 if (_robotEndpoint == null)
//                 {
//                     Log.Warning("[EGM发送]: 机器人端点为空，无法发送指令");
//                 }
//                 return;
//             }
//
//             MemoryStream memoryStream = null;
//             try
//             {
//                 memoryStream = SimpleMemoryStreamPool.Get();
//                 var sensorMessage = new EgmSensor();
//
//                 CreateJointMessage(sensorMessage, joints);
//                 sensorMessage.WriteTo(memoryStream);
//
//                 var data = memoryStream.ToArray();
//         
//                 // 添加发送统计
//                 if (_sendCount % 20 == 0)
//                 {
//                     Log.Information($"[EGM发送]: 向 {_robotEndpoint} 发送指令，序列号: {sensorMessage.Header.Seqno}, 数据长度: {data.Length} 字节");
//                 }
//         
//                 int bytesSent = await _localListener.SendAsync(data, data.Length, _robotEndpoint);
//         
//                 if (bytesSent <= 0)
//                 {
//                     Log.Error("[EGM发送]: 发送失败，字节数为0");
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"[EGM发送]: 发送关节控制消息失败: {ex.Message}");
//             }
//             finally
//             {
//                 if (memoryStream != null)
//                 {
//                     SimpleMemoryStreamPool.Return(memoryStream);
//                 }
//             }
//         }
//         
//         // 10. 核心发送方法的核心方法
//         private void CreateJointMessage(EgmSensor message, double[] joints)
//         {
//             // 创建EGM关节控制消息
//             var header = new EgmHeader
//             {
//                 Seqno = _egmSequenceNumber++,
//                 Tm = (uint)DateTime.Now.Ticks,
//                 Mtype = EgmHeader.Types.MessageType.MsgtypeCorrection
//             };
//
//             message.Header = header;
//
//             var planned = new EgmPlanned();
//             var egmJoints = new EgmJoints();
//
//             // 添加6个关节值
//             for (int i = 0; i < 6; i++)
//             {
//                 egmJoints.Joints.Add(joints[i]);
//             }
//
//             planned.Joints = egmJoints;
//             message.Planned = planned;
//         }
//         
//         
//
//         // 12. RL监听停止
//         private void RlEnd()
//         {
//             try
//             {
//                 _isRlListen = false;
//                 
//                 if (_rlValidityTimer != null)
//                 {
//                     _rlValidityTimer.Stop();
//                 }
//         
//                 // 重置目标速度为零
//                 Array.Clear(_targetVelocities, 0, _targetVelocities.Length);
//         
//                 // 重置积分位置为当前关节位置
//                 lock (_bufferLock)
//                 {
//                     Array.Copy(_currentJointPositions, _integratedPositions, 6);
//                 }
//         
//                 Log.Information("[RL]: RL监听已停止，速度指令和积分位置已重置");
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"[RL]: 停止RL监听失败: {ex.Message}");
//             }
//         }
//         
//         private async Task<bool> InitializeZeroingListener()
//         {
//             try
//             {
//                 int? port = IsValidPort(PortListener);
//                 if (port == null) return false;
//
//                 _zeroingCts = new CancellationTokenSource();
//
//                 // 创建归零专用的Socket
//                 var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
//                 {
//                     ExclusiveAddressUse = false
//                 };
//                 socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
//                 socket.Bind(new IPEndPoint(IPAddress.Any, port.Value));
//
//                 _zeroingListener = new UdpClient() { Client = socket };
//
//                 // 启动归零专用的监听循环
//                 _ = Task.Run(() => ZeroingListenLoopAsync(_zeroingCts.Token));
//                 
//                 Log.Information($"[归零]: 归零专用监听已启动，端口: {port.Value}");
//                 return true;
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"[归零]: 启动归零监听失败: {ex.Message}");
//                 return false;
//             }
//         }
//
//         private async Task ZeroingListenLoopAsync(CancellationToken token)
//         {
//             while (!token.IsCancellationRequested)
//             {
//                 try
//                 {
//                     var result = await _zeroingListener.ReceiveAsync();
//                     var sender = result.RemoteEndPoint;
//                     var data = result.Buffer;
//
//                     // 只处理EGM消息来更新关节位置
//                     _ = Task.Run(() => ProcessZeroingEgmMessage(data, sender));
//                 }
//                 catch (ObjectDisposedException)
//                 {
//                     break;
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[归零]: 监听循环异常: {ex.Message}");
//                 }
//             }
//         }
//
//         private void ProcessZeroingEgmMessage(byte[] data, IPEndPoint sender)
//         {
//             try
//             {
//                 EgmRobot robotMessage = EgmRobot.Parser.ParseFrom(data);
//                 if (robotMessage?.FeedBack?.Joints != null)
//                 {
//                     var joints = robotMessage.FeedBack.Joints;
//                     
//                     // 更新关节位置（归零过程中也需要更新）
//                     for (int i = 0; i < Math.Min(6, joints.Joints.Count); i++)
//                     {
//                         _currentJointPositions[i] = joints.Joints[i];
//                     }
//                     
//                     // 记录机器人端点，用于发送指令
//                     _robotEndpoint = sender;
//                     
//                     Log.Debug($"[归零]: 更新关节位置 - J1:{_currentJointPositions[0]:F4}, J2:{_currentJointPositions[1]:F4}, " +
//                               $"J3:{_currentJointPositions[2]:F4}, J4:{_currentJointPositions[3]:F4}, " +
//                               $"J5:{_currentJointPositions[4]:F4}, J6:{_currentJointPositions[5]:F4}");
//                 }
//             }
//             catch
//             {
//                 // 忽略非EGM消息
//             }
//         }
//
//         private async Task PerformZeroing()
//         {
//             try
//             {
//                 int frequency = GetnumberFrom(FreRobot);
//                 Log.Information($"[归零]: 开始执行归零，频率: {frequency}ms");
//                 
//                 bool reachedZero = false;
//                 int sendCount = 0;
//                 const int maxSendCount = 2000; // 增加最大发送次数，归零可能需要更长时间
//                 const double tolerance = 0.001; // 零位容差
//
//                 // 等待机器人连接
//                 int waitCount = 0;
//                 while (_robotEndpoint == null && waitCount < 100) // 最多等待5秒
//                 {
//                     await Task.Delay(50);
//                     waitCount++;
//                 }
//
//                 if (_robotEndpoint == null)
//                 {
//                     MessageBox.Show("归零过程中无法连接到机器人");
//                     return;
//                 }
//
//                 while (!reachedZero && sendCount < maxSendCount && !_zeroingCts.Token.IsCancellationRequested)
//                 {
//                     try
//                     {
//                         // 发送零位指令
//                         await SendZeroingMessage(new double[6] { 0, 0, 0, 0, 0, 0 });
//                         sendCount++;
//
//                         // 检查是否到达零位
//                         reachedZero = true;
//                         for (int i = 0; i < 6; i++)
//                         {
//                             if (Math.Abs(_currentJointPositions[i]) > tolerance)
//                             {
//                                 reachedZero = false;
//                                 break;
//                             }
//                         }
//
//                         if (reachedZero)
//                         {
//                             Log.Information("[归零]: 机器人已到达零位");
//                             MessageBox.Show("机器人已到达零位");
//                             break;
//                         }
//
//                         // 按界面频率等待
//                         if (frequency > 0)
//                         {
//                             await Task.Delay(frequency, _zeroingCts.Token);
//                         }
//
//                         // 每100次发送记录一次进度
//                         if (sendCount % 100 == 0)
//                         {
//                             Log.Information($"[归零]: 已发送 {sendCount} 次零位指令");
//                             Log.Information($"[归零]: 当前位置 - J1:{_currentJointPositions[0]:F4}, J2:{_currentJointPositions[1]:F4}, " +
//                                           $"J3:{_currentJointPositions[2]:F4}, J4:{_currentJointPositions[3]:F4}, " +
//                                           $"J5:{_currentJointPositions[4]:F4}, J6:{_currentJointPositions[5]:F4}");
//                         }
//                     }
//                     catch (OperationCanceledException)
//                     {
//                         break;
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Error($"[归零]: 发送零位指令失败: {ex.Message}");
//                         await Task.Delay(100);
//                     }
//                 }
//
//                 if (sendCount >= maxSendCount)
//                 {
//                     Log.Warning("[归零]: 达到最大发送次数，停止归零");
//                     MessageBox.Show("归零超时，机器人可能无法到达零位");
//                 }
//                 else if (!reachedZero)
//                 {
//                     Log.Warning("[归零]: 归零过程被取消");
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"[归零]: 执行归零过程异常: {ex.Message}");
//                 throw;
//             }
//         }
//
//         private async Task SendZeroingMessage(double[] joints)
//         {
//             if (_robotEndpoint == null) return;
//
//             using (var memoryStream = new MemoryStream())
//             {
//                 var sensorMessage = new EgmSensor();
//                 CreateJointMessage(sensorMessage, joints);
//                 sensorMessage.WriteTo(memoryStream);
//                 var data = memoryStream.ToArray();
//
//                 await _zeroingListener.SendAsync(data, data.Length, _robotEndpoint);
//             }
//         }
//
//         private void StopZeroingListener()
//         {
//             try
//             {
//                 _zeroingCts?.Cancel();
//             }
//             catch { /* ignore */ }
//             finally
//             {
//                 try
//                 {
//                     _zeroingListener?.Close();
//                     _zeroingListener?.Dispose();
//                 }
//                 catch { /* ignore */ }
//                 finally
//                 {
//                     _zeroingListener = null;
//                 }
//
//                 try
//                 {
//                     _zeroingCts?.Dispose();
//                 }
//                 catch { /* ignore */ }
//                 finally
//                 {
//                     _zeroingCts = null;
//                 }
//             }
//
//             Log.Information("[归零]: 归零专用监听已停止");
//         }
//         
//         #region 妙妙小工具
//
//             private int? IsValidPort(TextBox portTextBox)
//             {
//                 try
//                 {
//                     string text = portTextBox.Text?.Trim();
//                     if (string.IsNullOrWhiteSpace(text))
//                     {
//                         Log.Error($"[200]: 指定 {portTextBox.Name} 端口为空。");
//                         MessageBox.Show($"[200]: 指定 {portTextBox.Name} 端口为空。");
//                         return null;
//                     }
//
//                     if (!int.TryParse(text, out int port))
//                     {
//                         Log.Error($"[200]: 指定 {portTextBox.Name} 端口格式错误（不是数字）: {text}");
//                         MessageBox.Show($"[200]: 指定 {portTextBox.Name} 端口格式错误（不是数字）: {text}");
//                         return null;
//                     }
//
//                     if (port < 1 || port > 65535)
//                     {
//                         Log.Error($"[200]: 指定 {portTextBox.Name} 端口超出范围 (1-65535): {port}");
//                         MessageBox.Show($"[200]: 指定 {portTextBox.Name} 端口超出范围 (1-65535): {port}");
//                         return null;
//                     }
//
//                     // Log.Information($"[200]: 指定 {portTextBox.Name} 端口返回正确值: {port}");
//                     // 不用弹窗也没必要记录日志了
//                     return port;
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[200]: 获取 {portTextBox.Name} 端口时出现异常: {ex.Message}");
//                     MessageBox.Show($"[200]: 获取 {portTextBox.Name} 端口时出现异常: {ex.Message}");
//                     return null;
//                 }
//             }
//             
//             private int? IsValidFrequency(TextBox portTextBox)
//             {
//                 try
//                 {
//                     string text = portTextBox.Text?.Trim();
//                     if (string.IsNullOrWhiteSpace(text))
//                     {
//                         Log.Error($"[200]: 指定 {portTextBox.Name} 频率为空。");
//                         MessageBox.Show($"[200]: 指定 {portTextBox.Name} 频率为空。");
//                         portTextBox.Text = "100";
//                         return null;
//                     }
//
//                     if (!int.TryParse(text, out int port))
//                     {
//                         Log.Error($"[200]: 指定 {portTextBox.Name} 频率格式错误（不是整数）: {text}");
//                         MessageBox.Show($"[200]: 指定 {portTextBox.Name} 频率格式错误（不是数字）: {text}");
//                         portTextBox.Text = "100";
//                         return null;
//                     }
//
//                     if (port < 4 || port > 1000)
//                     {
//                         Log.Error($"[200]: 指定 {portTextBox.Name} 频率超出范围 (4-1000ms): {port}");
//                         MessageBox.Show($"[200]: 指定 {portTextBox.Name} 频率超出范围 (4-1000ms): {port}");
//                         portTextBox.Text = "100";
//                         return null;
//                     }
//                     
//                     if (port % 4 != 0)
//                     {
//                         int portnew = Math.Min(Math.Max((int)(port/4) * 4, 4),1000);
//                         portTextBox.Text = portnew.ToString();
//                         Log.Error($"[200]: 指定 {portTextBox.Name} 频率非4的倍数，已启用自动更改: {port} -> {portnew}");
//                         MessageBox.Show($"[200]: 指定 {portTextBox.Name} 频率非4的倍数，已启用自动更改: {port} -> {portnew}");
//                         return portnew;
//                     }
//
//                     // Log.Information($"[200]: 指定 {portTextBox.Name} 端口返回正确值: {port}");
//                     // 不用弹窗也没必要记录日志了
//                     return port;
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[200]: 获取 {portTextBox.Name} 端口时出现异常: {ex.Message}");
//                     MessageBox.Show($"[200]: 获取 {portTextBox.Name} 端口时出现异常: {ex.Message}");
//                     portTextBox.Text = "100";
//                     return null;
//                 }
//             }
//             
//             // IP地址验证函数
//             private bool IsValidIP(string ipAddress)
//             {
//                 try
//                 {
//                     if (string.IsNullOrWhiteSpace(ipAddress))
//                     {
//                         return false;
//                     }
//
//                     // 使用正则表达式验证IP地址格式
//                     var ipPattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
//                     return Regex.IsMatch(ipAddress, ipPattern);
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[IP验证]: IP地址验证异常: {ex.Message}");
//                     return false;
//                 }
//             }
//             
//             // private int GetnumberFrom(TextBox numTextBox)
//             // {
//             //     try
//             //     {
//             //         if (int.TryParse(numTextBox.Text, out int frequency))
//             //         {
//             //             return frequency;
//             //         }
//             //         numTextBox.Text = "100";
//             //         return 100; // 默认值
//             //     }
//             //     catch (Exception e)
//             //     {
//             //         numTextBox.Text = "100";
//             //         return 100; // 默认值
//             //     }
//             // }
//             private int GetnumberFrom(TextBox numTextBox)
//             {
//                 try
//                 {
//                     // 如果不在UI线程，使用Dispatcher
//                     if (!numTextBox.CheckAccess())
//                     {
//                         return (int)numTextBox.Dispatcher.Invoke(new Func<int>(() => GetnumberFrom(numTextBox)));
//                     }
//         
//                     // UI线程中的原始逻辑
//                     if (int.TryParse(numTextBox.Text, out int frequency))
//                     {
//                         return frequency;
//                     }
//                     numTextBox.Text = "100";
//                     return 100;
//                 }
//                 catch (Exception e)
//                 {
//                     numTextBox.Text = "100";
//                     return 100;
//                 }
//             }
//             
//             private void InitializeRlBlinkTimer()
//             {
//                 _rlBlinkTimer = new System.Timers.Timer(500); // 500ms闪烁间隔
//                 _rlBlinkTimer.Elapsed += (s, e) =>
//                 {
//                     Dispatcher.Invoke(() =>
//                     {
//                         RlConnection.Fill = new SolidColorBrush(Colors.Gray); // 变回灰色
//                     });
//                     _rlBlinkTimer.Stop();
//                 };
//                 _rlBlinkTimer.AutoReset = false;
//             }
//
//             private void TriggerRlBlink()
//             {
//                 Dispatcher.Invoke(() =>
//                 {
//                     RlConnection.Fill = new SolidColorBrush(Colors.Green); // 变绿色
//                 });
//     
//                 // 重启定时器
//                 _rlBlinkTimer.Stop();
//                 _rlBlinkTimer.Start();
//             }
//             
//             
//             
//         #endregion
//
//         #region 界面原生按钮
//         
//         private async void EgmStart(object sender, RoutedEventArgs e)
//         {
//             try
//             {
//                 Log.Information("[EGM]: 开始启动流程");
//                 
//                 // 启动前重置错误相关状态
//                 _egmPausedByError = false;
//                 _devicesNormalLast = true;
//                 _errorReported = false;
//
//                 // _virtualRobot.Refresh();
//                 
//                 // 1. 锁定egmstart和输入框
//                 LockInterface(true);
//                 
//                 // 2. 初始化监听界面上的端口
//                 InitializeListener();
//                 
//                 // 3. 初始化RL有效期定时器
//                 InitializeRlValidityTimer();
//         
//                 // 4. 自主开toggle和开启监听
//                 RlListener.IsChecked = true;
//                 RlStart();
//                 
//                 // 5. 初始化UDP发送器
//                 if (!InitializeRlSender())
//                 {
//                     Log.Warning("[EGM]: RL发送器初始化失败，继续其他流程");
//                 }
//                 if (!InitializeUeSender())
//                 {
//                     Log.Warning("[EGM]: UE发送器初始化失败，继续其他流程");
//                 }
//                 StartPeriodicSending();
//                 
//                 // 6. 直接启动控制循环
//                 _ = Task.Run(() => RunControlLoop());
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"[EGM]: 启动失败: {ex.Message}");
//                 MessageBox.Show($"[EGM]: 启动失败: {ex.Message}");
//                 // 确保失败时重置状态
//                 _egmStart = false;
//                 LockInterface(false);
//             }
//         }
//         
//         private void EgmEnd(object sender, RoutedEventArgs e)
//         {
//             try
//             {
//                 // 你原来的停止逻辑...
//                 RlEnd();
//                 RlListener.IsChecked = false;
//                 StopListener();
//
//                 _egmStart = false;
//                 _robotEndpoint = null;
//                 if (_controlCts != null)
//                 {
//                     _controlCts.Cancel();
//                 }
//
//                 StopPeriodicSender();
//                 if (_rlValidityTimer != null)
//                 {
//                     _rlValidityTimer.Stop();
//                 }
//                 if (_controlCts != null)
//                 {
//                     _controlCts.Dispose();
//                     _controlCts = null;
//                 }
//
//                 // 新增：清理错误相关状态
//                 _egmPausedByError = false;
//                 _devicesNormalLast = true;
//                 _errorReported = false;
//
//                 LockInterface(false);
//                 Log.Information("[EGM]: 已停止");
//             }
//             catch (Exception ex)
//             {
//                 Log.Error("[EGM]: 停止EGM时发生异常: " + ex.Message);
//             }
//         }
//
//
//
//         // ToggleButton事件处理
//         private void RlListener_Checked(object sender, RoutedEventArgs e)
//         {
//             RlStart();
//         }
//
//         private void RlListener_Unchecked(object sender, RoutedEventArgs e)
//         {
//             RlEnd();
//         }
//         
//         private async void ZeroRefresh(object sender, RoutedEventArgs e)
//         {
//             try
//             {
//                 Log.Information("[归零]: 开始归零流程");
//         
//                 // 1. 记录当前EGM状态
//                 _wasEgmRunning = _egmStart;
//         
//                 // 2. 如果EGM正在运行，先停止它
//                 if (_wasEgmRunning)
//                 {
//                     Log.Information("[归零]: 停止EGM以执行归零");
//                     // 停止EGM但不解锁界面（归零期间保持锁定）
//                     _egmStart = false;
//                     _controlCts?.Cancel();
//                     RlEnd();
//                     StopListener();
//                 }
//         
//                 // 3. 锁定所有按钮（归零专用锁定）
//                 // LockAllButtonsExceptZero();
//         
//                 // 4. 开启新的归零专用监听
//                 if (!await InitializeZeroingListener())
//                 {
//                     MessageBox.Show("归零监听启动失败");
//                     // 根据之前状态恢复界面
//                     if (_wasEgmRunning)
//                     {
//                         LockInterface(true); // EGM运行时的锁定状态
//                     }
//                     else
//                     {
//                         LockInterface(false); // 完全解锁
//                     }
//                     return;
//                 }
//         
//                 // 5. 执行归零操作
//                 await PerformZeroing();
//         
//                 // 6. 停止归零监听
//                 StopZeroingListener();
//         
//                 // 7. 根据之前的状态决定是否重启EGM和恢复界面状态
//                 if (_wasEgmRunning)
//                 {
//                     Log.Information("[归零]: 归零完成，重新启动EGM");
//                     // 短暂延迟确保资源释放
//                     await Task.Delay(100);
//             
//                     // 恢复EGM运行时的界面状态
//                     LockInterface(true);
//             
//                     // 重新启动EGM
//                     EgmStart(sender, e);
//                 }
//                 else
//                 {
//                     Log.Information("[归零]: 归零完成，恢复界面");
//                     // 完全解锁界面
//                     LockInterface(false);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"[归零]: 归零过程异常: {ex.Message}");
//                 MessageBox.Show($"归零失败: {ex.Message}");
//                 // 确保异常时清理资源并恢复界面
//                 StopZeroingListener();
//                 // 根据之前状态恢复界面
//                 if (_wasEgmRunning)
//                 {
//                     LockInterface(true);
//                 }
//                 else
//                 {
//                     LockInterface(false);
//                 }
//             }
//         }
//         
//         #endregion
//
//         
//         private void RobStop(object sender, RoutedEventArgs e)
//         {
//             _virtualRobot.EmergyStop();
//         }
//         private void RobEnable(object sender, RoutedEventArgs e)
//         {
//             _virtualRobot.NewInit();
//         }
         // private void RobRestart(object sender, RoutedEventArgs e)
         // {
         //     _virtualRobot.NewRestart();
         //     Thread.Sleep(15000);
         //     // // Log.Information("开始初始化");
         //     _virtualRobot.NewInit();
         // }
//         
//     }
// }