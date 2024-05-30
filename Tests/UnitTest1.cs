using Backend;
using Microsoft.Extensions.Configuration;
using Websocket.Client;
using Backend.service;
using Backend.ClientEventHandlers;
using Backend.exceptions;
using Backend.infrastructure;
using lib;
using Npgsql;



namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        Startup.Statup(null);
        
        using var conn = new NpgsqlConnection(DatabaseConnector.ProperlyFormattedConnectionString);
        conn.Open();

        // Read the SQL script file
        var sqlScript = File.ReadAllText("../../../../api/SQLDatabaseSetup.sql");

        // Execute the SQL script
        using var cmd = new NpgsqlCommand(sqlScript, conn);
        cmd.ExecuteNonQuery();
    }


    

    [Test]
    public async Task Test_1RegistrationTest()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();
        await ws.DoAndAssert(new ClientWantsToRegisterDto
            {
                Username = "Miran",
                Email = "Miran@gmail.com",
                Password = "Test12345",
            },
            fromServer => { return fromServer.Count(dto => dto.eventType == nameof(ServerConfirmsRegistration)) == 1; }
        );

        await ws.DoAndAssert(new ClientWantsToRegisterDto
            {
                Username = "Miran",
                Email = "Test@gmail.com",
                Password = "Test12345",
            },
            fromServer => { return fromServer.Count(dto => dto.eventType == nameof(UserAlreadyExistsException)) == 1; }
        );
    }
    
    [Test]
    public async Task Test_2LoginTests()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();
        await ws.DoAndAssert(new ClientWantsToLogInDto
            {
                Username = "Test12345",
                Password = "Test12345",
            },
            fromServer =>
            {
                return fromServer.Count(dto => dto.eventType == nameof(AuthenticationFailureException)) == 1;
            }
        );

        var ws2 = await new WebSocketTestClient().ConnectAsync();
        await ws2.DoAndAssert(new ClientWantsToLogInDto
            {
                Username = "Miran",
                Password = "Test12345",
            },
            fromServer => { return fromServer.Count(dto => dto.eventType == nameof(ServerConfirmsLogin)) == 1; }
        );
    }

    [Test]
    public async Task Test_3LogARunTest()
    {
        var ws2 = await new WebSocketTestClient().ConnectAsync();
        await ws2.DoAndAssert(new ClientWantsToLogARunDto
            {
                UserId = 1,
                RunStartTime = DateTime.Now,
                StartingLat = 1.0,
                StartingLng = 1.0,
            },
            fromServer => { return fromServer.Count(dto => dto.eventType == nameof(ServerSendsBackRunId)) == 1; }
        );
    }


    [Test]
    public async Task Test_4RegisterADevice()
    {
        var ws2 = await new WebSocketTestClient().ConnectAsync();
        await ws2.DoAndAssert(new ClientWantsToRegisterADeviceDto
            {
                UserId = 1,
                DeviceId = "123456789",
            },
            fromServer =>
            {
                return fromServer.Count(dto => dto.eventType == nameof(ServerConfirmsDeviceRegistration)) == 1;
            }
        );
    }

    [Test]
    public async Task Test_5CalculateDistanceBetweenTwoCoordinates()
    {
        var calc = new DistanceCalculator();
        var distance =
            calc.CalculateDistance(55.487986571921624, 8.446922438743762, -37.80983071669535, 144.96254276912558) /
            1000;
        var roundedDistance = Math.Round(distance, 0);
        Assert.That(roundedDistance, Is.EqualTo(16245));
    }
}