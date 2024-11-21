namespace Talabat.APIs.Errors
{
    public class ApiExceptionResponse : ApiResponse
    {
        public string? Details { get; set; }

        public ApiExceptionResponse(int statusCode, string? details = null, string? message = null)
            : base(statusCode,message)
        {
            Details = details;
        }
    }
}
