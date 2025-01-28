using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoneySaver.Identity.Data.Models;
using MoneySaver.System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoneySaver.Identity.Services.Identity
{
    public class TokenGeneratorService : ITokenGeneratorService
    {
        private readonly ApplicationSettings applicationSettings;

        public TokenGeneratorService(IOptions<ApplicationSettings> applicationSettings)
             => this.applicationSettings = applicationSettings.Value;

        public string GenerateToken(User user, IEnumerable<string> roles = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.applicationSettings.Secret);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                //TODO: Add the expire parameter in appsettings
                Expires = DateTime.UtcNow.AddMinutes(60),
                //Issuer = "issuer of the token",
                //Audience = "where it will be used"
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedToken = tokenHandler.WriteToken(token);

            return encryptedToken;

        }
    }
}
