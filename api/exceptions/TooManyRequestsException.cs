namespace Backend.exceptions;

public class TooManyRequestsException(string errorMessage) : Exception(errorMessage);

public class TooManyRequestsExceptionDto
{
    public string errorMessage { get; set; }
}