using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.Builder;
using System;
using Microsoft.Extensions.Configuration;

namespace ChatServer.Test
{
    public class FakeRemoteIpAddressMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IPAddress fakeIpAddress = IPAddress.Parse("127.168.1.10");

        public FakeRemoteIpAddressMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Connection.RemoteIpAddress = fakeIpAddress;

            await this.next(httpContext);
        }
    }

    public class MyClientRegistryService : IClientRegistryService
    {
        private Dictionary<string, string> _clients = new Dictionary<string, string>();

        public void FireRegister(string serverIp, string clientIp) => _clients.TryAdd(clientIp, serverIp);

        public Task<string> Get(string clientIp) => Task.FromResult(_clients[clientIp]);
    }

    public class StartupStub : ChatServer.Startup
    {
        public StartupStub(IConfiguration configuration, IHostingEnvironment environment) : base(configuration, environment)
        {
        }

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            app.UseMiddleware<FakeRemoteIpAddressMiddleware>();
            base.Configure(app, env, services);
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
            .UseStartup<StartupStub>()
            .UseEnvironment("Testing")
            .ConfigureTestServices(services =>
            {
                var con = Substitute.For<IConnectionMultiplexer>();
                services.AddSingleton(con);

                var msg = Substitute.For<IMessageSender>();
                services.AddSingleton(msg);

                services.AddSingleton<IClientRegistryService>(_service);
            }));
        }

        [Fact]
        public async Task PostMessage()
        {
            var client = _factory.CreateClient();

            var clientIp = "127.168.1.10";

            var updateBook = new { receiver = "X011AAS", message = "Hi! It's me." };

            var content = new StringContent(JsonConvert.SerializeObject(updateBook), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/send", content);

            response.EnsureSuccessStatusCode();

            Assert.Equal("localhost", await _service.Get(clientIp));
        }
    }
}