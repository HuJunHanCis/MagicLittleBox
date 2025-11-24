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

        #region WPF相关功能

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

        #region CHECK检查区域

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

        #endregion

        #region 界面按钮功能区

            // RlListener 按钮是否处于监听状态
            private bool _rlListenerEnabled = false;
            private void RlListener_Click(object sender, RoutedEventArgs e)
            {
                // ToggleButton 本身的状态
                bool isChecked = RlListener.IsChecked == true;

                _rlListenerEnabled = isChecked;   // 更新字段（你要求的内容）

                if (isChecked)
                {
                    Log.Information("[RL监听]: RL 监听已开启");
                    UdpListener();      // 如果希望点开就启动监听
                }
                else
                {
                    Log.Information("[RL监听]: RL 监听已关闭");
                }
            }
        
        #endregion

        #region UDP监听区域

            // UDP 监听用到的字段
            private UdpClient _udpListenerClient;
            private CancellationTokenSource _udpListenerCts;

            /// <summary>
            /// 根据界面上的 PortListener TextBox 的值开启 UDP 监听服务
            /// </summary>
            private void UdpListener()
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
                    MessageBox.Show(
                        "监听端口不合法，请输入 1~65534 之间的整数端口号。",
                        "端口错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

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
                                            RlConnection.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4CAF50"));
                                        });

                                        Task.Delay(500).ContinueWith(_ =>
                                        {
                                            try
                                            {
                                                Dispatcher.Invoke(() =>
                                                {
                                                    RlConnection.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#999999"));
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
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[UDP]: 监听循环异常");
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
        
        private void ProcessPoseMessage(EgmRobot robotMessage, IPEndPoint sender)
        {
            
        }
        
        private void ProcessCtrlMessage(EgmRobot robotMessage, IPEndPoint sender)
        {
            
        }

        #endregion



        
        public MainWindow()
        {
            InitializeComponent();
        }

    }
}

