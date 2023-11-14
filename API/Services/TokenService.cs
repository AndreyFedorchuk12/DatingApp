using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _securityKey;

    public TokenService(IConfiguration configuration)
    {
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenKey"] ??
                                                                       throw new InvalidOperationException(
                                                                           "Cannot generate JWT")));
    }

    public string CreateToken(AppUser user)
    {
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.NameId, user.UserName)
        };

        var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddHours(2),
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}