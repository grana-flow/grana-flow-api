namespace GranaFlow.Application.JwtTokens;

internal class Credentials(
    string securityKey,
    DateTime expirationInMinutes,
    string issuer,
    string audience)
{
    public string SecurityKey { get; private set; } = securityKey;
    public string Issuer { get; private set;} = issuer;
    public string Audience { get; private set; } = audience;
    public DateTime ExpirationInMinutes { get; private set; } = expirationInMinutes;
}
