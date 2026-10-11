using ARCServer.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    /// <summary>
    /// Shared base for dashboard APIs. Authorization is applied by
    /// <see cref="Authorization.DashboardAuthorizeConvention"/> for the Dashboard area.
    /// </summary>
    [Area("Dashboard")]
    [ApiController]
    public abstract class DashboardControllerBase : ApiControllerBase
    {
    }
}
