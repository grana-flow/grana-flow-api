using GranaFlow.Domain.Contracts;
using Swashbuckle.AspNetCore.Filters;

namespace GranaFlow.Infra.Ioc.Configs.Swagger.ExampleResponse;

public class SignInResponseExample : IExamplesProvider<AuthTokenResponse>
{
    public AuthTokenResponse GetExamples()
    {
        return new AuthTokenResponse(
            accessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
            expiration: DateTime.Now,
            refreshToken: "Bu0Ir5vDbeSKLAhj6jwQXThY="
        );
    }
}
