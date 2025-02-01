using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
                // No error occurred, return a success response or a neutral message
                return Ok(new { message = "No error occurred." });
            }

            var exception = context.Error;
            // Log the exception details
            _logger.LogError(exception, "Unhandled exception occurred.");

            // Return the problem response with details of the error
            return Problem(
                detail: exception.Message,
                title: "An internal server error occurred",
                statusCode: 500
            );
        }

    }
}