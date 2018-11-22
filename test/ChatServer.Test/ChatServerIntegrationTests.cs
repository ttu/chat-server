using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;
using Xunit;

namespace ChatServer.Test
{
    public class Client
    {
        private readonly WebSocket _webscoket;

        public Client(string userName, AutoResetEvent are, Action<MessageReceivedEventArgs> receiveFunc)
        {
            _webscoket = new WebSocket($"ws://localhost:5000/ws");

            _webscoket.Opened += (s, e) =>
            {
                _webscoket.Send($"username:{userName}");
                are.Set();
            };

            _webscoket.MessageReceived += (s, e) =>
            {
                receiveFunc(e);
                are.Set();
            };

            _webscoket.Open();
        }

        public void Close()
        {
            _webscoket.Close();
        }
    }

    public class ChatServerIntegrationTests
    {
        [Fact]
        public async Task WebSocket_ReceiveMessage()
        {
            var are = new AutoResetEvent(false);

            var action = new Action<MessageReceivedEventArgs>(e =>
            {
                var msg = JsonConvert.DeserializeObject<JToken>(e.Message);
                Assert.Equal("hello", msg["payload"].Value<string>());
            });

            var chatClient = new Client("timmy", are, action);

            are.WaitOne(); // wait for open

            var chatClient2 = new Client("james", are, action);

            are.WaitOne(); // wait for open

            var client = new HttpClient();

            // james sends a message to timmy
            var message = new { receiver = "timmy", payload = "hello" };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            content.Headers.Add("X-Username", "james");
            var result = await client.PostAsync($"http://localhost:5000/api/send", content);
            result.EnsureSuccessStatusCode();

            are.WaitOne(); // wait for message

            // timmy sends a message to james
            message = new { receiver = "james", payload = "hello" };
            content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            content.Headers.Add("X-Username", "timmy");
            result = await client.PostAsync($"http://localhost:5000/api/send", content);
            result.EnsureSuccessStatusCode();

            are.WaitOne(); // wait for message

            chatClient.Close();
            chatClient2.Close();
        }
    }
}