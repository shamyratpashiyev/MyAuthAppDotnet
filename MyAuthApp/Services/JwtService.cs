using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MyAuthApp.Services;

public class JwtService
{
    private const string SecretKey = "988eb86199ea4588b7c4a70ba0633ce5";
    private const string Issuer = "MyAuthApp";
    private const string Audience = "MyAuthAppAudience";
    private readonly SymmetricSecurityKey SecurityKey;
    private readonly SigningCredentials SigningCredential;
    private readonly JwtSecurityTokenHandler TokenHandler;

    public JwtService()
    {
        SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        SigningCredential = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
        TokenHandler = new JwtSecurityTokenHandler();
    }

    public string Create()
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "user123"),
            new Claim("role", "admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(1),
            signingCredentials: SigningCredential
        );
        
        return TokenHandler.WriteToken(token);
    }

    public void Decode(string token)
    {
        Console.WriteLine($"JwtAccessToken: {token}");
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
            foreach (var claim in res.Claims)
            {
                Console.WriteLine($"claim: type: {claim.Type}, value: {claim.Value}");
            }
            Console.WriteLine();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");  
        }
        
    }
}