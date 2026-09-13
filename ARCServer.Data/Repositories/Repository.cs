using System.Linq.Expressions;
using ARCServer.Data.Context;
using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Data.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly ArcDbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public Repository(ArcDbContext context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity.CreateDate == default)
            {
                entity.CreateDate = DateTime.UtcNow;
            }

            entity.Deleted = 0;
            await DbSet.AddAsync(entity, cancellationToken);
        }

        public virtual void Update(TEntity entity)
        {
            entity.UpdatedDate = DateTime.UtcNow;
            DbSet.Update(entity);
        }

        public virtual void SoftDelete(TEntity entity, int? deletorId = null)
        {
            entity.Deleted = entity.Id;
            entity.DeletedDate = DateTime.UtcNow;
            entity.DeletorId = deletorId;
            DbSet.Update(entity);
        }

        public virtual IQueryable<TEntity> Query()
        {
            return DbSet.AsQueryable();
        }
    }
}
