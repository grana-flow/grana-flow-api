using Microsoft.IdentityModel.Tokens;

namespace PlanWise.Application.JWT.Contract;

internal sealed class Credentials(
    string issuer,
    string audience,
    SymmetricSecurityKey secretKey,
    SigningCredentials creds,
    DateTime expirationInMinutes)
{
    public string Issuer { get; private set; } = issuer;
    public string Audience { get; private set; } = audience;
    public SymmetricSecurityKey SecretKey { get; private set; } = secretKey;
    public SigningCredentials Creds { get; private set; } = creds;
    public DateTime ExpirationInMinutes { get; private set; } = expirationInMinutes;

    public static Credentials CreateCredentials(string issuer,
        string audience,
        SymmetricSecurityKey secretKey,
        SigningCredentials creds,
        DateTime expirationInMinutes)
    {
        return new Credentials(
            issuer, audience, secretKey, creds, expirationInMinutes);
    }
}
