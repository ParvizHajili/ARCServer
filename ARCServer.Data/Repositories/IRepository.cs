using System.Linq.Expressions;
using ARCServer.Domain.Entities;

namespace ARCServer.Data.Repositories
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        void Update(TEntity entity);

        /// <summary>
        /// Soft delete: sets Deleted = Id and audit fields. Does not remove the row.
        /// </summary>
        void SoftDelete(TEntity entity, int? deletorId = null);

        IQueryable<TEntity> Query();
    }
}
