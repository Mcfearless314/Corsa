using System.Text.Json;
using Backend.service;
using Backend.infrastructure.dataModels;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToSeeAProgressOfAllRunsDto  : BaseDto
{
    public int UserId { get; set; }
}

public class ClientWantsToSeeAProgressOfAllRuns : BaseEventHandler<ClientWantsToSeeAProgressOfAllRunsDto>
{
    private RunService _runService;

    public ClientWantsToSeeAProgressOfAllRuns(RunService runService)
    {
        _runService = runService;
    }

    public override async Task Handle(ClientWantsToSeeAProgressOfAllRunsDto dto, IWebSocketConnection socket)
    {
        List<ProgressInfo> listOfAllRuns = await _runService.GetProgressOfRunsForUser(dto.UserId);
        
        var response = new ServerSendsBackAllProgress()
        {
            AllProgress = listOfAllRuns
        };
        
        await socket.Send(JsonSerializer.Serialize(response));
    }
}

public class ServerSendsBackAllProgress : BaseDto
{
    public List<ProgressInfo> AllProgress { get; set; }
}