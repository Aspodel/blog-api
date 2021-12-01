using BlogApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlogApi.Services
{
    public interface INotificationService
    {
        Task SendToAll(NotificationModel model);
        Task SendToUser(string receiverId, NotificationModel model);
    }
}