using System.Text.Json;
using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToRegisterDto : BaseDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
}

public class ClientWantsToRegister : BaseEventHandler<ClientWantsToRegisterDto>
{
    private AccountService _accountService;

    public ClientWantsToRegister(AccountService accountService)
    {
        _accountService = accountService;
    }

    public override Task Handle(ClientWantsToRegisterDto dto, IWebSocketConnection socket)
    {
        var user = _accountService.Register(dto.Username, dto.Email, dto.Password);
        socket.Send(JsonSerializer.Serialize(user));
        return Task.CompletedTask;
    }
}
