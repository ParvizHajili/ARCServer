using Microsoft.AspNetCore.Identity;

namespace ARCServer.Business.Identity
{
    /// <summary>
    /// ASP.NET Identity xəta mesajları — Azərbaycan dili.
    /// </summary>
    public class AzIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() =>
            new()
            {
                Code = nameof(DefaultError),
                Description = "Naməlum xəta baş verdi.",
            };

        public override IdentityError ConcurrencyFailure() =>
            new()
            {
                Code = nameof(ConcurrencyFailure),
                Description = "Optimistik eyni vaxtda dəyişiklik uğursuz oldu. Yenidən cəhd edin.",
            };

        public override IdentityError PasswordMismatch() =>
            new()
            {
                Code = nameof(PasswordMismatch),
                Description = "Parol yanlışdır.",
            };

        public override IdentityError InvalidToken() =>
            new()
            {
                Code = nameof(InvalidToken),
                Description = "Token etibarsızdır.",
            };

        public override IdentityError LoginAlreadyAssociated() =>
            new()
            {
                Code = nameof(LoginAlreadyAssociated),
                Description = "Bu login artıq başqa istifadəçiyə bağlıdır.",
            };

        public override IdentityError InvalidUserName(string? userName) =>
            new()
            {
                Code = nameof(InvalidUserName),
                Description = $"'{userName}' istifadəçi adı etibarsızdır.",
            };

        public override IdentityError InvalidEmail(string? email) =>
            new()
            {
                Code = nameof(InvalidEmail),
                Description = $"'{email}' e-poçt ünvanı etibarsızdır.",
            };

        public override IdentityError DuplicateUserName(string userName) =>
            new()
            {
                Code = nameof(DuplicateUserName),
                Description = $"'{userName}' istifadəçi adı artıq mövcuddur.",
            };

        public override IdentityError DuplicateEmail(string email) =>
            new()
            {
                Code = nameof(DuplicateEmail),
                Description = $"'{email}' e-poçt artıq mövcuddur.",
            };

        public override IdentityError InvalidRoleName(string? role) =>
            new()
            {
                Code = nameof(InvalidRoleName),
                Description = $"'{role}' rol adı etibarsızdır.",
            };

        public override IdentityError DuplicateRoleName(string role) =>
            new()
            {
                Code = nameof(DuplicateRoleName),
                Description = $"'{role}' rol adı artıq mövcuddur.",
            };

        public override IdentityError UserAlreadyHasPassword() =>
            new()
            {
                Code = nameof(UserAlreadyHasPassword),
                Description = "İstifadəçinin artıq parolu var.",
            };

        public override IdentityError UserLockoutNotEnabled() =>
            new()
            {
                Code = nameof(UserLockoutNotEnabled),
                Description = "Bu istifadəçi üçün kilidləmə aktiv deyil.",
            };

        public override IdentityError UserAlreadyInRole(string role) =>
            new()
            {
                Code = nameof(UserAlreadyInRole),
                Description = $"İstifadəçi artıq '{role}' rolundadır.",
            };

        public override IdentityError UserNotInRole(string role) =>
            new()
            {
                Code = nameof(UserNotInRole),
                Description = $"İstifadəçi '{role}' rolunda deyil.",
            };

        public override IdentityError PasswordTooShort(int length) =>
            new()
            {
                Code = nameof(PasswordTooShort),
                Description = $"Parol ən azı {length} simvol olmalıdır.",
            };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
            new()
            {
                Code = nameof(PasswordRequiresUniqueChars),
                Description = $"Parolda ən azı {uniqueChars} fərqli simvol olmalıdır.",
            };

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            new()
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "Parolda ən azı bir xüsusi simvol (!, ?, # və s.) olmalıdır.",
            };

        public override IdentityError PasswordRequiresDigit() =>
            new()
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "Parolda ən azı bir rəqəm (0-9) olmalıdır.",
            };

        public override IdentityError PasswordRequiresLower() =>
            new()
            {
                Code = nameof(PasswordRequiresLower),
                Description = "Parolda ən azı bir kiçik hərf (a-z) olmalıdır.",
            };

        public override IdentityError PasswordRequiresUpper() =>
            new()
            {
                Code = nameof(PasswordRequiresUpper),
                Description = "Parolda ən azı bir böyük hərf (A-Z) olmalıdır.",
            };
    }
}
