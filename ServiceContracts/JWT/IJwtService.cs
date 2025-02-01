using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Entities.Identity;
using ServiceContracts.DTO;

namespace Services.JWT
{
    public interface IJwtService
    {
        AuthenticationResponse CreateJwtToken(ApplicationUser user);
        ClaimsPrincipal? GetPrincipalFromJwtToken(string? token);
    }
}