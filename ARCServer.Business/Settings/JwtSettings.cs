namespace ARCServer.Business.Settings
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string Issuer { get; set; } = "ARCServer";

        public string Audience { get; set; } = "ARCClient";

        public string SecretKey { get; set; } = string.Empty;

        public int ExpirationMinutes { get; set; } = 480;
    }
}
