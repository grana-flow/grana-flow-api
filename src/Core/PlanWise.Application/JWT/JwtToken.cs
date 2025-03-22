using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PlanWise.Application.JWT.Contract;
using PlanWise.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlanWise.Application.JWT;

internal static class JwtToken
{
    public static RequestToken GenerateToken(this IConfiguration config, User user)
    {
        var credentials = GetCredentials(config);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // new Claim("scope", user.UserName!)   // permissao de escopo do usuario
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            // new Claim(JwtRegisteredClaimNames.Name, user.UserName!)  // nome do usuario
            new Claim("2FAAuthEnable", user.TwoFactorEnabled!.ToString()),
        };

        var token = new JwtSecurityToken(
            credentials.Issuer,
            credentials.Audience,
            claims,
            expires: credentials.ExpirationInMinutes,
            signingCredentials: credentials.Creds);

        return RequestToken.GenerateToken(new JwtSecurityTokenHandler().WriteToken(token), credentials.ExpirationInMinutes.ToString());
    }

    private static Credentials GetCredentials(IConfiguration config)
    {
        var issuer = config["JwtSettings:Issuer"]!;
        var audience = config["JwtSettings:Audience"]!;
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!));
        var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var expirationMinutes = DateTime.UtcNow.AddMinutes(int.Parse(config["JwtSettings:TokenExpirationMinutes"]!));

        return Credentials.CreateCredentials(issuer, audience, secretKey, creds, expirationMinutes);
    }
}
