﻿using System.Text.Json;
using System.Threading.Tasks;
using Backend.EventFilters;
using Backend.exceptions;
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

[RateLimiter(6,30,2,1)]
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
    
        // Creating a token from the user
        var token = _jwtService.IssueToken(SessionData.FromUser(user!));

        // Send the token to the client
        await socket.Send(JsonSerializer.Serialize(new ServerConfirmsLogin
        {
            Message = "Successfully authenticated",
            Token = new { token },
            UserId = user!.id
            
        }));
    }
    
}

public class ServerConfirmsLogin : BaseDto
{
    public string Message { get; set; }
    public object? Token { get; set; }
    public int UserId { get; set; }
}

public class ServerDeniesLogin : BaseDto
{
    public string Message { get; set; }
}