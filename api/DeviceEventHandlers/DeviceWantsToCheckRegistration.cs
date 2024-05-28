using System.Text.Json;
using Backend.service;
using Fleck;
using lib;

namespace Backend.DeviceEventHandlers;

public class DeviceWantsToCheckRegistrationDto : BaseDto
{
    public string DeviceId { get; set; }
}

public class DeviceWantsToCheckRegistration : BaseEventHandler<DeviceWantsToCheckRegistrationDto>
{
    private DeviceService _deviceService;

    public DeviceWantsToCheckRegistration(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public override async Task Handle(DeviceWantsToCheckRegistrationDto dto, IWebSocketConnection socket)
    {
        bool isDeviceRegistered = await _deviceService.IsDeviceRegisteredInDb(dto.DeviceId);
        var response = new ServerSendsBackDeviceIsRegisteredDto {IsRegistered = isDeviceRegistered};
        await socket.Send(JsonSerializer.Serialize(response));
    }
}

public class ServerSendsBackDeviceIsRegisteredDto : BaseDto
{
    public bool IsRegistered { get; set; }
}
