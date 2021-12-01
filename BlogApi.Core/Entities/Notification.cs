using BlogApi.Core.Entities;
using System;

namespace BlogApi.Core
{
    public class Notification : BaseEntity
    {
        public string RecipientId { get; set; } = string.Empty;
        public string? SenderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public User? Recipient { get; set; }
        public User? Sender { get; set; }
    }

    public enum NotificationType
    {
        Message,
        System
    }
}