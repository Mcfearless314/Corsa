using System;
using System.Threading.Tasks;
using Backend.EventFilters;
using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToLogNewCoordinatesDto : BaseDto
{
    public DateTime LoggingTime { get; set; }

    public double Lat { get; set; }

    public double Lng { get; set; }
    
    public string RunId { get; set; }
}

[AuthenticationFilter]
public class ClientWantsToLogNewCoordinates : BaseEventHandler<ClientWantsToLogNewCoordinatesDto>
{
    private RunService _runService;

    public ClientWantsToLogNewCoordinates(RunService runService)
    {
        _runService = runService;
    }

    public override async Task Handle(ClientWantsToLogNewCoordinatesDto dto, IWebSocketConnection socket)
    {
        await _runService.LogCoordinatesToDb(dto.RunId, dto.Lat, dto.Lng, dto.LoggingTime);
    }
}
