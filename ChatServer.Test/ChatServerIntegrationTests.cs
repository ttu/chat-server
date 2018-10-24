using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ChatServer.Test
{
    public class MyClientRegistryService : IClientRegistryService
    {
        private Dictionary<string, string> _clients = new Dictionary<string, string>();

        public void Register(string serverIp, string clientIp) => _clients.TryAdd(clientIp, serverIp);

        public Task<string> Get(string clientIp) => Task.FromResult(_clients[clientIp]);
    }

    public class ChatServerIntegrationTestsInject : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly MyClientRegistryService _service;
        private readonly TestServer _factory;

        public ChatServerIntegrationTestsInject()
        {
            _service = new MyClientRegistryService();

            _factory = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>()
            .ConfigureTestServices(services =>
            {
                //services.AddScoped<IClientRegistryService, MyClientRegistryService>();
                services.AddSingleton<IClientRegistryService>(_service);
            }));
        }

        [Fact]
        public async Task PostMessage()
        {
            var client = _factory.CreateClient();

            var clientIp = "192.168.0.1";

            var updateBook = new { receiver = "X011AAS", message = "Hi! It's me." };

            var content = new StringContent(JsonConvert.SerializeObject(updateBook), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/message", content);

            response.EnsureSuccessStatusCode();

            Assert.Equal("10.0.0.1", await _service.Get(clientIp));
        }
    }
}