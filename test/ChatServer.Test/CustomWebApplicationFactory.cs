using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Test
{
    public class CustomWebApplicationFactory<TStartup>
     : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Startup.TestingEnv);

            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IClientRegistryService, MyClientRegistryService>();
            });
        }
    }
}