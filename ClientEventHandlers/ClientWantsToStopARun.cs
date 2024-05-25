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

public class ClientWantsToStopARun : BaseEventHandler<ClientWantsToStopARunDto>
{
    private RunService _runService;

    public ClientWantsToStopARun(RunService runService)
    {
        _runService = runService;
    }

    public override async Task Handle(ClientWantsToStopARunDto dto, IWebSocketConnection socket)
    {
         await _runService.LogEndingOfRunToDb(dto.RunId, dto.EndingLat, dto.EndingLng, dto.RunEndTime);

    }
}

public class ServerConfirmsRunStopped : BaseDto
{
    public string Message { get; set; }
    public string runId { get; set; }
}