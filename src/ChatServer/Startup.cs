using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace ChatServer
{
    public class Startup
    {
        public const string TestingEnv = "Testing";
        public static string OwnHost;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            OwnHost = Configuration.GetValue<string>("OwnHost");

            services.AddRouting();

            // Use implementationfactory for lazy initialization
            services.AddSingleton(s => ConnectionMultiplexer.Connect(Configuration.GetValue<string>("Connections:Redis")));
            services.AddScoped(s => s.GetService<ConnectionMultiplexer>().GetDatabase());

            services.AddSingleton(new ConnectionFactory() { HostName = Configuration.GetValue<string>("Connections:RabbitMQ") });
            services.AddSingleton(s => s.GetService<ConnectionFactory>().CreateConnection());
            services.AddScoped(s => s.GetService<IConnection>().CreateModel());

            services.AddScoped<IClientRegistryService, ClientRegistryService>();
            services.AddScoped<IMessageSender, MessageSender>();

            services.AddSingleton<WebSocketService>();
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            var routeBuilder = new RouteBuilder(app);

            routeBuilder.MapGet("", (context) => Endpoints.Hello(context));
            routeBuilder.MapGet("api/time", (context) => Endpoints.Time(context));
            routeBuilder.MapGet("api/values/{id:int}", (context) => Endpoints.Values(context));
            routeBuilder.MapGet("api/name/{name?}", (context) => Endpoints.Name(context));
            routeBuilder.MapPost("api/login", context => Endpoints.Login(context));
            routeBuilder.MapPost("api/logout", context => Endpoints.Logout(context));
            routeBuilder.MapPost("api/send", context => Endpoints.Send(context));
            routeBuilder.MapPost("api/receive", context => Endpoints.Receive(context));

            var routes = routeBuilder.Build();
            app.UseRouter(routes);

            app.UseWebSockets();

            app.UseMiddleware<WebSocketMiddleware>();

            if (env.EnvironmentName == "Docker")
            {
                logger.LogInformation("Waiting for services");
                WaitConnections(services);
            }

            if (env.EnvironmentName != Startup.TestingEnv)
            {
                TestConnections(services);
            }
        }

        private void WaitConnections(IServiceProvider services)
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

        private void TestConnections(IServiceProvider services)
        {
            var redisDb = services.GetService<IDatabase>();
            var rabbitmq = services.GetService<IConnection>();
        }
    }
}