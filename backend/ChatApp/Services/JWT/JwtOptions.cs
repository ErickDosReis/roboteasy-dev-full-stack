namespace ChatApp.Services.JWT
{
    public sealed class JwtOptions
    {
        public const string SectionName = "JWT";
        public required string ValidIssuer { get; init; }
        public required string ValidAudience { get; init; }
        public required string SecretKey { get; init; }
        public int TokenValidityInMinutes { get; init; } = 30;
    }

}
