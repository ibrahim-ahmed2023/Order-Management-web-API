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
    /// Service class responsible for handling JSON Web Token (JWT) generation and validation.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object containing JWT settings such as key, issuer, audience, and expiration times.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Creates a JWT token and a refresh token for the specified user.
        /// </summary>
        /// <param name="user">The application user for whom the token is generated.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> object containing the JWT token, refresh token, and related user information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the JWT key is missing from the configuration.</exception>
        public AuthenticationResponse CreateJwtToken(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(ClaimTypes.Name, user.PersonName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
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
        /// Generates a cryptographically secure random refresh token.
        /// </summary>
        /// <returns>A Base64-encoded string representing the refresh token.</returns>
        private string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Validates a JWT token and retrieves the associated claims principal.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>A <see cref="ClaimsPrincipal"/> object if the token is valid; otherwise, <c>null</c>.</returns>
        /// <remarks>
        /// Returns <c>null</c> if the token is null, empty, or fails validation due to expiration, issuer mismatch, audience mismatch, or signature issues.
        /// </remarks>
        public ClaimsPrincipal? GetPrincipalFromJwtToken(string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var key = _configuration["Jwt:Key"] ?? string.Empty;
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (SecurityTokenException ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return null;
            }
        }
    }
}