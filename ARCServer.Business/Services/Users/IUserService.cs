using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.Users;

namespace ARCServer.Business.Services.Users
{
    public interface IUserService
    {
        Task<ServiceResult<PaginationResponseDto<UserListItemDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<UserDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<UserDetailDto>> CreateAsync(
            CreateUserDto dto,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<UserDetailDto>> UpdateAsync(
            int id,
            UpdateUserDto dto,
            int? actorUserId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? actorUserId = null,
            CancellationToken cancellationToken = default);
    }
}
