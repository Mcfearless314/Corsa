using System.Text.Json;
using Backend.infrastructure.dataModels;
using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToLogInDto : BaseDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class ClientWantsToLogIn : BaseEventHandler<ClientWantsToLogInDto>
{
    private AccountService _accountService;
    private JwtService _jwtService;

    public ClientWantsToLogIn(AccountService accountService, JwtService jwtService)
    {
        _accountService = accountService;
        _jwtService = jwtService;
    }

    public override async Task Handle(ClientWantsToLogInDto dto, IWebSocketConnection socket)
    {
        var user = _accountService.Authenticate(dto.Username, dto.Password);
        if (user == null)
        {
            // Authentication failed
            await socket.Send(JsonSerializer.Serialize(new ResponseDto
            {
                MessageToClient = "Authentication failed",
                ResponseData = null
            }));
            return;
        }

        // Creating a token from the user
        var token = _jwtService.IssueToken(SessionData.FromUser(user));

        // Send the token to the client
        await socket.Send(JsonSerializer.Serialize(new ResponseDto
        {
            MessageToClient = "Successfully authenticated",
            ResponseData = new { token }
        }));
    }
}