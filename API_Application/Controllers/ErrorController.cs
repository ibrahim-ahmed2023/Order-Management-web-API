using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderManagement.WebAPI.Exceptions;
using System.Net;

namespace OrderManagement.WebAPI.Controllers
{
    /// <summary>
    /// Controller responsible for handling application-wide errors.
    /// </summary>
    [ApiController]
    [Route("error")]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorController"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for logging errors.</param>
        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles unhandled exceptions and returns a standardized error response.
        /// </summary>
        /// <returns>A problem details response with error information.</returns>
        [HttpGet]
        public IActionResult HandleError()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (context?.Error == null)
            {
                return Ok(new { message = "No error occurred." });
            }

            var exception = context.Error;
            var statusCode = exception switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,               // 404
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,       // 401
                BadRequestException => (int)HttpStatusCode.BadRequest,           // 400
                ForbiddenException => (int)HttpStatusCode.Forbidden,             // 403
                InternalServerException => (int)HttpStatusCode.InternalServerError, // 500
                ConflictException => (int)HttpStatusCode.Conflict,               // 409
                _ => (int)HttpStatusCode.InternalServerError
            };

            _logger.LogError(exception, "Unhandled exception occurred.");

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = "An error occurred",
                Detail = exception.Message,
                Instance = HttpContext.Request.Path
            };

            return StatusCode(statusCode, problemDetails);
        }
    }
}
