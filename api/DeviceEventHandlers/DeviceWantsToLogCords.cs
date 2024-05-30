using System.ComponentModel.DataAnnotations;
using Backend.infrastructure.dataModels;
using Backend.service;
using Fleck;
using lib;

namespace Backend.DeviceEventHandlers;

public class DeviceWantsToLogCordsDto : BaseDto
{
    [Required]
    [MinLength(8)]
    public string DeviceId { get; set; }
    public List<Cords> gpsCordsList { get; set; }
}

public class DeviceWantsToLogCords : BaseEventHandler<DeviceWantsToLogCordsDto>
{
    private DeviceService _deviceService;

    public DeviceWantsToLogCords(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public override async Task Handle(DeviceWantsToLogCordsDto dto, IWebSocketConnection socket)
    {
        await _deviceService.LogCoordinates(dto.DeviceId, dto.gpsCordsList);
    }
}