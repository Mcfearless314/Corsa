using lib;

namespace Backend.exceptions;

public class AuthenticationFailureException(String message) : Exception(message);

public class AuthenticationFailureExceptionDto : BaseDto
{
    public string errorMessage { get; set; }
}