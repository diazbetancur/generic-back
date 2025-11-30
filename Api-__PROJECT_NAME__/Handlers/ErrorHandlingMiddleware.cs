using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Api_Portar_Paciente.Handlers
{
    /// <summary>
    /// Middleware para manejo centralizado de excepciones con logging estructurado
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ErrorHandlingMiddleware> logger;
        private readonly ExceptionControl exceptionControl;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ExceptionControl exceptionControl,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
            this.exceptionControl = exceptionControl;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.TraceIdentifier;

            try
            {
                await next(context);
            }
            catch (DbUpdateConcurrencyException concurrencyEx)
            {
                logger.LogWarning(concurrencyEx,
                    "Conflicto de concurrencia detectado. CorrelationId: {CorrelationId}, Path: {Path}",
                    correlationId, context.Request.Path);

                await HandleExceptionAsync(context, concurrencyEx,
                    "El registro ha sido modificado por otro usuario. Por favor, recarga los datos e intenta nuevamente.");
            }
            catch (UnauthorizedAccessException unauthorizedException)
            {
                logger.LogWarning(unauthorizedException,
                    "Acceso no autorizado. CorrelationId: {CorrelationId}, Path: {Path}, User: {User}",
                    correlationId, context.Request.Path, context.User?.Identity?.Name ?? "Anonymous");

                await HandleExceptionAsync(context, unauthorizedException, "Acceso no autorizado");
            }
            catch (KeyNotFoundException notFoundEx)
            {
                logger.LogWarning(notFoundEx,
                    "Recurso no encontrado. CorrelationId: {CorrelationId}, Path: {Path}, Message: {Message}",
                    correlationId, context.Request.Path, notFoundEx.Message);

                await HandleExceptionAsync(context, notFoundEx, notFoundEx.Message);
            }
            catch (InvalidOperationException invopexp)
            {
                logger.LogError(invopexp,
                    "Operación inválida. CorrelationId: {CorrelationId}, Path: {Path}, Message: {Message}",
                    correlationId, context.Request.Path, invopexp.Message);

                await HandleExceptionAsync(context, invopexp, invopexp.Message);
            }
            catch (ValidationException validationEx)
            {
                logger.LogWarning(validationEx,
                    "Error de validación. CorrelationId: {CorrelationId}, Path: {Path}",
                    correlationId, context.Request.Path);

                await HandleExceptionAsync(context, validationEx, validationEx.Message);
            }
            catch (SystemException systemEx)
            {
                logger.LogError(systemEx,
                    "Error del sistema. CorrelationId: {CorrelationId}, Path: {Path}",
                    correlationId, context.Request.Path);

                await HandleExceptionAsync(context, systemEx, systemEx.Message);
            }
            catch (Exception ex) when (ex != null)
            {
                var message = exceptionControl.GetExceptionMessage(ex);

                logger.LogError(ex,
                    "Error no controlado. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}, Message: {Message}",
                    correlationId, context.Request.Path, context.Request.Method, message);

                await HandleExceptionAsync(context, ex, message);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, string message)
        {
            var code = GetHttpCode(exception);
            var result = GetHttpResult(exception, message, context.TraceIdentifier);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }

        private static HttpStatusCode GetHttpCode(Exception exception)
        {
            // El orden importa: de más específico a más general
            return exception switch
            {
                DbUpdateConcurrencyException => HttpStatusCode.Conflict,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                KeyNotFoundException => HttpStatusCode.NotFound,
                ValidationException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.UnprocessableEntity,
                SystemException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };
        }

        private static string GetHttpResult(Exception exception, string message, string correlationId)
        {
            if (exception is ValidationException ve)
            {
                var validation = ve.ValidationResult;
                return JsonConvert.SerializeObject(new
                {
                    errors = validation,
                    correlationId
                });
            }

            if (exception is DbUpdateConcurrencyException)
            {
                return JsonConvert.SerializeObject(new
                {
                    error = message,
                    type = "ConcurrencyConflict",
                    correlationId
                });
            }

            return JsonConvert.SerializeObject(new
            {
                error = message,
                correlationId
            });
        }
    }
}