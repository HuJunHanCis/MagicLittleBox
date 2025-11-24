using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Serilog;

namespace MagicLittleBox
{
    public partial class MainWindow
    {
        private UdpClient _udpListener;
        private CancellationTokenSource _udpListenerCts;

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
    }
}
