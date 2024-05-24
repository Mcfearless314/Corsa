using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToSaveARunDto : BaseDto
{
    public string? RunDateTime { get; set; }

    public int UserId { get; set; }

    public double RunDistance { get; set; }

    public string? RunTime { get; set; }

}

public class ClientWantsToSaveARun : BaseEventHandler<ClientWantsToSaveARunDto>
{
    private RunService _runService;

    public ClientWantsToSaveARun(RunService runService)
    {
        _runService = runService;
    }
    public override async Task Handle(ClientWantsToSaveARunDto dto, IWebSocketConnection socket)
    {
        var runSaved = await _runService.SaveRunToDb(dto.UserId, dto.RunDateTime, dto.RunTime, dto.RunDistance);
        await socket.Send(runSaved);
    }
}