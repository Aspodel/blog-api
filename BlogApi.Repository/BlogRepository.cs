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
    public class BlogRepository : BaseRepository<Blog>, IBlogRepository
    {
        public BlogRepository(ApplicationDbContext context) : base(context) { }

        public override IQueryable<Blog> FindAll(Expression<Func<Blog, bool>>? predicate = null)
            => _dbSet
                .WhereIf(predicate != null, predicate!)
                .Include(b => b.Authors)
                .Include(b => b.Categories);

        public override async Task<Blog?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
            => await FindAll(b => b.Id == id)
                    // .Include(b => b.Authors)
                    // .Include(b => b.Categories)
                    .FirstOrDefaultAsync(cancellationToken);

        public async Task<Blog?> FindBySlug(string slug, CancellationToken cancellationToken = default)
            => await FindAll()
                .Where(b => b.Slug == slug)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
