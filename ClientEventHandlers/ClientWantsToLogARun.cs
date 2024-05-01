﻿using Backend.infrastructure;
using Backend.service;
using Fleck;
using lib;
using Npgsql;

namespace Backend.ClientEventHandlers;

public class ClientWantsToLogARunDto : BaseDto
{
    public DateTime RunStartTime { get; set; }

    public double StartingLat { get; set; }

    public double StartingLng { get; set; }
    
    public double UserId { get; set; }
}

public class ClientWantsToLogARun : BaseEventHandler<ClientWantsToLogARunDto>
{
    private RunService _runService;

    public ClientWantsToLogARun(RunService runService)
    {
        _runService = runService;
    }

    public override async Task Handle(ClientWantsToLogARunDto dto, IWebSocketConnection socket)
    {
        var runStarted = await _runService.LogRunToDb(dto.UserId, dto.StartingLat, dto.StartingLng, dto.RunStartTime);
        await socket.Send(runStarted);

    }
}