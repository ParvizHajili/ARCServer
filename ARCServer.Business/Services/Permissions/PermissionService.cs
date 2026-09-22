using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Permissions;
using ARCServer.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly ArcDbContext _db;

        public PermissionService(ArcDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceResult<IReadOnlyList<PermissionModuleDto>>> GetGroupedAsync(
            CancellationToken cancellationToken = default)
        {
            var items = await _db.Permissions
                .AsNoTracking()
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Action)
                .Select(p => new PermissionItemDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Module = p.Module,
                    Action = p.Action,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                })
                .ToListAsync(cancellationToken);

            var grouped = items
                .GroupBy(p => p.Module)
                .Select(g => new PermissionModuleDto
                {
                    Module = g.Key,
                    Permissions = g.ToList(),
                })
                .ToList();

            return ServiceResult<IReadOnlyList<PermissionModuleDto>>.Success(grouped);
        }
    }
}
