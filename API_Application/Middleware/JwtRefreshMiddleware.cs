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

namespace OrderManagement.WebAPI.Middleware
{
    public class JwtRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public JwtRefreshMiddleware(RequestDelegate next, IConfiguration configuration, IJwtService jwtService, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _configuration = configuration;
            _jwtService = jwtService;
            _serviceScopeFactory = serviceScopeFactory;
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
                    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = _configuration["JWT:Issuer"],  // Corrected from Issure to Issuer
                        ValidAudience = _configuration["JWT:Audience"],
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };

                    tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                }
                catch (SecurityTokenExpiredException)
                {
                    await RefreshTokenAsync(context);
                    return;
                }
                catch (Exception)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid or malformed token.");
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

            // Use IServiceScopeFactory to create a scope for resolving ApplicationDbContext
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
                if (user == null || user.RefreshTokenExpirationDateTime < DateTime.UtcNow)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired refresh token.");
                    return;
                }

                var authResponse = _jwtService.CreateJwtToken(user);

                user.RefreshToken = authResponse.RefreshToken;
                user.RefreshTokenExpirationDateTime = authResponse.RefreshTokenExpirationDateTime;
                await _context.SaveChangesAsync();

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(authResponse);
            }
        }
    }

    public static class JwtRefreshMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtRefreshMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JwtRefreshMiddleware>();
        }
    }
}
