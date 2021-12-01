using BlogApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs
{
    [ModelBinder(typeof(MultipleSourcesModelBinder<RoleDTO>))]
    public class RoleDTO : BaseDTO<string>
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
