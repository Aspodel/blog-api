using BlogApi.Hubs;
using BlogApi.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlogApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        public NotificationService(IHubContext<NotificationHub> notificationHubContext)
        {
            _notificationHubContext = notificationHubContext;
        }

        public int numberOnlineUsers = NotificationHub.Count;

        public Task SendToAll(NotificationModel model)
        {
            return _notificationHubContext.Clients.All.SendAsync("ReceiveNotification", model);
        }

        public Task SendToUser(string receiverId, NotificationModel model)
        {
            return _notificationHubContext.Clients.User(receiverId).SendAsync("ReceiveNotification", model);
        }
    }
}