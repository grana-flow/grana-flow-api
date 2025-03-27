using Swashbuckle.AspNetCore.Filters;
using System.Text.Json.Serialization;

namespace GranaFlow.Infra.Ioc.Configs.Swagger.ExampleResponse;

public class SignInResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("expiration")]
    public DateTime Expiration { get; set; } = DateTime.Now;
}

public class SignInResponseExample : IExamplesProvider<SignInResponse>
{
    public SignInResponse GetExamples()
    {
        return new SignInResponse
        {
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
            Expiration = DateTime.Now,
        };
    }
}
