using System.Security.Authentication;
using System.Text.Json;
using Fleck;
using lib;

namespace Backend;

public class GlobalExceptionHandler
{
    public static void Handle(Exception exception, IWebSocketConnection ws, string? message)
    {
        Console.WriteLine(exception.Message);
        Console.WriteLine(exception.InnerException);
        Console.WriteLine(exception.StackTrace);
        if (exception is AuthenticationException)
        {
            
            //TODO Switch this out with an real BaseDTO to handle the exceptions you want
            ws.Send(JsonSerializer.Serialize(new Example
            {
                errorMessage = exception.Message
            }));
        }
    }
}

public class Example : BaseDto
{
    public string errorMessage { get; set; }
}