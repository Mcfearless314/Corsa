using System.Text.Json;
using Backend.EventFilters;
using Backend.infrastructure.dataModels;
using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToSeeFullInfoOfRunDto : BaseDto
{
    public int UserId { get; set; }
    
    public string RunId { get; set; }
}

[AuthenticationFilter]
public class ClientWantsToSeeFullInfoOfRun : BaseEventHandler<ClientWantsToSeeFullInfoOfRunDto>
{
    private RunService _runService;

    public ClientWantsToSeeFullInfoOfRun(RunService runService)
    {
        _runService = runService;
    }

    public override async Task Handle(ClientWantsToSeeFullInfoOfRunDto dto, IWebSocketConnection socket)
    {
        var runInfo = await _runService.GetFullInfoOfRun(dto.UserId, dto.RunId);
        
        var response = new ServerSendsBackFullRunInfo
        {
            FullRunInfo = runInfo
        };
        
        await socket.Send(JsonSerializer.Serialize(response));
    }
}

public class ServerSendsBackFullRunInfo : BaseDto
{
    public RunInfoWithMap FullRunInfo { get; set; }
}