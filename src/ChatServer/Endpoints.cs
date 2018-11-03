using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ChatServer
{
    public static class Endpoints
    {
        private static string _payloadId = "payload";
        private static string _receiverId = "receiver";

        public static Task Hello(HttpContext context)
        {
            return context.Response.WriteAsync("Hello from ChatServer");
        }

        public static async Task GetUserStatus(HttpContext context)
        {
            string username = context.GetRouteValue("username") as string;

            var service = context.RequestServices.GetRequiredService<IClientRegistryService>();
            var found = await service.Get(username);

            await context.Response.WriteAsync($"{username} is {(string.IsNullOrEmpty(found) ? "offline" : "online")}");
        }

        public static Task Login(HttpContext context)
        {
            var userName = "TODO";

            var service = context.RequestServices.GetRequiredService<IClientRegistryService>();
            service.FireRegister(Startup.OwnHost, userName);

            return context.Response.WriteAsync($"Login");
        }

        public static Task Logout(HttpContext context)
        {
            return context.Response.WriteAsync($"Logout");
        }

        public static async Task Send(HttpContext context)
        {
            var reader = new StreamReader(context.Request.Body);
            string text = await reader.ReadToEndAsync();
            string sender = context.Request.Headers["X-Username"].ToString();

            if (string.IsNullOrEmpty(sender))
                throw new Exception("Sender not authenticated");

            if (!text.Contains(_payloadId) || !text.Contains(_receiverId))
                throw new Exception("Not valid message");

            Handle(text,
                sender,
                context.RequestServices.GetRequiredService<ILogger<Startup>>(),
                context.RequestServices.GetRequiredService<IClientRegistryService>(),
                context.RequestServices.GetRequiredService<IMessageSender>());

            await context.Response.WriteAsync($"Send");
        }

        private static void Handle(string message, string senderName, ILogger<Startup> logger, IClientRegistryService clientRegister, IMessageSender messageSender)
        {
            var tt = JsonConvert.DeserializeObject<Message>(message);

            logger.LogInformation($"FireRegister - Server: {Startup.OwnHost} User: {tt.Receiver}");

            // TODO: Same user in multiple server
            clientRegister.FireRegister(Startup.OwnHost, senderName);
            messageSender.Send(message);
        }

        public static async Task Receive(HttpContext context)
        {
            var reader = new StreamReader(context.Request.Body);
            string text = await reader.ReadToEndAsync();
            var message = JsonConvert.DeserializeObject<Message>(text);

            var _logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
            _logger.LogInformation($"New message for: {message.Receiver}");

            var service = context.RequestServices.GetRequiredService<WebSocketService>();
            var success = await service.Send(message);

            context.Response.StatusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync($"Receive");
        }
    }

    public class Message
    {
        public string Receiver { get; set; }
        public string Sender { get; set; }
        public string Payload { get; set; }
    }
}