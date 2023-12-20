using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SymmetricSecurityKey _securityKey;

    public TokenService(IConfiguration configuration, UserManager<AppUser> userManager)
    {
        _userManager = userManager;
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenKey"] ??
                                                                       throw new InvalidOperationException(
                                                                           "Cannot generate JWT")));
    }

    public async Task<string> CreateToken(AppUser? user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user?.Id.ToString() ?? throw new InvalidOperationException()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? throw new InvalidOperationException())
        };

        var roles = await _userManager.GetRolesAsync(user);
        
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
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