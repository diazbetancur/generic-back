using CC.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CC.Aplication.Utils
{
    /// <summary>
    /// Generador de tokens JWT para autenticaci�n
    /// </summary>
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Genera un token JWT para un usuario administrativo
        /// </summary>
        /// <param name="user">Usuario</param>
        /// <param name="roles">Roles del usuario</param>
        /// <returns>Token JWT</returns>
        public string GenerateAdminToken(User user, IList<string> roles)
        {
            var jwtSecret = _configuration["Authentication:JwtSecret"];
            if (string.IsNullOrWhiteSpace(jwtSecret))
            {
                throw new InvalidOperationException("JWT Secret no configurado en appsettings");
            }

            if (jwtSecret.Length < 32)
            {
                throw new InvalidOperationException("JWT Secret debe tener al menos 32 caracteres");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            // Agregar roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            int.TryParse(_configuration["Authentication:TokenLifetimeMinutes"], out int tokenLifetime);
            if (tokenLifetime == 0) tokenLifetime = 480; // Default 8 horas

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(tokenLifetime),
                SigningCredentials = credentials,
                Issuer = _configuration["Authentication:Issuer"],
                Audience = _configuration["Authentication:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Genera un token JWT para un paciente (OTP autenticado)
        /// </summary>
        /// <param name="user">Usuario paciente</param>
        /// <param name="history">N�mero de historia cl�nica</param>
        /// <returns>Token JWT</returns>
        public string GeneratePatientToken(User user, string? history = null)
        {
            var jwtSecret = _configuration["Authentication:JwtSecret"];
            if (string.IsNullOrWhiteSpace(jwtSecret))
            {
                throw new InvalidOperationException("JWT Secret no configurado en appsettings");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(history))
            {
                claims.Add(new Claim("History", history));
            }

            int.TryParse(_configuration["Authentication:TokenLifetimeMinutes"], out int tokenLifetime);
            if (tokenLifetime == 0) tokenLifetime = 30; // Default 30 min para pacientes

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(tokenLifetime),
                SigningCredentials = credentials,
                Issuer = _configuration["Authentication:Issuer"],
                Audience = _configuration["Authentication:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Obtiene el tiempo de vida del token en minutos
        /// </summary>
        /// <returns>Minutos de vigencia del token</returns>
        public int GetTokenLifetimeMinutes()
        {
            int.TryParse(_configuration["Authentication:TokenLifetimeMinutes"], out int tokenLifetime);
            return tokenLifetime == 0 ? 480 : tokenLifetime;
        }
    }
}
