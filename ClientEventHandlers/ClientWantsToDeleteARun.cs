using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToDeleteARunDto : BaseDto
{
    public int UserId { get; set; }

    public string RunId { get; set; }
}

public class ClientWantsToDeleteARun : BaseEventHandler<ClientWantsToDeleteARunDto>
{
    private RunService _runService;

    public ClientWantsToDeleteARun(RunService runService)
    {
        _runService = runService;
    }

    public override async Task Handle(ClientWantsToDeleteARunDto dto, IWebSocketConnection socket)
    {
        var runDeleted = await _runService.DeleteRunFromDb(dto.UserId, dto.RunId);
        await socket.Send(runDeleted);
    }
}