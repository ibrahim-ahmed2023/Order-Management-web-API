using System.Security.Claims;
using ServiceContracts.DTO;
using Entities.Identity;

namespace Services.JWT
{
    public interface IJwtService
    {
        AuthenticationResponse CreateJwtToken(ApplicationUser user);
        ClaimsPrincipal? GetPrincipalFromJwtToken(string? token);
    }
}
