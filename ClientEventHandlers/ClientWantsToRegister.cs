using System.Text.Json;
using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToRegisterDto : BaseDto
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

}

public class ClientWantsToRegister : BaseEventHandler<ClientWantsToRegisterDto>
{
    private AccountService _accountService;

    public ClientWantsToRegister(AccountService accountService)
    {
        _accountService = accountService;
    }

    public override async Task Handle(ClientWantsToRegisterDto dto, IWebSocketConnection socket)
    {
        var userId = _accountService.Register(dto.Username, dto.Email, dto.Password);
        if (userId <= 0)
        {
            // Registration failed
            var response = new ServerConfirmsRegistration()
            {
                Message = "Registration failed"
            };
            await socket.Send(JsonSerializer.Serialize(response));
        }
        else
        {
            // Registration successful
            var response = new ServerConfirmsRegistration()
            {
                Message = "Registration successful",
                UserId = userId
            };

            await socket.Send(JsonSerializer.Serialize(response));
        }
    }
}

public class ServerConfirmsRegistration : BaseDto
{
    public string Message { get; set; }
    public int UserId { get; set; }
}