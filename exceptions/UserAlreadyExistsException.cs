using lib;

namespace Backend.exceptions;

public class UserAlreadyExistsException(string message) : Exception(message);

public class UserAlreadyExistsExceptionDto : BaseDto
{
    public string errorMessage { get; set; }
}
