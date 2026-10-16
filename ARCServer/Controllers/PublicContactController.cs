using ARCServer.Business.Dtos.Contact;
using ARCServer.Business.Services.Contact;
using ARCServer.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class PublicContactController : ApiControllerBase
    {
        private readonly IContactInquiryService _service;

        public PublicContactController(IContactInquiryService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ContactInquiryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateContactInquiryDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(dto, cancellationToken);
            return FromResult(result);
        }
    }
}
