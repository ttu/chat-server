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
            return context.Response.WriteAsync("Hello");
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

        public static Task Auth(HttpContext context)
        {
            return context.Response.WriteAsync($"Authenticate");
        }

        public static Task Logout(HttpContext context)
        {
            return context.Response.WriteAsync($"Logout");
        }

        private static string _messageId = "message";
        private static string _receiverId = "receiver";

        public static async Task PostMessage(HttpContext context)
        {
            var reader = new StreamReader(context.Request.Body);
            string text = await reader.ReadToEndAsync();

            // TODO: Get IP
            var ip = context.Connection.RemoteIpAddress;

            if (!text.Contains(_messageId) || !text.Contains(_receiverId))
                throw new Exception("Not valid message");

            var service =  context.RequestServices.GetRequiredService<IClientRegistryService>();
            service.Register("10.0.0.1", ip?.ToString());

            await context.Response.WriteAsync($"Post");
        }
    }
}