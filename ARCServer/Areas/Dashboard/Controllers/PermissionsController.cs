using ARCServer.Authorization;
using ARCServer.Business.Dtos.Permissions;
using ARCServer.Business.Services.Permissions;
using ARCServer.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    [Route("api/dashboard/permissions")]
    public class PermissionsController : DashboardControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.Permissions.List)]
        [ProducesResponseType(typeof(IReadOnlyList<PermissionModuleDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _permissionService.GetGroupedAsync(cancellationToken);
            return FromResult(result);
        }
    }
}
