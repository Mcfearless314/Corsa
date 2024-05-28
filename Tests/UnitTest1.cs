using System.Text.Json;
using Backend;
using Backend.ClientEventHandlers;
using Backend.exceptions;
using lib;
using Websocket.Client;

namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        Startup.Statup(null);
    }


    [Test]
    public async Task RegistrationTest()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();
        await ws.DoAndAssert(new ClientWantsToRegisterDto
            {
                Username = "Miran",
                Email = "Miran@gmail.com",
                Password = "Test12345",
            },
            fromServer =>
            {
                return fromServer.Count(dto => dto.eventType == nameof(ServerConfirmsRegistration)) == 1;
            }
        );
        
        await ws.DoAndAssert(new ClientWantsToRegisterDto
            {
                Username = "Miran",
                Email = "Test@gmail.com",
                Password = "Test12345",
            },
            fromServer =>
            {
                return fromServer.Count(dto => dto.eventType == nameof(UserAlreadyExistsExceptionDto)) == 1;
            }
        );
        
    }

    [Test]
    public async Task LoginTests()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();
        await ws.DoAndAssert(new ClientWantsToLogInDto
            {
                Username = "test",
                Password = "test",
            },
            fromServer =>
            {
                return fromServer.Count(dto => dto.eventType == nameof(AuthenticationFailureExceptionDto)) == 1;
            }
        );

        var ws2 = await new WebSocketTestClient().ConnectAsync();
        await ws2.DoAndAssert(new ClientWantsToLogInDto
        {
            Username = "Miran",
            Password = "Test12345",
            
        },
            fromServer =>
            {
                return fromServer.Count(dto => dto.eventType == nameof(ServerConfirmsLogin)) == 1;
            }
        );
        
    }

    [Test]
    public async Task LogARunTest()
    {
        var ws2 = await new WebSocketTestClient().ConnectAsync();
        await ws2.DoAndAssert(new ClientWantsToLogARunDto
        {
            UserId = 1,
            RunStartTime = DateTime.Now,
            StartingLat = 1.0,
            StartingLng = 1.0,
        },
            fromServer =>
            {
                return fromServer.Count(dto => dto.eventType == nameof(ServerSendsBackRunId)) == 1;
            }
        );
        
        
    }

}