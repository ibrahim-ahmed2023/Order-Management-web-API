using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Entities;
using Services.JWT;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.WebAPI.Middleware
{
    public class JwtRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _context;

        public JwtRefreshMiddleware(RequestDelegate next, IConfiguration configuration, IJwtService jwtService, ApplicationDbContext context)
        {
            _next = next;
            _configuration = configuration;
            _jwtService = jwtService;
            _context = context;
        }

        public async Task Invoke(HttpContext context)
        {
            var AuthInHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (AuthInHeader != null && AuthInHeader.StartsWith("Bearer "))
            {
                var token = AuthInHeader.Substring("Bearer ".Length).Trim();
                var tokenHandler = new JwtSecurityTokenHandler();

                try
                {
                    var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = _configuration["JWT:Issure"],
                        ValidAudience = _configuration["JWT:Audience"],
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };

                    tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                }
                catch (SecurityTokenExpiredException)
                {
                    // Token is expired, try refreshing it
                    await RefreshTokenAsync(context);
                    return;
                }
            }

            await _next(context);
        }

        private async Task RefreshTokenAsync(HttpContext context)
        {
            var refreshToken = context.Request.Headers["RefreshToken"].FirstOrDefault();
            if (string.IsNullOrEmpty(refreshToken))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Refresh token is required.");
                return;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null || user.RefreshTokenExpirationDateTime < DateTime.UtcNow)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired refresh token.");
                return;
            }

            // Generate new access and refresh tokens
            var authResponse = _jwtService.CreateJwtToken(user);

            // Save new refresh token to the database
            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = authResponse.RefreshTokenExpirationDateTime;
            await _context.SaveChangesAsync();

            // Return the new tokens in the response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(authResponse);
        }
    }

    // Extension method for WebApplication
    public static class JwtRefreshMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtRefreshMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtRefreshMiddleware>();
        }
    }
}
