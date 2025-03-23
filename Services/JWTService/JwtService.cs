using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServiceContracts.DTO;
using Services.JWT;
using Entities.Identity;

namespace Services.JWTService
{
    /// <summary>
    /// Service responsible for generating and validating JWT tokens.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration containing JWT settings.</param>
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a new JWT token and refresh token for a given user.
        /// </summary>
        /// <param name="user">The application user.</param>
        /// <returns>An authentication response containing tokens and user information.</returns>
        public AuthenticationResponse CreateJwtToken(ApplicationUser user)
        {
            var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(ClaimTypes.Name, user.PersonName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenGenerator = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(tokenGenerator);

            return new AuthenticationResponse()
            {
                Token = token,
                Email = user.Email,
                PersonName = user.PersonName,
                Expiration = expiration,
                RefreshToken = GenerateRefreshToken(),
                RefreshTokenExpirationDateTime = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["RefreshToken:EXPIRATION_MINUTES"]))
            };
        }

        /// <summary>
        /// Generates a secure random refresh token.
        /// </summary>
        /// <returns>The generated refresh token.</returns>
        private string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Extracts the claims principal from a given JWT token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>The claims principal if the token is valid, otherwise null.</returns>
        public ClaimsPrincipal? GetPrincipalFromJwtToken(string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}