using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VirtualWallet.Domain.Exceptions;

namespace VirtualWallet.Api.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Logueamos el error en los logs del servidor para tener un registro.
            _logger.LogError(exception, "An exception occurred: {Message}", exception.Message);

            // Respuesta estandar.
            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            // Indentificamos el tipo de error.
            if (exception is ValidationException fluentException)
            {
                problemDetails.Title = "Validation Error";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = "One or more fields have formatting errors.";

                // Extraemos los errores exactos de FluentValidation y los mandamos al frontend
                var validationErrors = fluentException.Errors
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(group => group.Key, group => group.ToArray());

                problemDetails.Extensions.Add("errors", validationErrors);
            }
            else if (exception is BadRequestException badRequestException)
            {
                problemDetails.Title = "Business rule not followed";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = badRequestException.Message;
            }
            else
            {
                // En caso de que sea un error inesperado.
                problemDetails.Title = "Internal Server Error";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Detail = "An unexpected error occurred. Please try again later.";
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; 
        }
    }
}
