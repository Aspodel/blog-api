using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BlogApi.Core.Entities
{
    public class Role : IdentityRole
    {
        public virtual ICollection<UserRole> UserRoles { get; } = new HashSet<UserRole>();
    }
}
