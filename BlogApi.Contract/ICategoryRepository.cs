using BlogApi.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace BlogApi.Contract
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<Category?> FindBySlug(string slug, CancellationToken cancellationToken = default);
    }
}
