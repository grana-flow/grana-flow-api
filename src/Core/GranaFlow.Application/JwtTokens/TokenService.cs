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

    public static string GenerateRefreshToken(IConfiguration configs, User user)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configs["JwtSettings:RefreshToken:SecretKey"]!));
        var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var refreshTokenHandler = new JwtSecurityTokenHandler();
        var refreshToken = new JwtSecurityToken(
            issuer: configs["JwtSettings:Issuer"],
            audience: configs["JwtSettings:Audience"],
            claims: new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("token_type", configs["JwtSettings:RefreshToken:Name"]!)
            },
            expires: DateTime.UtcNow.AddDays(int.Parse(configs["JwtSettings:RefreshToken:TokenExpirationInDays"]!)),
            signingCredentials: creds);
        return refreshTokenHandler.WriteToken(refreshToken);
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
