using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.Users;
using ARCServer.Data.Context;
using ARCServer.Domain.Common;
using ARCServer.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ArcDbContext _db;

        public UserService(UserManager<ApplicationUser> userManager, ArcDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<ServiceResult<PaginationResponseDto<UserListItemDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1
                ? PaginationRequestDto.DefaultPageSize
                : Math.Min(request.PageSize, PaginationRequestDto.MaxPageSize);

            var query = _userManager.Users.AsNoTracking();

            var search = request.NormalizedSearch;
            if (search is not null)
            {
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.Contains(search))
                    || (u.Email != null && u.Email.Contains(search))
                    || u.FirstName.Contains(search)
                    || u.LastName.Contains(search));
            }

            query = request.NormalizedSortBy switch
            {
                "email" => request.IsDescending
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                "fullname" => request.IsDescending
                    ? query.OrderByDescending(u => u.UserName)
                    : query.OrderBy(u => u.UserName),
                "active" => request.IsDescending
                    ? query.OrderByDescending(u => u.IsActive)
                    : query.OrderBy(u => u.IsActive),
                "created" => request.IsDescending
                    ? query.OrderByDescending(u => u.CreateDate)
                    : query.OrderBy(u => u.CreateDate),
                _ => request.IsDescending
                    ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                    : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = new List<UserListItemDto>(users.Count);
            foreach (var user in users)
            {
                items.Add(await MapListItemAsync(user, cancellationToken));
            }

            return ServiceResult<PaginationResponseDto<UserListItemDto>>.Success(
                PaginationResponseDto<UserListItemDto>.Create(items, page, pageSize, totalCount));
        }

        public async Task<ServiceResult<UserDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
            {
                return ServiceResult<UserDetailDto>.NotFound("id", ErrorMessages.User.NotFound);
            }

            return ServiceResult<UserDetailDto>.Success(await MapDetailAsync(user, cancellationToken));
        }

        public async Task<ServiceResult<UserDetailDto>> CreateAsync(
            CreateUserDto dto,
            CancellationToken cancellationToken = default)
        {
            var validation = ValidateCreate(dto);
            if (validation is not null)
            {
                return validation;
            }

            if (await _userManager.FindByNameAsync(dto.UserName.Trim()) is not null)
            {
                return ServiceResult<UserDetailDto>.Failure(
                    "userName",
                    ErrorMessages.Format(ErrorMessages.User.UserNameExists, dto.UserName.Trim()));
            }

            if (await _userManager.FindByEmailAsync(dto.Email.Trim()) is not null)
            {
                return ServiceResult<UserDetailDto>.Failure(
                    "email",
                    ErrorMessages.Format(ErrorMessages.User.EmailExists, dto.Email.Trim()));
            }

            var permissionResult = await ResolvePermissionIdsAsync(dto.PermissionCodes, cancellationToken);
            if (!permissionResult.Succeeded)
            {
                return ServiceResult<UserDetailDto>.Failure(
                    permissionResult.Errors,
                    permissionResult.ErrorType);
            }

            var user = new ApplicationUser
            {
                UserName = dto.UserName.Trim(),
                Email = dto.Email.Trim(),
                EmailConfirmed = true,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                IsActive = dto.IsActive,
                CreateDate = DateTime.UtcNow,
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                return ServiceResult<UserDetailDto>.Failure(
                    "password",
                    string.Join(" ", createResult.Errors.Select(e => e.Description)));
            }

            await ReplaceUserPermissionsAsync(user.Id, permissionResult.Data!, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var created = await _userManager.FindByIdAsync(user.Id.ToString());
            return ServiceResult<UserDetailDto>.Success(await MapDetailAsync(created!, cancellationToken));
        }

        public async Task<ServiceResult<UserDetailDto>> UpdateAsync(
            int id,
            UpdateUserDto dto,
            int? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            var validation = ValidateUpdate(dto);
            if (validation is not null)
            {
                return validation;
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
            {
                return ServiceResult<UserDetailDto>.NotFound("id", ErrorMessages.User.NotFound);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isSuperAdmin = roles.Contains(AppRoles.SuperAdmin, StringComparer.OrdinalIgnoreCase);

            if (isSuperAdmin && actorUserId != id)
            {
                // Allow SUPERADMIN self-edit of name/email/password, but block other admins
                // from stripping permissions of the seeded system admin via this endpoint.
                // Direct permission edits for SUPERADMIN are ignored (role grants all).
            }

            if (actorUserId == id && !dto.IsActive)
            {
                return ServiceResult<UserDetailDto>.Failure(
                    "isActive",
                    ErrorMessages.User.CannotDeactivateSelf);
            }

            var emailOwner = await _userManager.FindByEmailAsync(dto.Email.Trim());
            if (emailOwner is not null && emailOwner.Id != id)
            {
                return ServiceResult<UserDetailDto>.Failure(
                    "email",
                    ErrorMessages.Format(ErrorMessages.User.EmailExists, dto.Email.Trim()));
            }

            var permissionResult = await ResolvePermissionIdsAsync(dto.PermissionCodes, cancellationToken);
            if (!permissionResult.Succeeded)
            {
                return ServiceResult<UserDetailDto>.Failure(
                    permissionResult.Errors,
                    permissionResult.ErrorType);
            }

            user.Email = dto.Email.Trim();
            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.IsActive = dto.IsActive;
            user.UpdatedDate = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return ServiceResult<UserDetailDto>.Failure(
                    "user",
                    string.Join(" ", updateResult.Errors.Select(e => e.Description)));
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!passwordResult.Succeeded)
                {
                    return ServiceResult<UserDetailDto>.Failure(
                        "password",
                        string.Join(" ", passwordResult.Errors.Select(e => e.Description)));
                }
            }

            if (!isSuperAdmin)
            {
                await ReplaceUserPermissionsAsync(user.Id, permissionResult.Data!, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }

            var refreshed = await _userManager.FindByIdAsync(id.ToString());
            return ServiceResult<UserDetailDto>.Success(await MapDetailAsync(refreshed!, cancellationToken));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == id)
            {
                return ServiceResult<bool>.Failure("id", ErrorMessages.User.CannotDeleteSelf);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
            {
                return ServiceResult<bool>.NotFound("id", ErrorMessages.User.NotFound);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(AppRoles.SuperAdmin, StringComparer.OrdinalIgnoreCase))
            {
                return ServiceResult<bool>.Failure("id", ErrorMessages.User.CannotModifySystemAdmin);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return ServiceResult<bool>.Failure(
                    "user",
                    string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            return ServiceResult<bool>.Success(true);
        }

        private static ServiceResult<UserDetailDto>? ValidateCreate(CreateUserDto dto)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(dto.UserName))
                errors["userName"] = [ErrorMessages.Format(ErrorMessages.Common.Required, "İstifadəçi adı")];
            if (string.IsNullOrWhiteSpace(dto.Email))
                errors["email"] = [ErrorMessages.Format(ErrorMessages.Common.Required, "E-poçt")];
            if (string.IsNullOrWhiteSpace(dto.FirstName))
                errors["firstName"] = [ErrorMessages.User.FirstNameRequired];
            if (string.IsNullOrWhiteSpace(dto.LastName))
                errors["lastName"] = [ErrorMessages.User.LastNameRequired];
            if (string.IsNullOrWhiteSpace(dto.Password))
                errors["password"] = [ErrorMessages.User.PasswordRequired];

            return errors.Count == 0
                ? null
                : ServiceResult<UserDetailDto>.Failure(errors);
        }

        private static ServiceResult<UserDetailDto>? ValidateUpdate(UpdateUserDto dto)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(dto.Email))
                errors["email"] = [ErrorMessages.Format(ErrorMessages.Common.Required, "E-poçt")];
            if (string.IsNullOrWhiteSpace(dto.FirstName))
                errors["firstName"] = [ErrorMessages.User.FirstNameRequired];
            if (string.IsNullOrWhiteSpace(dto.LastName))
                errors["lastName"] = [ErrorMessages.User.LastNameRequired];

            return errors.Count == 0
                ? null
                : ServiceResult<UserDetailDto>.Failure(errors);
        }

        private async Task<ServiceResult<List<int>>> ResolvePermissionIdsAsync(
            IEnumerable<string>? codes,
            CancellationToken cancellationToken)
        {
            var normalized = (codes ?? [])
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalized.Count == 0)
            {
                return ServiceResult<List<int>>.Success([]);
            }

            var permissions = await _db.Permissions
                .AsNoTracking()
                .Where(p => normalized.Contains(p.Code))
                .Select(p => new { p.Id, p.Code })
                .ToListAsync(cancellationToken);

            var foundCodes = permissions.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missing = normalized.FirstOrDefault(c => !foundCodes.Contains(c));
            if (missing is not null)
            {
                return ServiceResult<List<int>>.Failure(
                    "permissionCodes",
                    ErrorMessages.Format(ErrorMessages.User.InvalidPermission, missing));
            }

            return ServiceResult<List<int>>.Success(permissions.Select(p => p.Id).ToList());
        }

        private async Task ReplaceUserPermissionsAsync(
            int userId,
            IReadOnlyList<int> permissionIds,
            CancellationToken cancellationToken)
        {
            var existing = await _db.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync(cancellationToken);

            _db.UserPermissions.RemoveRange(existing);

            foreach (var permissionId in permissionIds)
            {
                _db.UserPermissions.Add(new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionId,
                });
            }
        }

        private async Task<UserListItemDto> MapListItemAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissionCount = await CountEffectivePermissionsAsync(user.Id, roles, cancellationToken);

            return new UserListItemDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                IsActive = user.IsActive,
                Roles = roles.ToList(),
                PermissionCount = permissionCount,
                CreateDate = user.CreateDate,
            };
        }

        private async Task<UserDetailDto> MapDetailAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var direct = await _db.UserPermissions
                .AsNoTracking()
                .Where(up => up.UserId == user.Id)
                .Select(up => up.Permission.Code)
                .OrderBy(c => c)
                .ToListAsync(cancellationToken);

            var effective = await GetEffectivePermissionsAsync(user.Id, roles, cancellationToken);

            return new UserDetailDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                IsActive = user.IsActive,
                Roles = roles.ToList(),
                PermissionCodes = direct,
                EffectivePermissions = effective,
                CreateDate = user.CreateDate,
                UpdatedDate = user.UpdatedDate,
            };
        }

        private async Task<int> CountEffectivePermissionsAsync(
            int userId,
            IList<string> roles,
            CancellationToken cancellationToken)
        {
            if (roles.Contains(AppRoles.SuperAdmin, StringComparer.OrdinalIgnoreCase))
            {
                return await _db.Permissions.CountAsync(cancellationToken);
            }

            return (await GetEffectivePermissionsAsync(userId, roles, cancellationToken)).Count;
        }

        private async Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(
            int userId,
            IList<string> roles,
            CancellationToken cancellationToken)
        {
            if (roles.Contains(AppRoles.SuperAdmin, StringComparer.OrdinalIgnoreCase))
            {
                return await _db.Permissions
                    .AsNoTracking()
                    .Select(x => x.Code)
                    .OrderBy(x => x)
                    .ToListAsync(cancellationToken);
            }

            var rolePermissions = await _db.RolePermissions
                .AsNoTracking()
                .Where(rp => _db.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == rp.RoleId))
                .Select(rp => rp.Permission.Code)
                .ToListAsync(cancellationToken);

            var userPermissions = await _db.UserPermissions
                .AsNoTracking()
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission.Code)
                .ToListAsync(cancellationToken);

            return rolePermissions
                .Concat(userPermissions)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }
    }
}
