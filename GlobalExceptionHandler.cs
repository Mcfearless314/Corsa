using System.Security.Authentication;
using System.Text.Json;
using Backend.exceptions;
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
        if (exception is UserAlreadyExistsException)
        {
            ws.Send(JsonSerializer.Serialize(new UserAlreadyExistsExceptionDto
            {
                errorMessage = exception.Message
            }));
        }
        else if (exception is AuthenticationFailureException)
        {
            ws.Send(JsonSerializer.Serialize(new AuthenticationFailureExceptionDto
            {
                errorMessage = exception.Message
            }));
        }
        else if (exception is DeviceAlreadyRegisteredException)
        {
            ws.Send(JsonSerializer.Serialize(new DeviceAlreadyRegisteredExceptionDto
            {
                errorMessage = exception.Message
            }));
        }
    }
}

