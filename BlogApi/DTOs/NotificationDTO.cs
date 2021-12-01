using System;
using BlogApi.Core;
using BlogApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.DTOs
{
    [ModelBinder(typeof(MultipleSourcesModelBinder<NotificationDTO>))]
    public class NotificationDTO : BaseDTO
    {
        public string RecipientId { get; set; } = string.Empty;
        public string? SenderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
    }
}