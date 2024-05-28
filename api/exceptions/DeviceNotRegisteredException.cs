namespace Backend.exceptions;

public class DeviceNotRegisteredException(string errorMessage) : Exception(errorMessage);

public class DeviceNotRegisteredExceptionDto
{
    public string errorMessage { get; set; }
}