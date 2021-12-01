using System.Collections.Generic;

namespace BlogApi.Core.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<Blog> Blogs { get; set; } = new HashSet<Blog>();
    }
}
