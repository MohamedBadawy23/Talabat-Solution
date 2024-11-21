using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using Talabat.APIs.Errors;

namespace Talabat.APIs.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
              await  _next.Invoke(context);

            }catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                // Log Exception in Database [Production]

                // Header of Response Message
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;



                var response = _env.IsDevelopment() ?
                    new ApiExceptionResponse(500, ex.StackTrace.ToString(), ex.Message)
                    : new ApiExceptionResponse(500);

                var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                // Body of Response Message
                 await context.Response.WriteAsync(json);


            }



        }


    }
}
