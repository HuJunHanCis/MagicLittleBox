// using System;
// using System.Diagnostics;
// using System.Net;
// using System.Net.Sockets;
// using System.Text;
// using System.Threading;
// using System.Threading.Tasks;
// using System.Windows;
// using System.Windows.Controls;
// using System.Windows.Input;
// using Abb.Egm;
// using Newtonsoft.Json;
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
//
//         #region 非UDP字段
//         
//             // 发生碰撞后只发送一次错误的标志位
//             bool _errorSent = false;
//             
//             // 这俩实例不解释
//             private readonly SupVirtualRobot _virtualRobot = SupVirtualRobot.Instance;
//             private readonly SupVirtualTruss _virtualTruss = SupVirtualTruss.Instance;
//             
//             private static readonly SolidColorBrush BrushRed = new SolidColorBrush(Color.FromArgb(0xFF, 0xC9, 0x4F, 0x4F));
//             private static readonly SolidColorBrush BrushYel = new SolidColorBrush(Color.FromArgb(0xFF, 0xD9, 0xB7, 0x2B));
//             private static readonly SolidColorBrush BrushGre = new SolidColorBrush(Color.FromArgb(0xFF, 0x57, 0x96, 0x5C));
//             private static readonly SolidColorBrush BrushGry = new SolidColorBrush(Color.FromArgb(0xFF, 0x91, 0x91, 0x91));
//
//             private readonly double[][] _axisLimits = 
//             {
//                 new double[] { -179, 179 }, // J1 关节
//                 new double[] { -89, 69 }, // J2 关节  
//                 new double[] { -129, 72 }, // J3 关节
//                 new double[] { -169, 169 }, // J4 关节
//                 new double[] { -116, 89 }, // J5 关节
//                 new double[] { -139, 125 }, // J6 关节
//                 new double[] { -650, 4200 },   // 桁架X轴
//                 new double[] { -550, 2429 }     // 桁架Y轴
//             };
//             
//         #endregion
//
//         #region 100%完成: 检测box控件是否合格
//
//             /// <summary>
//             /// 检查 TextBox 中的文本是否为合法 IP 地址。
//             /// 合规则返回 IP 字符串，不合法则弹出 MessageBox，返回 null。
//             /// </summary>
//             private string CheckIpTextBox(TextBox textBox)
//             {
//                 if (textBox == null)
//                 {
//                     return null;
//                 }
//
//                 string text = textBox.Text.Trim();
//
//                 IPAddress address;
//                 if (IPAddress.TryParse(text, out address))
//                 {
//                     return address.ToString();
//                 }
//
//                 MessageBox.Show("请输入合法的 IP 地址。",
//                     "输入错误",
//                     MessageBoxButton.OK,
//                     MessageBoxImage.Warning);
//                 return null;
//             }
//
//             /// <summary>
//             /// 检查 TextBox 中的文本是否为合法端口（1-65535）。
//             /// 合规则返回端口整数，不合法则弹出 MessageBox，返回 null。
//             /// </summary>
//             private int? CheckPortTextBox(TextBox textBox)
//             {
//                 if (textBox == null)
//                 {
//                     return null;
//                 }
//
//                 string text = textBox.Text.Trim();
//
//                 int port;
//                 if (int.TryParse(text, out port) && port >= 1 && port <= 65535)
//                 {
//                     return port;
//                 }
//
//                 MessageBox.Show("请输入 1-65535 之间的合法端口号。",
//                     "输入错误",
//                     MessageBoxButton.OK,
//                     MessageBoxImage.Warning);
//                 return null;
//             }
//
//             /// <summary>
//             /// 检查 TextBox 中的文本是否为合法频率（4-1000）。
//             /// 合规则返回频率整数，不合法则弹出 MessageBox，返回 null。
//             /// </summary>
//             private int? CheckFreTextBox(TextBox textBox)
//             {
//                 if (textBox == null)
//                 {
//                     return null;
//                 }
//
//                 string text = textBox.Text.Trim();
//
//                 int fre;
//                 if (int.TryParse(text, out fre) && fre >= 4 && fre <= 1000)
//                 {
//                     return fre;
//                 }
//
//                 MessageBox.Show("请输入 4-1000 之间的建议频率值。",
//                     "输入错误",
//                     MessageBoxButton.OK,
//                     MessageBoxImage.Warning);
//                 return null;
//             }
//
//         #endregion
//
//         #region 100%完成: 监听服务
//
//             private UdpClient _mainListenerClient;
//             private CancellationTokenSource _mainListenerCts;
//             /// <summary>
//             /// 启动总监听
//             /// </summary>
//             private void StartMainListener()
//             {
//                 if (_mainListenerClient != null)
//                     return;
//
//                 int? port = CheckPortTextBox(PortListener);
//                 if (!port.HasValue) return;
//
//                 try
//                 {
//                     _mainListenerClient = new UdpClient(port.Value);
//                     _mainListenerCts = new CancellationTokenSource();
//
//                     var localClient = _mainListenerClient;
//                     var localCts = _mainListenerCts;
//
//                     Task.Run(() => MainListenLoop(localClient, localCts.Token));
//                 }
//                 catch (Exception ex)
//                 {
//                     if (_mainListenerClient != null)
//                     {
//                         _mainListenerClient.Close();
//                         _mainListenerClient = null;
//                     }
//
//                     if (_mainListenerCts != null)
//                     {
//                         _mainListenerCts.Cancel();
//                         _mainListenerCts.Dispose();
//                         _mainListenerCts = null;
//                     }
//
//                     MessageBox.Show("启动总监听失败: " + ex.Message,
//                         "总监听",
//                         MessageBoxButton.OK,
//                         MessageBoxImage.Error);
//                 }
//             }
//
//             /// <summary>
//             /// 停止总监听
//             /// </summary>
//             private void StopMainListener()
//             {
//                 try
//                 {
//                     if (_mainListenerCts != null)
//                     {
//                         _mainListenerCts.Cancel();
//                         _mainListenerCts.Dispose();
//                         _mainListenerCts = null;
//                     }
//
//                     if (_mainListenerClient != null)
//                     {
//                         _mainListenerClient.Close();
//                         _mainListenerClient = null;
//                     }
//                 }
//                 catch
//                 {
//                     // 静默处理
//                 }
//             }
//             
//         #endregion
//
//         #region 100%完成: RL和UE的发送服务
//
//             private UdpClient _rlSenderClient;
//             private CancellationTokenSource _rlSenderCts;
//             private IPEndPoint _rlRemoteEndPoint;
//
//             private UdpClient _ueSenderClient;
//             private CancellationTokenSource _ueSenderCts;
//             private IPEndPoint _ueRemoteEndPoint;
//             
//             /// <summary>
//             /// 启动RL发送
//             /// </summary>
//             private void StartRlSender()
//             {
//                 if (_rlSenderClient != null)
//                     return;
//
//                 string ip = CheckIpTextBox(IpReinLearn);
//                 int? port = CheckPortTextBox(PortReinLearn);
//                 int? freq = CheckFreTextBox(FreOutRlUe);
//                 
//                 if (ip == null || !port.HasValue || !freq.HasValue) return;
//
//                 try
//                 {
//                     IPAddress address;
//                     if (!IPAddress.TryParse(ip, out address))
//                     {
//                         MessageBox.Show("RL发送IP地址不合法。",
//                             "RL发送",
//                             MessageBoxButton.OK,
//                             MessageBoxImage.Warning);
//                         return;
//                     }
//
//                     _rlRemoteEndPoint = new IPEndPoint(address, port.Value);
//                     _rlSenderClient = new UdpClient();
//                     _rlSenderCts = new CancellationTokenSource();
//
//                     var localClient = _rlSenderClient;
//                     var localCts = _rlSenderCts;
//                     var localEndPoint = _rlRemoteEndPoint;
//                     var localFreq = freq.Value;
//
//                     Task.Run(() => RlUeSendLoop(localClient, localEndPoint, localFreq, localCts.Token));
//                 }
//                 catch (Exception ex)
//                 {
//                     if (_rlSenderClient != null)
//                     {
//                         _rlSenderClient.Close();
//                         _rlSenderClient = null;
//                     }
//
//                     if (_rlSenderCts != null)
//                     {
//                         _rlSenderCts.Cancel();
//                         _rlSenderCts.Dispose();
//                         _rlSenderCts = null;
//                     }
//
//                     _rlRemoteEndPoint = null;
//
//                     MessageBox.Show("启动RL发送失败: " + ex.Message,
//                         "RL发送",
//                         MessageBoxButton.OK,
//                         MessageBoxImage.Error);
//                 }
//             }
//
//             /// <summary>
//             /// 停止RL发送
//             /// </summary>
//             private void StopRlSender()
//             {
//                 try
//                 {
//                     if (_rlSenderCts != null)
//                     {
//                         _rlSenderCts.Cancel();
//                         _rlSenderCts.Dispose();
//                         _rlSenderCts = null;
//                     }
//
//                     if (_rlSenderClient != null)
//                     {
//                         _rlSenderClient.Close();
//                         _rlSenderClient = null;
//                     }
//
//                     _rlRemoteEndPoint = null;
//                 }
//                 catch
//                 {
//                     // 静默处理
//                 }
//             }
//
//             /// <summary>
//             /// 启动UE发送
//             /// </summary>
//             private void StartUeSender()
//             {
//                 if (_ueSenderClient != null)
//                     return;
//
//                 string ip = CheckIpTextBox(IpUe);
//                 int? port = CheckPortTextBox(PortUe);
//                 int? freq = CheckFreTextBox(FreOutRlUe);
//                 
//                 if (ip == null || !port.HasValue || !freq.HasValue) return;
//
//                 try
//                 {
//                     IPAddress address;
//                     if (!IPAddress.TryParse(ip, out address))
//                     {
//                         MessageBox.Show("UE发送IP地址不合法。",
//                             "UE发送",
//                             MessageBoxButton.OK,
//                             MessageBoxImage.Warning);
//                         return;
//                     }
//
//                     _ueRemoteEndPoint = new IPEndPoint(address, port.Value);
//                     _ueSenderClient = new UdpClient();
//                     _ueSenderCts = new CancellationTokenSource();
//
//                     var localClient = _ueSenderClient;
//                     var localCts = _ueSenderCts;
//                     var localEndPoint = _ueRemoteEndPoint;
//                     var localFreq = freq.Value;
//
//                     Task.Run(() => RlUeSendLoop(localClient, localEndPoint, localFreq, localCts.Token));
//                 }
//                 catch (Exception ex)
//                 {
//                     if (_ueSenderClient != null)
//                     {
//                         _ueSenderClient.Close();
//                         _ueSenderClient = null;
//                     }
//
//                     if (_ueSenderCts != null)
//                     {
//                         _ueSenderCts.Cancel();
//                         _ueSenderCts.Dispose();
//                         _ueSenderCts = null;
//                     }
//
//                     _ueRemoteEndPoint = null;
//
//                     MessageBox.Show("启动UE发送失败: " + ex.Message,
//                         "UE发送",
//                         MessageBoxButton.OK,
//                         MessageBoxImage.Error);
//                 }
//             }
//
//             /// <summary>
//             /// 停止UE发送
//             /// </summary>
//             private void StopUeSender()
//             {
//                 try
//                 {
//                     if (_ueSenderCts != null)
//                     {
//                         _ueSenderCts.Cancel();
//                         _ueSenderCts.Dispose();
//                         _ueSenderCts = null;
//                     }
//
//                     if (_ueSenderClient != null)
//                     {
//                         _ueSenderClient.Close();
//                         _ueSenderClient = null;
//                     }
//
//                     _ueRemoteEndPoint = null;
//                 }
//                 catch
//                 {
//                     // 静默处理
//                 }
//             }
//             
//         #endregion
//
//         #region 100%完成: Egm的接收与发送
//         
//             private IPEndPoint _robotEndpoint; // 机器人端点
//             private uint _egmSequenceNumber; // EGM序列号
//             private readonly double[] _egmPositions = { 0, 0, 0, 0, 0, 0 };
//             
//             /// <summary>
//             /// 处理接收到的EGM消息
//             /// </summary>
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
//                             _egmPositions[i] = joints.Joints[i];  // 直接取double值
//                         }
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Warning($"[EGM]: 解析EGM消息失败: {ex.Message}");
//                 }
//             }
//
//             /// <summary>
//             /// 发送关节消息到机器人
//             /// </summary>
//             private async Task SendJointMessageToRobot(double[] joints)
//             {
//                 if (_robotEndpoint == null || joints.Length != 6) 
//                 {
//                     Log.Warning("[EGM发送]: 机器人端点不存在，检查RS是否打开");
//                     return;
//                 }
//
//                 using (var memoryStream = new MemoryStream())
//                 {
//                     try
//                     {
//                         var sensorMessage = new EgmSensor();
//                         CreateJointMessage(sensorMessage, joints);
//                         sensorMessage.WriteTo(memoryStream);
//
//                         var data = memoryStream.ToArray();
//                         await _mainListenerClient.SendAsync(data, data.Length, _robotEndpoint);
//                 
//                         // Log.Debug($"[EGM发送]: 已向 {_robotEndpoint} 发送关节指令");
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Error($"[EGM发送]: 发送失败: {ex.Message}");
//                     }
//                 }
//             }
//             
//             /// <summary>
//             /// 创建EGM关节控制消息
//             /// </summary>
//             private void CreateJointMessage(EgmSensor message, double[] joints)
//             {
//                 var header = new EgmHeader
//                 {
//                     Seqno = _egmSequenceNumber++,
//                     Tm = (uint)DateTime.Now.Ticks,
//                     Mtype = EgmHeader.Types.MessageType.MsgtypeCorrection
//                 };
//
//                 message.Header = header;
//
//                 var planned = new EgmPlanned();
//                 var egmJoints = new EgmJoints();
//
//                 // 添加6个关节值
//                 for (int i = 0; i < 6; i++)
//                 {
//                     egmJoints.Joints.Add(joints[i]);
//                 }
//
//                 planned.Joints = egmJoints;
//                 message.Planned = planned;
//             }
//
//         #endregion
//
//         #region 接收与发送大函数
//
//             /// <summary>
//             /// 总监听循环
//             /// </summary>
//             private async Task MainListenLoop(UdpClient client, CancellationToken ct)
//             {
//                 while (!ct.IsCancellationRequested)
//                 {
//                     try
//                     {
//                         // 接收UDP数据
//                         var result = await client.ReceiveAsync();
//                         var data = result.Buffer;
//                         var sender = result.RemoteEndPoint;
//
//                         try
//                         {
//                             // 先尝试解析为Egm数据
//                             EgmRobot robotMessage = EgmRobot.Parser.ParseFrom(data);
//                             ProcessEgmMessage(robotMessage, sender);
//                             continue;
//                         }
//                         catch { /* 不是Egm消息 */ }
//
//                         try
//                         {
//                             // 再尝试解析为JSON数据
//                             string jsonString = Encoding.UTF8.GetString(data);
//                             var jsonMessage = JsonConvert.DeserializeObject<dynamic>(jsonString);
//
//                             if (jsonMessage?.Header != null)
//                             {
//                                 if (jsonMessage.Header.ToString() == "RL" && jsonMessage.Type?.ToString() == "POSE")
//                                 {
//                                     ProcessPoseMessage(jsonMessage, jsonMessage.TimeStamp?.ToString());
//                                 }
//
//                                 if (jsonMessage.Header.ToString() == "RL" && jsonMessage.Type?.ToString() == "CONTROL")
//                                 {
//                                     ProcessCtrlMessage(jsonMessage, jsonMessage.TimeStamp?.ToString());
//                                 }
//                             }
//                         }
//                         catch { /* 不是JSON消息 */ }
//                     }
//                     catch (OperationCanceledException)
//                     {
//                         break;
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Error($"[总监听]: 异常: {ex.Message}");
//                         await Task.Delay(1000, ct);
//                     }
//                 }
//             }
//
//             /// <summary>
//             /// RL和UE发送循环
//             /// </summary>
//             private async Task RlUeSendLoop(UdpClient client, IPEndPoint remoteEndPoint, int frequency, CancellationToken ct)
//             {
//                 int intervalMs = frequency;
//                 
//                 while (!ct.IsCancellationRequested)
//                 {
//                     try
//                     {
//                         // 获取当前状态
//                         var (robotStatus, trussStatus) = GetCurrentStatus();
//                         
//                         // 条件判定：机器人状态为4且桁架状态为3时发送Normal报文
//                         bool isNormal = (robotStatus == 4 && trussStatus == 3);
//                         
//                         if (isNormal)
//                         {
//                             // 正常发送
//                             double[] currentAxes = GetCurrentEightRax();
//                             var data = new
//                             {
//                                 Header = "Normal",
//                                 Timestamp = DateTime.Now.ToString("yyMMddHHmmssfff"),
//                                 J1 = currentAxes[0],
//                                 J2 = currentAxes[1],
//                                 J3 = currentAxes[2],
//                                 J4 = currentAxes[3],
//                                 J5 = currentAxes[4],
//                                 J6 = currentAxes[5],
//                                 TrussX = currentAxes[6],
//                                 TrussY = currentAxes[7],
//                                 RobotStatus = "Running",
//                                 TrussStatus = "Running"
//                             };
//                             
//                             byte[] sendData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
//                             await client.SendAsync(sendData, sendData.Length, remoteEndPoint);
//                             _errorSent = false; // 重置错误发送标志
//                                 
//                             // 调试日志
//                             // Log.Debug($"[发送]: 正常报文已发送到 {remoteEndPoint}");
//                         }
//                         else
//                         {
//                             // 异常发送 - 只发送一次
//                             if (!_errorSent)
//                             {
//                                 // 异常发送
//                                 var data = new
//                                 {
//                                     Header = "Error",
//                                     Timestamp = DateTime.Now.ToString("yyMMddHHmmssfff"),
//                                     RobotStatus = "Stopped",
//                                     TrussStatus = "Stopped"
//                                 };
//                                 
//                                 byte[] sendData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
//                                 await client.SendAsync(sendData, sendData.Length, remoteEndPoint);
//                                 _errorSent = true; // 标记错误已发送
//                                     
//                                 // 错误日志
//                                 Log.Warning($"[发送]: 错误报文已发送 - 机器人状态:{robotStatus}, 桁架状态:{trussStatus}");
//
//                             }
//                         }
//
//                         await Task.Delay(intervalMs, ct);
//                     }
//                     catch (OperationCanceledException)
//                     {
//                         break;
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Error($"[发送]: 异常: {ex.Message}");
//                         await Task.Delay(1000, ct);
//                     }
//                 }
//             }
//
//         #endregion
//         
//         #region 50%: 针对RL的接收
//
//         /// <summary>
//         /// 处理POSE消息 - 速度指令，需要积分
//         /// </summary>
//         private void ProcessPoseMessage(dynamic message, string timeStampStr)
//         {
//             try
//             {
//                 // 解析关节速度（度/秒）
//                 _currentJointVelocities[0] = (double)message.Rax1;
//                 _currentJointVelocities[1] = (double)message.Rax2;
//                 _currentJointVelocities[2] = (double)message.Rax3;
//                 _currentJointVelocities[3] = (double)message.Rax4;
//                 _currentJointVelocities[4] = (double)message.Rax5;
//                 _currentJointVelocities[5] = (double)message.Rax6;
//         
//                 // 解析桁架速度（毫米/秒）
//                 _currentTrussVelX = (double)message.TrussX;
//                 _currentTrussVelY = (double)message.TrussY;
//         
//                 _hasActiveCommand = true;
//         
//                 // Log.Debug($"[POSE消息]: 接收到速度指令 - J1_vel:{_currentJointVelocities[0]:F3},J2_vel:{_currentJointVelocities[1]:F3}, TrussX_vel:{_currentTrussVelX:F1}");
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"[POSE消息]: 解析失败 - {ex.Message}");
//             }
//         }
//         
//         /// <summary>
//         /// 处理CTRL消息
//         /// </summary>
//         private void ProcessCtrlMessage(dynamic message, string timeStampStr)
//         {
//             _hasActiveCommand = false;
//
//             // 后台线程：重启机器人
//             Task.Run(() =>
//             {
//                 try
//                 {
//                     RestartRobotCore();
//
//                     // 机器人恢复后，回到 UI 线程重新启动 EGM
//                     Dispatcher.Invoke(() =>
//                     {
//                         EgmStart(this, new RoutedEventArgs());
//                     });
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error(ex, "[CTRL消息]: 自动重启机器人失败: " + ex.Message);
//                 }
//             });
//         }
//
//
//         #endregion
//
//         #region 100%完成: 实时状态，用于RLUE的发送包（包含发送桁架）
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
//             // 后台线程数据存储
//             private volatile double[] _currentEightAxes = new double[8];
//             private volatile int _currentRobotStatus = -1;
//             private volatile int _currentTrussStatus = -1;
//             private CancellationTokenSource _dataUpdateCts;
//             private Task _dataUpdateTask;
//             
//             // 添加发送频率控制
//             private DateTime _lastRobotSendTime = DateTime.MinValue;
//             private DateTime _lastTrussSendTime = DateTime.MinValue;
//             
//             /// <summary>
//             /// 获取机器人发送频率
//             /// </summary>
//             private double GetRobotFrequency()
//             {
//                 int? freq = CheckFreTextBox(FreRobot);
//                 return freq ?? 100.0; // 默认100ms
//             }
//             
//             /// <summary>
//             /// 获取桁架发送频率
//             /// </summary>
//             private double GetTrussFrequency()
//             {
//                 int? freq = CheckFreTextBox(FreTruss);
//                 return freq ?? 500.0; // 默认500ms
//             }
//             
//             /// <summary>
//             /// 发送桁架位置到PLC
//             /// </summary>
//             private async Task SendTrussMessageToPlc(double trussX, double trussY)
//             {
//                 try
//                 {
//                     // 调用虚拟桁架的方法设置目标位置
//                     _virtualTruss.PlcGotoPositionQuick((float)trussX, (float)trussY,(float)_currentTrussVelX,(float)_currentTrussVelY);
//         
//                     // 调试日志（可选择性开启）
//                     // Log.Debug($"[桁架发送]: 已设置桁架位置 X:{trussX:F1}, Y:{trussY:F1}");
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[桁架发送]: 设置桁架位置失败: {ex.Message}");
//                 }
//             }
//
//             // 启动后台数据更新线程
//             private void StartDataUpdateThread()
//             {
//                 _dataUpdateCts = new CancellationTokenSource();
//                 _dataUpdateTask = Task.Run(async () => await RunDataUpdateLoop(_dataUpdateCts.Token));
//             }
//
//             // 后台数据更新循环
//             private async Task RunDataUpdateLoop(CancellationToken ct)
//             {
//                 while (!ct.IsCancellationRequested)
//                 {
//                     try
//                     {
//                         // 更新八轴数据
//                         double[] eightAxes = new double[8];
//                         var (_, _, _, _, _, _, _, j1, j2, j3, j4, j5, j6) = _virtualRobot.GetCurrentPose();
//                         eightAxes[0] = j1;
//                         eightAxes[1] = j2;
//                         eightAxes[2] = j3;
//                         eightAxes[3] = j4;
//                         eightAxes[4] = j5;
//                         eightAxes[5] = j6;
//                         
//                         var (trussX, trussY, _, _) = _virtualTruss.GetTrussPose();
//                         eightAxes[6] = trussX;
//                         eightAxes[7] = trussY;
//                         
//                         _currentEightAxes = eightAxes;
//                         
//                         // 更新状态数据
//                         _currentRobotStatus = _virtualRobot.GetVirRobotStatus();
//                         _currentTrussStatus = _virtualTruss.GetVirPlcStatus();
//                         
//                         var (robotText, robotColor) = GetRobotStatusInfo(_currentRobotStatus);
//                         var (plcText, plcColor) = GetPlcStatusInfo(_currentTrussStatus);
//                         
//                         try
//                         {
//                             Dispatcher.Invoke(() =>
//                             {
//                                 // 这里根据你实际的控件名改，我按你之前习惯先写一版
//                                 AbbJ1.Text = j1.ToString("F3");
//                                 AbbJ2.Text = j2.ToString("F3");
//                                 AbbJ3.Text = j3.ToString("F3");
//                                 AbbJ4.Text = j4.ToString("F3");
//                                 AbbJ5.Text = j5.ToString("F3");
//                                 AbbJ6.Text = j6.ToString("F3");
//
//                                 PlcX.Text = trussX.ToString("F4");
//                                 PlcY.Text = trussY.ToString("F4");
//                                 
//                                 AbbStatusText.Text = robotText;
//                                 AbbStatusText.Foreground = robotColor;
//                                  
//                                 PlcStatusText.Text = plcText;
//                                 PlcStatusText.Foreground = plcColor;
//                             });
//                         }
//                         catch
//                         {
//                             // 界面关闭等情况，忽略即可
//                         }
//                         
//                         // 积分计算：如果有速度指令，就进行积分
//                         if (_hasActiveCommand)
//                         {
//                             for (int i = 0; i < 6; i++)
//                             {
//                                 double displacement = _jointIntegrators[i].Integrate(_currentJointVelocities[i]);
//                                 _targetJointPositions[i] += displacement;
//                             }
//                 
//                             double trussXDisplacement = _trussXIntegrator.Integrate(_currentTrussVelX);
//                             double trussYDisplacement = _trussYIntegrator.Integrate(_currentTrussVelY);
//                             _targetTrussX += trussXDisplacement;
//                             _targetTrussY += trussYDisplacement;
//                 
//                             // 机器人发送频率控制
//                             DateTime now = DateTime.Now;
//
//                             // 使用已经在 UI 线程设置好的间隔字段
//                             if ((now - _lastRobotSendTime).TotalMilliseconds >= _robotIntervalMs)
//                             {
//                                 await SendJointMessageToRobot(_targetJointPositions);
//                                 _lastRobotSendTime = now;
//                             }
//
//                             if ((now - _lastTrussSendTime).TotalMilliseconds >= _trussIntervalMs)
//                             {
//                                 await SendTrussMessageToPlc(_targetTrussX, _targetTrussY);
//                                 _lastTrussSendTime = now;
//                             }
//
//                         }
//                         
//                         // 高频更新 - 10ms间隔
//                         await Task.Delay(10, ct);
//                     }
//                     catch (Exception ex)
//                     {
//                         Log.Error($"[后台数据更新]: 更新失败: {ex.Message}");
//                         await Task.Delay(100, ct); // 出错时稍作延迟
//                     }
//                 }
//             }
//
//             // 停止后台数据更新
//             private void StopDataUpdateThread()
//             {
//                 try
//                 {
//                     _dataUpdateCts?.Cancel();
//                     _dataUpdateTask?.Wait(1000); // 等待1秒让任务结束
//                     _dataUpdateCts?.Dispose();
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error($"[后台数据更新]: 停止失败: {ex.Message}");
//                 }
//             }
//
//             // 零延迟获取当前八轴数据
//             private double[] GetCurrentEightRax()
//             {
//                 return _currentEightAxes; // volatile读取，几乎零延迟
//             }
//
//             // 零延迟获取当前状态
//             private (int robotStatus, int trussStatus) GetCurrentStatus()
//             {
//                 return (_currentRobotStatus, _currentTrussStatus); // volatile读取，几乎零延迟
//             }
//
//         #endregion
//
//         #region 积分计算相关
//         
//             // 发送间隔（单位：ms），只由 UI 线程修改
//             private double _robotIntervalMs = 100;  // 默认 100ms
//             private double _trussIntervalMs = 500;  // 默认 500ms
//             
//             /// <summary>
//             /// 在 UI 线程上读取频率设置，更新后台用的间隔字段
//             /// </summary>
//             private void RefreshFrequenciesFromUi()
//             {
//                 // 这里本来就是在 UI 线程上调用 EgmStart，所以其实可以不加 Dispatcher
//                 // 如果你以后在别的线程调用，再加 Dispatcher 也可以
//                 int? freRobot = CheckFreTextBox(FreRobot);
//                 int? freTruss = CheckFreTextBox(FreTruss);
//
//                 // 这里要想清楚：你 TextBox 填的是 “ms” 还是 “Hz”
//                 // 你前面说 “频率是 ms 单位”，那就按 “周期(ms)” 来理解：
//                 _robotIntervalMs = freRobot ?? 100.0;  // 例如输入 4 → 4ms
//                 _trussIntervalMs = freTruss ?? 500.0;  // 例如输入 20 → 20ms
//             }
//
//         
//             public class Integrator
//             {
//                 private double _integral;
//                 private double _timeWindow; // 积分时间窗口（秒）
//         
//                 public Integrator(double frequency)
//                 {
//                     _timeWindow = 1.05 / frequency; // 每次积分的时间窗口
//                 }
//         
//                 /// <summary>
//                 /// 对速度进行固定时间窗口积分
//                 /// </summary>
//                 /// <param name="velocity">速度值</param>
//                 /// <returns>位移增量</returns>
//                 public double Integrate(double velocity)
//                 {
//                     // 位移 = 速度 × 时间窗口(1050ms)
//                     double displacement = velocity * _timeWindow;
//                     _integral += displacement;
//                     return displacement;
//                 }
//         
//                 public void Reset()
//                 {
//                     _integral = 0;
//                 }
//         
//                 public double GetCurrentIntegral()
//                 {
//                     return _integral;
//                 }
//             }
//
//             // 发送频率（从界面获取）
//             private int _sendingFrequency = 100; // 默认100Hz
//
//             // 积分器数组
//             private readonly Integrator[] _jointIntegrators = new Integrator[6];
//             private Integrator _trussXIntegrator;
//             private Integrator _trussYIntegrator;
//
//             // 当前目标位置（积分结果）
//             private double[] _targetJointPositions = new double[6];
//             private double _targetTrussX, _targetTrussY;
//
//             // 当前速度指令
//             private double[] _currentJointVelocities = new double[6];
//             private double _currentTrussVelX, _currentTrussVelY;
//
//             // 控制模式
//             private bool _hasActiveCommand;
//             
//             /// <summary>
//             /// 初始化积分器
//             /// </summary>
//             private void InitializeIntegrators()
//             {
//                 // 从界面获取发送频率
//                 int? freq = CheckFreTextBox(FreOutRlUe);
//                 _sendingFrequency = freq ?? 100; // 默认100Hz
//     
//                 for (int i = 0; i < 6; i++)
//                 {
//                     _jointIntegrators[i] = new Integrator(_sendingFrequency);
//                 }
//                 _trussXIntegrator = new Integrator(_sendingFrequency);
//                 _trussYIntegrator = new Integrator(_sendingFrequency);
//     
//                 // 从当前位置初始化目标位置
//                 var currentAxes = GetCurrentEightRax();
//                 for (int i = 0; i < 6; i++)
//                 {
//                     _targetJointPositions[i] = currentAxes[i];
//                 }
//                 _targetTrussX = currentAxes[6];
//                 _targetTrussY = currentAxes[7];
//             }
//
//             /// <summary>
//             /// 重置所有积分器
//             /// </summary>
//             private void ResetAllIntegrators()
//             {
//                 for (int i = 0; i < 6; i++)
//                 {
//                     _jointIntegrators[i]?.Reset();
//                 }
//                 _trussXIntegrator?.Reset();
//                 _trussYIntegrator?.Reset();
//     
//                 // 重置速度指令
//                 Array.Clear(_currentJointVelocities, 0, 6);
//                 _currentTrussVelX = 0;
//                 _currentTrussVelY = 0;
//                 _hasActiveCommand = false;
//             }
//
//             /// <summary>
//             /// 停止积分（当机器人停止时调用）
//             /// </summary>
//             private void StopIntegration()
//             {
//                 ResetAllIntegrators();
//             }
//
//         #endregion
//         
//         private void OnWindowDragMove(object sender, MouseButtonEventArgs e)
//         {
//             if (e.ChangedButton == MouseButton.Left)
//             {
//                 DragMove();
//             }
//         }
//         private void RobRestart(object sender, RoutedEventArgs e)
//         {
//             Task.Run(() =>
//             {
//                 try
//                 {
//                     RestartRobotCore();
//                 }
//                 catch (Exception ex)
//                 {
//                     Log.Error(ex, "[213]: 手动重启仿真机器人时发生异常");
//                 }
//             });
//         }
//         // 纯业务逻辑，不碰 UI
//         private void RestartRobotCore()
//         {
//             _virtualRobot.NewRestart();
//             Thread.Sleep(15000);
//             _virtualRobot.NewInit();
//         }
//
//         public MainWindow()
//         {
//             InitializeComponent();
//             _virtualRobot.Init();
//             _virtualTruss.Init();
//
//             // 先开后台数据持续读取
//             StartDataUpdateThread();
//             
//             // 初始化积分器
//             InitializeIntegrators();
//         }
//         protected override void OnClosed(EventArgs e)
//         {
//             StopMainListener();
//             StopRlSender();
//             StopUeSender();
//             base.OnClosed(e);
//         }
//
//         private void EgmStart(object sender, RoutedEventArgs e)
//         {
//             try
//             {
//                 // 启动总监听
//                 StartMainListener();
//                 
//                 // 1. 先从界面读取频率，更新间隔
//                 RefreshFrequenciesFromUi();
//                 
//                 // 2. 重置积分器，从当前位置开始
//                 ResetAllIntegrators();
//         
//                 // 3. 设置活动命令标志，开始积分计算
//                 _hasActiveCommand = true;
//
//                 StartRlSender();
//                 StartUeSender();
//         
//                 Log.Information("[EGM]: 总监听已启动，等待EGM消息...");
//                 MessageBox.Show("总监听已启动，请查看控制台输出的关节位置信息", "EGM测试", 
//                     MessageBoxButton.OK, MessageBoxImage.Information);
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"[EGM]: 启动失败: {ex.Message}");
//                 MessageBox.Show($"EGM启动失败: {ex.Message}", "错误", 
//                     MessageBoxButton.OK, MessageBoxImage.Error);
//             }
//         }
//
//         private void EgmStop(object sender, RoutedEventArgs e)
//         {
//             try
//             {
//                 // 停止总监听
//                 StopMainListener();
//                 
//                 // 停止积分
//                 StopIntegration();
//         
//                 Log.Information("[EGM]: 总监听已停止");
//                 MessageBox.Show("总监听已停止", "EGM测试", 
//                     MessageBoxButton.OK, MessageBoxImage.Information);
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"[EGM]: 停止失败: {ex.Message}");
//             }
//         }
//     }
// }