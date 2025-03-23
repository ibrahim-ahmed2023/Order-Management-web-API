using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderManagement.WebAPI.Exceptions;
using System.Net;
using System.Text.Json;

namespace OrderManagement.WebAPI.Middleware
{
    /// <summary>
    /// Middleware to handle exceptions globally and return proper HTTP responses.
    /// Not Used
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next delegate in the request pipeline.</param>
        /// <param name="logger">Logger instance for recording exceptions.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware and handles exceptions.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                BadRequestException => (int)HttpStatusCode.BadRequest,
                ForbiddenException => (int)HttpStatusCode.Forbidden,
                ConflictException => (int)HttpStatusCode.Conflict,
                InternalServerException => (int)HttpStatusCode.InternalServerError,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = exception.Message
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Standard error response structure.
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
    }
}
