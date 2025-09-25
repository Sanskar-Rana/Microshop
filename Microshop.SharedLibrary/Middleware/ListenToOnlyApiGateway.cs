using Microsoft.AspNetCore.Http;

namespace Microshop.SharedLibrary.Middleware;

public class ListenToOnlyApiGateway(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        //Extract specific header from the request
        var signedHeader = context.Request.Headers["Api-Gateway"];

        //NULL- The request is not coming from the API GATEWAY, 503 service unavailbe for the user
        if (signedHeader.FirstOrDefault() is null)
        {
            context. Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("Service unavailable");
            return;
        }
        else
        {
            await next(context);
        }
    }
}