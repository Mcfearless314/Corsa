namespace Backend.exceptions;

public class LoggingOfRunFailedException(string errorMessage) : Exception(errorMessage);

public class LoggingOfRunFailedExceptionDto
{
    public string errorMessage { get; set; }
}