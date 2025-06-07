using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MyWebApiAuthApp.Services;

public interface IJwtService
{
    /// <summary>
    /// Generates JwtToken based on credentials provided.
    /// </summary>
    string Create(string userName, List<string> roles);

    /// <summary>
    /// Validates and decodes jwt token provided (for testing purposes).
    /// </summary>
    object Decode(string token);
}

public class JwtService : IJwtService
{
    private readonly string SecretKey;
    private readonly string Issuer;
    private readonly string Audience;
    private readonly SymmetricSecurityKey SecurityKey;
    private readonly SigningCredentials SigningCredential;
    private readonly JwtSecurityTokenHandler TokenHandler;

    public JwtService(IConfiguration configuration)
    {
        var jwtSettingsSection = configuration.GetSection("JwtSettings");
        SecretKey = jwtSettingsSection["SecretKey"];
        Issuer = jwtSettingsSection["Issuer"];
        Audience = jwtSettingsSection["Audience"];
        
        SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        SigningCredential = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
        TokenHandler = new JwtSecurityTokenHandler();
    }

    public string Create(string userName, List<string> roles)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  // Id of the issued token (can be used for revocation)
            new Claim(JwtRegisteredClaimNames.Sub, userName),   // Token subject (usually unique user identifier)
        };

        foreach (var role in roles)
        {
            claims.Append(new Claim("role", role));
        }

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(1),
            signingCredentials: SigningCredential
        );
        
        return TokenHandler.WriteToken(token);
    }

    public object Decode(string token)
    {
        var validationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Issuer,
            ValidAudience = Audience,
            IssuerSigningKey = SecurityKey,
        };
        try
        {
            var res = TokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            
            return new 
            { 
                Message = "Success",
                TokenId = validatedToken.Id,
                Issuer = validatedToken.Issuer, 
                SecurityKey = validatedToken.SecurityKey,
                SigningKey = validatedToken.SigningKey,
                ValidFrom = validatedToken.ValidFrom,
                ValidTo = validatedToken.ValidTo,
                Claims = res.Claims.Select(x => new{ Type = x.Type, Value = x.Value })
            };
        }
        catch (Exception e)
        {
            return new { Message = "Failed" };
        }
        
    }
}