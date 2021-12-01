using System;
using BlogApi.Core;

namespace BlogApi.Models
{
    public class NotificationModel
    {
        public string? SenderId { get; set; }
        public NotificationType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}