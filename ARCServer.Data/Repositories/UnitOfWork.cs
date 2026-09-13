using ARCServer.Data.Context;

namespace ARCServer.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ArcDbContext _context;

        public UnitOfWork(ArcDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
