namespace DoctorBooking.MiddleWares
{
    public class GlobalExceptionHanlderMiddleWare
    {
        private readonly RequestDelegate next;

        public GlobalExceptionHanlderMiddleWare( RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch(Exception ex)
            {
                await context.Response.WriteAsJsonAsync(new
                {
                    ex.Message
                });
            }
        }
    }
}
