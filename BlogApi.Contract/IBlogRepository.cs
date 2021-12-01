using BlogApi.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace BlogApi.Contract
{
    public interface IBlogRepository : IBaseRepository<Blog>
    {
        Task<Blog?> FindBySlug(string slug, CancellationToken cancellationToken = default);
    }
}
