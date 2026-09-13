using ARCServer.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [ApiController]
    public abstract class DashboardControllerBase : ApiControllerBase
    {
    }
}
