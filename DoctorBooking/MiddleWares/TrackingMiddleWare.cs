namespace DoctorBooking.MiddleWares
{
    public class TrackingMiddleWare
    {
        private readonly ILogger<TrackingMiddleWare> logger;
        private readonly RequestDelegate next;

        public TrackingMiddleWare(ILogger<TrackingMiddleWare> logger, RequestDelegate next)
        {
            this.logger = logger;
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            logger.LogInformation("Request: {method} {path}", request.Method, request.Path);

            await next(context);

            await context.Response.WriteAsync("Request logged.");
        }
    }
}
