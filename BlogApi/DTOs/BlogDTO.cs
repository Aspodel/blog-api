using BlogApi.Core.Entities;
using BlogApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs
{
    [ModelBinder(typeof(MultipleSourcesModelBinder<BlogDTO>))]
    public class BlogDTO : BaseDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Slug { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        public BlogStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<string> Authors { get; set; } = Array.Empty<string>();
        public virtual ICollection<int> Categories { get; set; } = Array.Empty<int>();
    }
}
