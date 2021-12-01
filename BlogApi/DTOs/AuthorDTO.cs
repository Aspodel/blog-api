using BlogApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs
{
    [ModelBinder(typeof(MultipleSourcesModelBinder<AuthorDTO>))]
    public class AuthorDTO
    {
        [FromRoute]
        public string Guid { get; set; } = string.Empty;

        public string? Username { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public bool? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool? IsActive { get; set; }

        public ICollection<string> Roles { get; set; } = Array.Empty<string>();
        public ICollection<int> Blogs { get; set; } = Array.Empty<int>();
    }
}
