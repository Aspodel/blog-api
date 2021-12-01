using System;
using System.Collections.Generic;

namespace BlogApi.Core.Entities
{
    public class Blog : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public BlogStatus Status { get; set; } = BlogStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Author> Authors { get; set; } = new HashSet<Author>();
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }

    public enum BlogStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
