using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
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
    }
}