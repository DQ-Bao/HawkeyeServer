using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HawkeyeServer.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HawkeyeServer.Api.Services;

public class JwtOptions
{
    public string Key { get; set; } = String.Empty;
}

public class JwtService(IOptions<JwtOptions> options)
{
    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(
            new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddYears(1),
                SigningCredentials = new SigningCredentials(
                    key: new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Key)),
                    algorithm: SecurityAlgorithms.HmacSha256Signature
                ),
            }
        );
        return handler.WriteToken(token);
    }
}
