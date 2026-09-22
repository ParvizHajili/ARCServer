using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Auth;
using ARCServer.Business.Services.Permissions;
using ARCServer.Business.Settings;
using ARCServer.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ARCServer.Business.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserPermissionService _permissionService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IUserPermissionService permissionService,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _permissionService = permissionService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<ServiceResult<LoginResponseDto>> LoginAsync(
            LoginRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.UserNameOrEmail)
                || string.IsNullOrWhiteSpace(request.Password))
            {
                return ServiceResult<LoginResponseDto>.Failure(
                    "credentials",
                    ErrorMessages.Auth.InvalidCredentials);
            }

            var user = await FindUserAsync(request.UserNameOrEmail.Trim());
            if (user is null || !user.IsActive)
            {
                return ServiceResult<LoginResponseDto>.Failure(
                    "credentials",
                    ErrorMessages.Auth.InvalidCredentials);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                return ServiceResult<LoginResponseDto>.Failure(
                    "credentials",
                    ErrorMessages.Auth.InvalidCredentials);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _permissionService.GetEffectivePermissionsAsync(
                user.Id,
                roles,
                cancellationToken);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
            var token = CreateToken(user, roles, expiresAt);

            return ServiceResult<LoginResponseDto>.Success(new LoginResponseDto
            {
                AccessToken = token,
                ExpiresAtUtc = expiresAt,
                User = MapUser(user, roles, permissions),
            });
        }

        public async Task<ServiceResult<AuthUserDto>> GetCurrentUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null || !user.IsActive)
            {
                return ServiceResult<AuthUserDto>.NotFound(
                    "user",
                    ErrorMessages.Auth.UserNotFound);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _permissionService.GetEffectivePermissionsAsync(
                user.Id,
                roles,
                cancellationToken);

            return ServiceResult<AuthUserDto>.Success(MapUser(user, roles, permissions));
        }

        private async Task<ApplicationUser?> FindUserAsync(string userNameOrEmail)
        {
            if (userNameOrEmail.Contains('@'))
            {
                return await _userManager.FindByEmailAsync(userNameOrEmail)
                    ?? await _userManager.FindByNameAsync(userNameOrEmail);
            }

            return await _userManager.FindByNameAsync(userNameOrEmail)
                ?? await _userManager.FindByEmailAsync(userNameOrEmail);
        }

        private string CreateToken(
            ApplicationUser user,
            IList<string> roles,
            DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey) || _jwtSettings.SecretKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "JwtSettings:SecretKey must be configured and at least 32 characters.");
            }

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(AuthClaimTypes.FirstName, user.FirstName),
                new(AuthClaimTypes.LastName, user.LastName),
                new(AuthClaimTypes.DisplayName, user.DisplayName),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static AuthUserDto MapUser(
            ApplicationUser user,
            IList<string> roles,
            IReadOnlyList<string> permissions)
        {
            return new AuthUserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                Roles = roles.ToList(),
                Permissions = permissions,
            };
        }
    }
}
