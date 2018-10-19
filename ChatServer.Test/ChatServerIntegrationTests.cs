using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ChatServer.Test
{
    public class ChatServerIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public ChatServerIntegrationTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/api/time")]
        public async Task GetEndpoints(string url)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task PostMessage()
        {
            var client = _factory.CreateClient();

            var updateBook = new { receiver = "X011AAS", message = "Hi! It's me." };

            var content = new StringContent(JsonConvert.SerializeObject(updateBook), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/message", content);

            response.EnsureSuccessStatusCode();
        }
    }
}