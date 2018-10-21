using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ChatServer.Test
{
    public class MyClientRegistryService : IClientRegistryService
    {
        public string SavedIp { get; set; }

        public void Register(string serverIp, string clientIp)
        {
            SavedIp = serverIp;
        }
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

            var updateBook = new { receiver = "X011AAS", message = "Hi! It's me." };

            var content = new StringContent(JsonConvert.SerializeObject(updateBook), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/message", content);

            response.EnsureSuccessStatusCode();

            Assert.Equal("10.0.0.1", _service.SavedIp);
        }
    }
}