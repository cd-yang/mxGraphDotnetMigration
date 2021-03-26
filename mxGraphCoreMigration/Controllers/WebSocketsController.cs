using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mxGraphCoreMigration.Controllers
{
    [ApiController]
    public class WebSocketsController : Controller
    {
        private readonly ILogger<WebSocketsController> _logger;

        public WebSocketsController(ILogger<WebSocketsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// client 使用 ws://127.0.0.1:5000/ws 连接到这里
        /// </summary>
        /// <returns></returns>
        [HttpGet("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _logger.Log(LogLevel.Information, "WebSocket connection established");
                await Echo(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                TrimTrailingBytes(ref buffer, (byte)'\0');

                var receivedText = Encoding.UTF8.GetString(buffer);
                _logger.Log(LogLevel.Information, "收到消息：" + receivedText);

                var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {receivedText}");
                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg), result.MessageType, result.EndOfMessage, CancellationToken.None);

                buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger.Log(LogLevel.Information, "WebSocket connection closed");
        }

        private void TrimTrailingBytes(ref byte[] buffer, byte trimValue)
        {
            int i = buffer.Length;

            while (i > 0 && buffer[--i] == trimValue)
            {
                ; // no-op by design
            }

            Array.Resize(ref buffer, i + 1);

            return;
        }
    }
}
