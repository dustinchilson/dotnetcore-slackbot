using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Slackbot
{
    internal class SocketConnection
    {
        private readonly ILogger<SocketConnection> _logger;
        public string Url { get; }
        public event EventHandler<string> OnData;
        private ClientWebSocket _socket;

        public SocketConnection(string url, ILogger<SocketConnection> logger = null)
        {
            _logger = logger;
            this.Url = url;
            this.Connect();
        }

        public async Task SendDataAsync(ArraySegment<byte> data)
        {
            await _socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async void Connect()
        {
            try
            {
                _socket = new ClientWebSocket();
                await _socket.ConnectAsync(new Uri(this.Url), CancellationToken.None);

                var receiveBytes = new byte[4096];
                var receiveBuffer = new ArraySegment<byte>(receiveBytes);

                while (_socket.State == WebSocketState.Open)
                {
                    var receivedMessage = await _socket.ReceiveAsync(receiveBuffer, CancellationToken.None);
                    if (receivedMessage.MessageType == WebSocketMessageType.Close)
                    {
                        await
                            _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing websocket",
                                CancellationToken.None);
                    }
                    else
                    {
                        var messageBytes =
                            receiveBuffer.Skip(receiveBuffer.Offset).Take(receivedMessage.Count).ToArray();

                        var rawMessage = new UTF8Encoding().GetString(messageBytes);
                        OnData?.Invoke(this, rawMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.Message);
                this.Connect();
            }
        }
    }
}