using GranaFlow.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GranaFlow.Application.JwtTokens;

public static class TokenService
{
    public static string GenerateAccessToken(IConfiguration configs, User user)
    {
        var credentials = configs.GetCredentials();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(credentials.SecurityKey));
        var signinCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = SetClaims(user);

        var token = new JwtSecurityToken(
            issuer: credentials.Issuer,
            audience: credentials.Audience,
            claims: claims,
            expires: credentials.ExpirationInMinutes,
            signingCredentials: signinCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateRefreshToken(IConfiguration configs, string accessToken)
    {
        var secretKey = configs["JwtSettings:RefreshToken:SecretKey"];

        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(accessToken)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(accessToken));
            return Convert.ToBase64String(hash);
        }
    }

    private static Claim[] SetClaims(User user)
    {
        return new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, user.Id),
            //new Claim(JwtRegisteredClaimNames.Name, user.UserName!), inserir nome do usuario
            new Claim("twoFactorEnable", user.TwoFactorEnabled.ToString())
        };
    }

    private static Credentials GetCredentials(this IConfiguration configs)
    {
        var secretKey = configs["JwtSettings:SecretKey"];
        var issuer = configs["JwtSettings:Issuer"];
        var audience = configs["JwtSettings:Audience"];
        var expirationInMinutes = configs["JwtSettings:TokenExpirationMinutes"];

        return new Credentials(secretKey!, DateTime.UtcNow.AddMinutes(int.Parse(expirationInMinutes!)), issuer!, audience!);
    }
}
