using BlogApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.DTOs
{
    [ModelBinder(typeof(MultipleSourcesModelBinder<CategoryDTO>))]
    public class CategoryDTO : BaseDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }
}
