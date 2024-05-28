using lib;

namespace Backend.exceptions;

public class DeviceAlreadyRegisteredException(string message) : Exception(message);


public class DeviceAlreadyRegisteredExceptionDto : BaseDto
{
    public string errorMessage { get; set; }
}