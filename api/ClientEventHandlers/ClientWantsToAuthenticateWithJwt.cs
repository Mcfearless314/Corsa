using System.Text.Json;
using System.Threading.Tasks;
using Backend.infrastructure.dataModels;
using Backend.service;
using Fleck;
using lib;


namespace Backend.ClientEventHandlers;

public class ClientWantsToAuthenticateWithJwtDto : BaseDto
{
    public string token { get; set; }
}

public class ClientWantsToAuthenticateWithJwt : BaseEventHandler<ClientWantsToAuthenticateWithJwtDto>
{
    private JwtService _jwtService;
    
    public ClientWantsToAuthenticateWithJwt(JwtService jwtService)
    {
        _jwtService = jwtService;
    }


    public override Task Handle(ClientWantsToAuthenticateWithJwtDto dto, IWebSocketConnection socket)
    {
        var sessionData = _jwtService.ValidateAndDecodeToken(dto.token);

        // Send the token to the client
        socket.Send(JsonSerializer.Serialize(new ResponseDto
        {
            MessageToClient = "Successfully authenticated",
            ResponseData = new { sessionData }
        }));
        return Task.CompletedTask;
    }
}