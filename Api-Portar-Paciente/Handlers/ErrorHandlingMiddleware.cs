using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
// Removed System.Web.Http dependency for .NET 8 compatibility

namespace Api_Portar_Paciente.Handlers
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        private readonly ExceptionControl exceptionControl;

        public ErrorHandlingMiddleware(RequestDelegate next, ExceptionControl exceptionControl, ILogger logger)
        {
            this.next = next;
            this.logger = logger;
            this.exceptionControl = exceptionControl;
        }

        public async Task InvokeAsync(HttpContext context /* other scoped dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (InvalidOperationException invopexp)
            {
                logger.LogError(invopexp.Message);
                await HandleExceptionAsync(context, invopexp, invopexp.Message);
            }
            catch (Exception ex) when (ex != null)
            {
                var message = exceptionControl.GetExceptionMessage(ex);
                logger.LogError(ex.Message, ex);
                await HandleExceptionAsync(context, ex, message);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, string message)
        {
            string result = string.Empty;
            HttpStatusCode code = HttpStatusCode.InternalServerError; // 500 if unexpected

            code = GetHttpCode(exception, code);
            result = GetHttpResult(exception, message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }

        private static HttpStatusCode GetHttpCode(Exception exception, HttpStatusCode code)
        {
            HttpStatusCode newCode = 0;
            if (exception is SystemException || exception is ValidationException)
            {
                newCode = HttpStatusCode.BadRequest;
            }
            // Custom unauthorized exceptions could be handled here with a specific type
            else if (exception is InvalidOperationException)
            {
                newCode = HttpStatusCode.UnprocessableEntity;
            }

            return newCode != 0 ? newCode : code;
        }

        private static string GetHttpResult(Exception exception, string message)
        {
            string result;

            if (exception is ValidationException ve)
            {
                var validation = ve.ValidationResult;
                result = JsonConvert.SerializeObject(validation);
            }
            else
            {
                result = JsonConvert.SerializeObject(new { errors = message });
            }

            return result;
        }
    }
}