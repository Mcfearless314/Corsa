using System.Text.Json;
using Backend.infrastructure.dataModels;
using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToSeeAllSavedRunsDto : BaseDto
{
    public int UserId { get; set; }
}

public class ClientWantsToSeeAllSavedRuns : BaseEventHandler<ClientWantsToSeeAllSavedRunsDto>
{
    private RunService _runService;

    public ClientWantsToSeeAllSavedRuns(RunService runService)
    {
        _runService = runService;
    }

    public override async Task Handle(ClientWantsToSeeAllSavedRunsDto dto, IWebSocketConnection socket)
    {
        List<RunInfo> listOfAllRuns = await _runService.GetAllRunsForUser(dto.UserId);
        await socket.Send(JsonSerializer.Serialize(listOfAllRuns));
    }
}