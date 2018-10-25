using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace ChatServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            services.AddSingleton(ConnectionMultiplexer.Connect(Configuration.GetValue<string>("Connections:Redis")));
            services.AddScoped(s => s.GetService<ConnectionMultiplexer>().GetDatabase());

            services.AddSingleton(new ConnectionFactory() { HostName = Configuration.GetValue<string>("Connections:RabbitMQ") });
            services.AddSingleton(s => s.GetService<ConnectionFactory>().CreateConnection());
            services.AddScoped(s => s.GetService<IConnection>().CreateModel());

            services.AddScoped<IClientRegistryService, ClientRegistryService>();
            services.AddScoped<IMessageSender, MessageSender>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            var routeBuilder = new RouteBuilder(app);

            routeBuilder.MapGet("", (context) => Endpoints.Hello(context));
            routeBuilder.MapGet("api/time", (context) => Endpoints.Time(context));
            routeBuilder.MapGet("api/values/{id:int}", (context) => Endpoints.Values(context));
            routeBuilder.MapGet("api/name/{name?}", (context) => Endpoints.Name(context));
            routeBuilder.MapPost("api/auth", context => Endpoints.Auth(context));
            routeBuilder.MapPost("api/logout", context => Endpoints.Logout(context));
            routeBuilder.MapPost("api/message", context => Endpoints.PostMessage(context));

            var routes = routeBuilder.Build();
            app.UseRouter(routes);

            TestConnections(services);
        }

        private void TestConnections(IServiceProvider services)
        {
            var redisDb = services.GetService<IDatabase>();
            var rabbitmq = services.GetService<IConnection>();
        }
    }
}