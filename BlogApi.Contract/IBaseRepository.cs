using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlogApi.Contract
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> FindAll(Expression<Func<T, bool>>? predicate = null);
        Task<T?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
