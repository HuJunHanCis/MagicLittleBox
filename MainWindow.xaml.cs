using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Serilog;

namespace MagicLittleBox
{
    public partial class MainWindow : Window
    {
        // Legacy code from version 0.1.2 is preserved in
        // Legacy/MainWindow.Legacy_0.1.2.cs for reference during refactors.

        private UdpClient _udpListener;
        private CancellationTokenSource _udpListenerCts;

        public MainWindow()
        {
            InitializeComponent();
        }

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

        #region UDP监听区域

        private async void UdpListener()
        {
            var port = CheckValidPort(PortListener);
            if (port == null)
            {
                Log.Error("[UDP]: 无效的监听端口");
                MessageBox.Show(this, "监听端口无效，请输入1-65535之间的数字。", "UDP监听", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _udpListenerCts?.Cancel();
            _udpListenerCts?.Dispose();
            _udpListener?.Close();
            _udpListener?.Dispose();

            _udpListenerCts = new CancellationTokenSource();

            try
            {
                _udpListener = new UdpClient(port.Value);
                var token = _udpListenerCts.Token;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            var result = await _udpListener.ReceiveAsync(token);
                            Log.Information("[UDP]: 收到来自 {Remote} 的 {Length} 字节数据", result.RemoteEndPoint, result.Buffer.Length);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Log.Information("[UDP]: 监听已取消");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[UDP]: 监听循环异常");
                    }
                }, token);

                Log.Information("[UDP]: UDP监听端口已启动: {Port}", port.Value);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[UDP]: 启动监听失败");
                MessageBox.Show(this, "无法启动UDP监听，请检查端口是否被占用。", "UDP监听", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region CHECK检查区域

        private int? CheckValidFreq(TextBox textBox)
        {
            if (textBox == null)
            {
                return null;
            }

            var text = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text) || !int.TryParse(text, out var freq))
            {
                return null;
            }

            if (freq < 4 || freq > 1000)
            {
                return null;
            }

            if (freq % 4 != 0)
            {
                var nearest = (int)(Math.Round(freq / 4.0, MidpointRounding.AwayFromZero) * 4);
                freq = nearest;
                textBox.Text = freq.ToString();
            }

            return freq;
        }

        private int? CheckValidPort(TextBox textBox)
        {
            if (textBox == null)
            {
                return null;
            }

            var text = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text) || !int.TryParse(text, out var port))
            {
                return null;
            }

            if (port <= 0 || port >= 65535)
            {
                return null;
            }

            return port;
        }

        private string CheckValidAddr(TextBox textBox)
        {
            if (textBox == null)
            {
                return null;
            }

            var text = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var pattern = @"^(25[0-5]|2[0-4]\d|1?\d?\d)(\.(25[0-5]|2[0-4]\d|1?\d?\d)){3}$";
            return Regex.IsMatch(text, pattern) ? text : null;
        }

        #endregion
    }
}
