using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace BlogApi.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public static int Count = 0;
        // public override Task OnConnectedAsync()
        // {
        //     Count++;
        //     base.OnConnectedAsync();
        //     Clients.All.SendAsync("updateCount", Count);
        //     return Task.CompletedTask;
        // }
        // public override Task OnDisconnectedAsync(Exception? exception)
        // {
        //     Count--;
        //     base.OnDisconnectedAsync(exception);
        //     Clients.All.SendAsync("updateCount", Count);

        //     string name = Context!.User!.Identity!.Name!;
        //     Console.WriteLine(name);

        //     return Task.CompletedTask;
        // }
    }
}