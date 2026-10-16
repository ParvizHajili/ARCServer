using ARCServer.Authorization;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.Contact;
using ARCServer.Business.Services.Contact;
using ARCServer.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    [Route("api/dashboard/inquiries")]
    public class ContactInquiriesController : DashboardControllerBase
    {
        private readonly IContactInquiryService _service;

        public ContactInquiriesController(IContactInquiryService service)
        {
            _service = service;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.Inquiries.List)]
        [ProducesResponseType(typeof(PaginationResponseDto<ContactInquiryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return FromResult(result);
        }
    }
}
