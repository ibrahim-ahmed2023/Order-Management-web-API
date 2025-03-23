using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Entities;
using Services.JWT;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTO;

namespace OrderManagement.WebAPI.Middleware
{
    /// <summary>
    /// Middleware for handling JWT refresh token mechanism.
    /// </summary>
    public class JwtRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Initializes a new instance of JwtRefreshMiddleware.
        /// </summary>
        public JwtRefreshMiddleware(RequestDelegate next, IConfiguration configuration, IJwtService jwtService, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _configuration = configuration;
            _jwtService = jwtService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Middleware logic to validate token and refresh if expired.
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            var authInHeader = context.Request.Headers.Authorization.FirstOrDefault();

            // Ensure Authorization header is present and properly formatted
            if (string.IsNullOrEmpty(authInHeader) || !authInHeader.StartsWith("Bearer "))
            {
                await _next(context);
                return;
            }

            var token = authInHeader["Bearer ".Length..].Trim();
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Consistent JWT configuration key casing
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                // Validate the provided JWT token
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            }
            catch (SecurityTokenExpiredException)
            {
                // Refresh the token if expired
                await RefreshTokenAsync(context);
                return;
            }
            catch (SecurityTokenValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token validation failed.");
                return;
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Internal server error: {ex.Message}");
                return;
            }

            await _next(context);
        }

        /// <summary>
        /// Refreshes the access token using the refresh token.
        /// </summary>
        private async Task RefreshTokenAsync(HttpContext context)
        {
            var refreshToken = context.Request.Headers["RefreshToken"].FirstOrDefault();
            if (string.IsNullOrEmpty(refreshToken))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Refresh token is required.");
                return;
            }

            // Create scope for retrieving the database context
            using var scope = _serviceScopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Remove AsNoTracking() to allow updates to user entity
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null || user.RefreshTokenExpirationDateTime < DateTime.UtcNow)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired refresh token.");
                return;
            }

            // Generate new JWT and refresh token
            var authResponse = _jwtService.CreateJwtToken(user);

            // Update refresh token in the database
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = authResponse.RefreshTokenExpirationDateTime;
            await _context.SaveChangesAsync();

            // Respond with the new tokens
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(authResponse);
        }
    }

    /// <summary>
    /// Extension method to add JwtRefreshMiddleware.
    /// </summary>
    public static class JwtRefreshMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtRefreshMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtRefreshMiddleware>();
        }
    }
}