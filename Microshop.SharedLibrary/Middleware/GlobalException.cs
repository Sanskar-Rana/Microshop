using System.Net;
using System.Text.Json;
using Microshop.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microshop.SharedLibrary.Middleware;

public class GlobalException(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        
        //Default declare variables
        string message = "Internal Server Error!, Kindly try again";
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string title = "Interenal Server Error";

        try
        {
            await next(context);
            //Response
            //Check for - 429 status code
            if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
            {
                title = "Warning";
                message = "Too Many Requests";
                statusCode = (int)HttpStatusCode.TooManyRequests;

                await ModifyHeader(context, title, message, statusCode);
            }

            //Check for - 401 status code
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                title = "Unauthorized";
                message = "You are not authorized to access this resource";
                statusCode = (int)HttpStatusCode.Unauthorized;

                await ModifyHeader(context, title, message, statusCode);
            }
            
            //Check for - 403 status code
            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                title = "Forbidden";
                message = "You do not have permission to access this resource";
                statusCode = (int)HttpStatusCode.Forbidden;

                await ModifyHeader(context, title, message, statusCode);
            }
        }
        catch (Exception ex)
        {
            //Log Original Exception - Log to file, debugger and console
            LogException.LogExceptions(ex);
            
            //Check if the exception is a timeout --  408 status code
            if (ex is TaskCanceledException || ex is TimeoutException)
            {
                title = "Timeout";
                message = "Request timeout, try again";
                statusCode = (int)HttpStatusCode.RequestTimeout;
            }
            
            //If exception is caught, none of the exceptions then do the default
            await ModifyHeader(context, title, message, statusCode);
        }
    }
    
    private static async Task ModifyHeader(HttpContext context, string tilte, string message, int statusCode)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.Clear(); //Clear any partially written response
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails
            {
                
                Detail = tilte,
                Status = statusCode,
                Title = tilte,
                
            }));
        }
        else
        {
            //Optional: Log that couldn't modify header
            LogException.LogExceptions(new InvalidOperationException(
                "Reponse has already started, cannot modify headers."));
        }
    }

}