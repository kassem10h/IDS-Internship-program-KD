using Microsoft.IdentityModel.Tokens;
using Smart_Meeting.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Smart_Meeting.JWT
{
    public static class GenerateJWT
    {
        public static string GenerateJwtToken(Employee emp, IConfiguration config)
        {
            var secret = config["JWT:SecretKey"];
            if (string.IsNullOrEmpty(secret))
                throw new InvalidOperationException("JWT secret key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var claims = new List<Claim>
            {
                new Claim("employee_id", emp.EmployeeId.ToString()),
                new Claim(ClaimTypes.Role, emp.Role ?? "User"),
                new Claim(ClaimTypes.NameIdentifier, emp.EmployeeId.ToString())
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                    int.TryParse(config["JWT:AccessTokenExpirationMinutes"], out var m) ? m : 60),
                Issuer = config["JWT:Issuer"],
                Audience = config["JWT:Audience"],
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
