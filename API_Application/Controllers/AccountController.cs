using Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceContracts.DTO;
using Services.JWT;

namespace OrderManagement.WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for user account management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtService jwtService, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="registerDTO">User registration data.</param>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult> PostRegister(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(GetModelErrors());
            }

            var user = new ApplicationUser
            {
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber,
                UserName = registerDTO.Email,
                PersonName = registerDTO.PersonName
            };

            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(GetIdentityErrors(result));
            }

            await SignInUser(user);
            var authenticationResponse = await GenerateJwtToken(user);
            _logger.LogInformation($"User {user.Email} registered successfully.");
            return Ok(authenticationResponse);
        }

        /// <summary>
        /// Checks if an email is already registered.
        /// </summary>
        /// <param name="email">Email to check.</param>
        [HttpGet("check-email")]
        public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return Ok(user == null);
        }

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginDTO">User login data.</param>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> PostLogin(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(GetModelErrors());
            }

            var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, false, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning($"Failed login attempt for {loginDTO.Email}.");
                return Unauthorized("Invalid email or password");
            }

            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user is null)
            {
                return NotFound("User not found.");
            }

            await SignInUser(user);
            var authenticationResponse = await GenerateJwtToken(user);
            _logger.LogInformation($"User {user.Email} logged in successfully.");
            return Ok(authenticationResponse);
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        [HttpGet("logout")]
        public async Task<IActionResult> GetLogout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out successfully.");
            return NoContent();
        }

        /// <summary>
        /// Signs in the specified user.
        /// </summary>
        /// <param name="user">User to sign in.</param>
        private async Task SignInUser(ApplicationUser user)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">User to generate token for.</param>
        /// <returns>Authentication response containing JWT and refresh token.</returns>
        private async Task<AuthenticationResponse> GenerateJwtToken(ApplicationUser user)
        {
            var authenticationResponse = _jwtService.CreateJwtToken(user);
            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);
            return authenticationResponse;
        }

        /// <summary>
        /// Retrieves model validation errors from the current ModelState.
        /// </summary>
        /// <returns>Formatted error message string.</returns>
        private string GetModelErrors()
        {
            return string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        }

        /// <summary>
        /// Retrieves identity errors from the specified IdentityResult.
        /// </summary>
        /// <param name="result">IdentityResult to extract errors from.</param>
        /// <returns>Formatted error message string.</returns>
        private static string GetIdentityErrors(IdentityResult result)
        {
            return string.Join(" | ", result.Errors.Select(e => e.Description));
        }
    }
}
