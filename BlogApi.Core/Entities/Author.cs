using System.Collections.Generic;

namespace BlogApi.Core.Entities
{
    public class Author : User
    {
        public virtual ICollection<Blog> Blogs { get; set; } = new HashSet<Blog>();
    }
}
