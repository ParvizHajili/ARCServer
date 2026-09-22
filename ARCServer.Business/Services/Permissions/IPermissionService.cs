using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Permissions;

namespace ARCServer.Business.Services.Permissions
{
    public interface IPermissionService
    {
        Task<ServiceResult<IReadOnlyList<PermissionModuleDto>>> GetGroupedAsync(
            CancellationToken cancellationToken = default);
    }
}
