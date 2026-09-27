using ARCServer.Authorization;
using ARCServer.Business.Services.HeroVideos;
using ARCServer.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    [Route("api/dashboard/hero-video")]
    public class HeroVideosController : DashboardControllerBase
    {
        private readonly IHeroVideoService _service;

        public HeroVideosController(IHeroVideoService service)
        {
            _service = service;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.HeroVideo.List)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _service.GetAsync(cancellationToken);
            return FromResult(result);
        }

        [HttpPut]
        [RequirePermission(PermissionCodes.HeroVideo.Update)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Save(
            IFormFile video,
            CancellationToken cancellationToken)
        {
            var result = await _service.SaveAsync(video, userId: null, cancellationToken);
            return FromResult(result);
        }
    }
}
