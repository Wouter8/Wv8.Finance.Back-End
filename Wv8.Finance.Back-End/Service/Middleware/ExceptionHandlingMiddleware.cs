namespace PersonalFinance.Service.Middleware
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// Middleware that catches custom exceptions which gets nicely replied to the client.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        /// <summary>
        /// The next middleware in the pipeline.
        /// </summary>
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// The method that gets automatically called by .NET Core.
        /// </summary>
        /// <param name="httpContext">The current context.</param>
        /// <returns>Nothing.</returns>
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await this.next(httpContext);
            }
            catch (CustomException e)
            {
                httpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                await httpContext.Response.WriteAsync(e.Message);
            }
            catch (Exception e)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpContext.Response.WriteAsync(e.Message);
            }
        }
    }
}