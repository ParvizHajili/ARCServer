namespace ARCServer.Business.Dtos.Auth
{
    public class LoginResponseDto
    {
        public required string AccessToken { get; init; }

        public required DateTime ExpiresAtUtc { get; init; }

        public required AuthUserDto User { get; init; }
    }

    public class AuthUserDto
    {
        public required int Id { get; init; }

        public required string UserName { get; init; }

        public required string Email { get; init; }

        public required string FirstName { get; init; }

        public required string LastName { get; init; }

        public required string DisplayName { get; init; }

        public required IReadOnlyList<string> Roles { get; init; }

        public required IReadOnlyList<string> Permissions { get; init; }
    }
}
