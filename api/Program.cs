using System.Reflection;
using System.Text.Json.Serialization;
using Backend.infrastructure;
using Backend.infrastructure.Repositories;
using Backend.service;
using Fleck;
using lib;


namespace Backend;

public static class Startup
{
    public static void Main(string[] args)
    {
        var app =Statup(args);
        app.Run();
    }

    public static WebApplication Statup(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpClient();
        builder.Services.AddJwtService();
        builder.Services.AddSingleton<RunService>();
        builder.Services.AddSingleton<AccountService>();
        builder.Services.AddSingleton<DeviceService>();
        builder.Services.AddSingleton<RunRepository>();
        builder.Services.AddSingleton<UserRepository>();
        builder.Services.AddSingleton<DeviceRepository>();
        builder.Services.AddSingleton<PasswordHashRepository>();
        builder.Services.AddSingleton<Argon2idPasswordHashAlgorithm>();
        builder.Services.AddSingleton<MQTTClientService>();

        builder.Services.AddNpgsqlDataSource(DatabaseConnector.ProperlyFormattedConnectionString,
            dataSourceBuilder => dataSourceBuilder.EnableParameterLogging());

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        var clientEventHandlers = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());

        var app = builder.Build();
        builder.WebHost.UseUrls("http://*:4545");

        var port = Environment.GetEnvironmentVariable("PORT") ?? "8181";
        var server = new WebSocketServer("ws://0.0.0.0:"+port);

        server.Start(ws =>
        {
            var keepAliveInterval = TimeSpan.FromSeconds(30);
            var keepAliveTimer = new System.Timers.Timer(keepAliveInterval.TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = true
            };
            keepAliveTimer.Elapsed += (sender, e) =>
            {
                try
                {
                    ws.Send("ping");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception in keep-alive timer: " + ex.Message);
                    keepAliveTimer.Stop();
                }

            };
           
            
            ws.OnClose = () =>
            {
                StateService.RemoveConnection(ws.ConnectionInfo.Id);
                keepAliveTimer.Stop();
            };

            ws.OnOpen =  () => { StateService.AddConnection(ws.ConnectionInfo.Id, ws); };

            ws.OnMessage = async message =>
            {
                try
                {
                    await app.InvokeClientEventHandler(clientEventHandlers, ws, message);
                }
                catch (Exception e)
                {
                    GlobalExceptionHandler.Handle(e, ws, message);
                }
            };
        });
        
        app.Services.GetService<MQTTClientService>().CommunicateWithBroker();
        return app;
    }
}