using System.ComponentModel.DataAnnotations;

namespace PlanWise.Application.JWT.Contract;

internal sealed class RequestToken(
    string accessToken,
    string expiration)
{
    [Display(Name = "access_token")]
    public string AccessToken { get; private set; } = accessToken;
    public string Expiration { get; private set; } = expiration;

    public static RequestToken GenerateToken(
        string accessToken,
        string expiration)
    {
        return new RequestToken(accessToken, expiration);
    }
}
