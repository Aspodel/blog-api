using BlogApi.Contract;
using BlogApi.Core.Database;
using BlogApi.Core.Entities;
using BlogApi.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlogApi.Repository
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }

        public override IQueryable<Category> FindAll(Expression<Func<Category, bool>>? predicate = null)
            => _dbSet
                .WhereIf(predicate != null, predicate!)
                .Include(c => c.Blogs);

        public async Task<Category?> FindBySlug(string slug, CancellationToken cancellationToken = default)
            => await FindAll()
                    .Where(c => c.Slug == slug)
                    .FirstOrDefaultAsync(cancellationToken);
    }
}
