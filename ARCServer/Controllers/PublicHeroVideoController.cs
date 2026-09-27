using ARCServer.Business.Services.HeroVideos;
using ARCServer.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/hero-video")]
    public class PublicHeroVideoController : ApiControllerBase
    {
        private readonly IHeroVideoService _service;

        public PublicHeroVideoController(IHeroVideoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _service.GetAsync(cancellationToken);
            return FromResult(result);
        }
    }
}
