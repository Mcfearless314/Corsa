using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToRegisterADeviceDto : BaseDto
{
    [Required]
    [MinLength(8)]
    public string DeviceId { get; set; }
    public int UserId { get; set; }
}

public class ClientWantsToRegisterADevice : BaseEventHandler<ClientWantsToRegisterADeviceDto> {
    
    private AccountService _accountService;
    
    public ClientWantsToRegisterADevice(AccountService accountService)
    {
        _accountService = accountService;
    }
    
    
    public override async Task Handle(ClientWantsToRegisterADeviceDto dto, IWebSocketConnection socket)
    {
        string deviceId = await _accountService.RegisterDevice(dto.UserId, dto.DeviceId);
        var response = new ServerConfirmsDeviceRegistration
        {
            Message = "Device registration successful",
            DeviceId = deviceId
        };
        await socket.Send(JsonSerializer.Serialize(response));
    }
}

public class ServerConfirmsDeviceRegistration : BaseDto
{
    public string Message { get; set; }
    public string DeviceId { get; set; }
}