using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Auth;

namespace ARCServer.Business.Services.Auth
{
    public interface IAuthService
    {
        Task<ServiceResult<LoginResponseDto>> LoginAsync(
            LoginRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<AuthUserDto>> GetCurrentUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
