using System.Text.Json.Serialization;

namespace GranaFlow.Domain.Contracts;

public class AuthTokenResponse(
    string accessToken,
    string refreshToken,
    DateTime expiration)
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; private set; } = accessToken;
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; private set;} = refreshToken;
    [JsonPropertyName("expiration")]
    public DateTime Expiration { get; private set;} = expiration;
}
