using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddSingleton(ConnectionMultiplexer.Connect(Configuration.GetValue<string>("ConnectionStrings:Redis")));

            services.AddScoped<IClientRegistryService, ClientRegistryService>();
            services.AddScoped(s => s.GetService<ConnectionMultiplexer>().GetDatabase());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
        }
    }
}