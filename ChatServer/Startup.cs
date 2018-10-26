using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using StackExchange.Redis;
using System;

namespace ChatServer
{
    public class Startup
    {
        public const string TestingEnv = "Testing";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            // Use implementationfactory for lazy initialization
            services.AddSingleton(s => ConnectionMultiplexer.Connect(Configuration.GetValue<string>("Connections:Redis")));
            services.AddScoped(s => s.GetService<ConnectionMultiplexer>().GetDatabase());

            services.AddSingleton(new ConnectionFactory() { HostName = Configuration.GetValue<string>("Connections:RabbitMQ") });
            services.AddSingleton(s => s.GetService<ConnectionFactory>().CreateConnection());
            services.AddScoped(s => s.GetService<IConnection>().CreateModel());

            services.AddScoped<IClientRegistryService, ClientRegistryService>();
            services.AddScoped<IMessageSender, MessageSender>();
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
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
            routeBuilder.MapPost("api/receive", context => Endpoints.Send(context));

            var routes = routeBuilder.Build();
            app.UseRouter(routes);

            if (env.EnvironmentName != Startup.TestingEnv)
            {
                TestConnections(services);
            }
        }

        private void TestConnections(IServiceProvider services)
        {
            var redisDb = services.GetService<IDatabase>();
            var rabbitmq = services.GetService<IConnection>();
        }
    }
}