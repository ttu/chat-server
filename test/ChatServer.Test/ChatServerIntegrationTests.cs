using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;
using Xunit;

namespace ChatServer.Test
{
    public class ChatServerIntegrationTests
    {
        [Fact]
        public async Task WebSocket_ReceiveMessage()
        {
            var are = new AutoResetEvent(false);

            var webSoketMessages = new List<dynamic>();

            var websocket = new WebSocket($"ws://localhost:5000/ws");

            websocket.MessageReceived += (s, e) =>
            {
                var msg = JsonConvert.DeserializeObject<JToken>(e.Message);
                Assert.Equal("hello", msg["payload"].Value<string>());
                are.Set();
            };

            websocket.Opened += (s, e) =>
            {
                websocket.Send("username:timmy");
                are.Set();
            };

            websocket.Open();

            are.WaitOne(); // wait for open

            var client = new HttpClient();

            var message = new { receiver = "timmy", payload = "hello" };
            var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            var result = await client.PostAsync($"http://localhost:5000/api/send", content);
            result.EnsureSuccessStatusCode();

            are.WaitOne(); // wait for message

            websocket.Close();
        }
    }
}