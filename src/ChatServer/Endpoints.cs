using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChatServer
{
    public static class Endpoints
    {
        public static Task Hello(HttpContext context)
        {
            return context.Response.WriteAsync("Hello from ChatServer");
        }

        public static Task Time(HttpContext context)
        {
            return context.Response.WriteAsync(DateTime.Now.ToShortTimeString());
        }

        public static Task Values(HttpContext context)
        {
            return context.Response.WriteAsync($"You wrote: {context.GetRouteValue("id")}");
        }

        public static Task Name(HttpContext context)
        {
            return context.Response.WriteAsync($"You wrote: {context.GetRouteValue("name")}");
        }

        public static Task Login(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress;
            var host = context.Request.Host.Value;

            var service = context.RequestServices.GetRequiredService<IClientRegistryService>();
            service.FireRegister(host, ip?.ToString());

            return context.Response.WriteAsync($"Login");
        }

        public static Task Logout(HttpContext context)
        {
            return context.Response.WriteAsync($"Logout");
        }

        private static string _messageId = "message";
        private static string _receiverId = "receiver";

        public static async Task Send(HttpContext context)
        {
            var reader = new StreamReader(context.Request.Body);
            string text = await reader.ReadToEndAsync();

            var ip = context.Connection.RemoteIpAddress;

            if (!text.Contains(_messageId) || !text.Contains(_receiverId))
                throw new Exception("Not valid message");

            var host = context.Request.Host.Value;

            var service = context.RequestServices.GetRequiredService<IClientRegistryService>();
            service.FireRegister(host, ip?.ToString());

            var messageService = context.RequestServices.GetRequiredService<IMessageSender>();
            messageService.Send(text);

            await context.Response.WriteAsync($"Send");
        }

        public static async Task Receive(HttpContext context)
        {
            var reader = new StreamReader(context.Request.Body);
            string text = await reader.ReadToEndAsync();

            await context.Response.WriteAsync($"Receive");
        }
    }
}