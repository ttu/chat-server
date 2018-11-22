using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatBroker
{
    public class Startup
    {
        public static string BrodcastHost;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddSingleton(s => ConnectionMultiplexer.Connect(Configuration.GetValue<string>("Connections:Redis")));

            services.AddSingleton(new ConnectionFactory() { HostName = Configuration.GetValue<string>("Connections:RabbitMQ") });
            services.AddSingleton(s => s.GetService<ConnectionFactory>().CreateConnection());

            //services.AddSingleton(new MessageStore(Configuration.GetValue<string>("Connections:Postgres")));
            services.AddSingleton(new InMemoryMessageStore());

            services.AddSingleton(s => new MessageHandler(
                            s.GetService<IConnection>(),
                            s.GetService<ConnectionMultiplexer>(),
                            s.GetService<IHttpClientFactory>(),
                            s.GetService<MessageStore>(),
                            s.GetService<ILogger<MessageHandler>>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello from ChatBroker");
            });

            // Docker compose doesn't set ASPNETCORE_ENVIRONMENT correctly, so use another env variable
            if (Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER") || env.EnvironmentName == "Docker")
            {
                logger.LogInformation("Waiting for services");
                WaitConnections();
            }

            if (env.EnvironmentName != "Testing")
            {
                var handler = services.GetService<MessageHandler>();
            }
        }

        private void WaitConnections()
        {
            void ExecuteWhileTrue(Action act)
            {
                while (true)
                {
                    try
                    {
                        act();
                        return;
                    }
                    catch (Exception) { }

                    Task.Delay(1000).Wait();
                }
            }

            var redisTest = new Action(() => ConnectionMultiplexer.Connect(Configuration.GetValue<string>("Connections:Redis")));
            var rabbitTest = new Action(() => new ConnectionFactory() { HostName = Configuration.GetValue<string>("Connections:RabbitMQ") }.CreateConnection());

            ExecuteWhileTrue(redisTest);
            ExecuteWhileTrue(rabbitTest);
        }
    }
}