using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    public class WebSocketService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();
        private readonly ConcurrentDictionary<string, string> _userNameCollection = new ConcurrentDictionary<string, string>();
        private readonly ILogger<WebSocketService> _logger;

        private readonly JsonSerializerSettings _camelCaseSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public WebSocketService(ILogger<WebSocketService> logger)
        {
            _logger = logger;
        }

        public void AddConnection(WebSocket webSocket)
        {
            var socketHash = webSocket.GetHashCode().ToString();
            _sockets.TryAdd(socketHash, webSocket);
        }

        public void AddName(WebSocket webSocket, string userName)
        {
            userName = userName.Replace("\0", "");

            _logger.LogInformation($"New user: {userName}");

            var socketHash = webSocket.GetHashCode().ToString();
            _userNameCollection.TryAdd(userName, socketHash);
        }

        public void RemoveConnection(WebSocket webSocket)
        {
            var socketHash = webSocket.GetHashCode().ToString();

            _sockets.TryRemove(socketHash, out WebSocket toRemove);
            var userToRemove = _userNameCollection.Where(kvp => kvp.Value == socketHash).FirstOrDefault();
            _userNameCollection.TryRemove(userToRemove.Key, out string hash);
        }

        public async Task<bool> Send(Message message)
        {
            if (_userNameCollection.TryGetValue(message.Receiver, out string hash) == false)
                return false;

            var socket = _sockets[hash];

            var text = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, _camelCaseSettings));
            var buffer = new ArraySegment<byte>(text);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

            _logger.LogInformation($"Sent message for: {message.Receiver}");

            return true;
        }
    }

    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketService _socketService;

        public WebSocketMiddleware(RequestDelegate next, WebSocketService socketService)
        {
            _next = next;
            _socketService = socketService;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            _socketService.AddConnection(webSocket);

            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<Byte>(new Byte[1024]);
                var received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                switch (received.MessageType)
                {
                    case WebSocketMessageType.Text:
                        var content = Encoding.UTF8.GetString(buffer);
                        if (content.StartsWith("username"))
                            _socketService.AddName(webSocket, content.Split(":")[1]);
                        continue;

                    case WebSocketMessageType.Close:
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        _socketService.RemoveConnection(webSocket);
                        return;
                }
            }
        }
    }
}