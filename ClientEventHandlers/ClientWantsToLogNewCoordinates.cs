﻿using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToLogNewCoordinatesDto : BaseDto
{
    public DateTime LoggingTime { get; set; }

    public double Lat { get; set; }

    public double Lng { get; set; }
    
    public double RunId { get; set; }
}

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
