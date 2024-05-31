using System.Text.Json;
using Backend.EventFilters;
using Backend.infrastructure.dataModels;
using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToStopARunDto : BaseDto
{
    public DateTime RunEndTime { get; set; }

    public double EndingLat { get; set; }

    public double EndingLng { get; set; }

    public string RunId { get; set; }
}

[AuthenticationFilter]
public class ClientWantsToStopARun : BaseEventHandler<ClientWantsToStopARunDto>
{
    private RunService _runService;

    public ClientWantsToStopARun(RunService runService)
    {
        _runService = runService;
    }

    public override async Task Handle(ClientWantsToStopARunDto dto, IWebSocketConnection socket)
    {
         var runCompleted = await _runService.LogEndingOfRunToDb(dto.RunId, dto.EndingLat, dto.EndingLng, dto.RunEndTime);
         var response = new ServerSendsBackRunWithMap
         {
             Message = "Run successfully stopped: " + runCompleted.RunId,
             FullRunInfo = runCompleted
         };
         
            await socket.Send(JsonSerializer.Serialize(response));
             
             

    }
}

public class ServerSendsBackRunWithMap : BaseDto
{
    public string Message { get; set; }
    public RunInfoWithMap FullRunInfo { get; set; }
}