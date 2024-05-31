using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.EventFilters;
using Backend.exceptions;
using Backend.service;
using Fleck;
using lib;

namespace Backend.ClientEventHandlers;

public class ClientWantsToRegisterDto : BaseDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [MinLength(3)]
    public string Username { get; set; }
    
    [Required]
    [MinLength(8)]
    [MaxLength(32)]
    public string Password { get; set; }

}

//[RateLimiter(2,30,1,1)]
public class ClientWantsToRegister : BaseEventHandler<ClientWantsToRegisterDto>
{
    private AccountService _accountService;

    public ClientWantsToRegister(AccountService accountService)
    {
        _accountService = accountService;
    }

    public override async Task Handle(ClientWantsToRegisterDto dto, IWebSocketConnection socket)
    {
        await _accountService.CheckIfUserExists(dto.Username, dto.Email);
        
        var userId = _accountService.Register(dto.Username, dto.Email, dto.Password);
        if (userId <= 0)
        {
            var response = new ServerDeniesRegistration()
            {
                Message = "Registration failed"
            };
            await socket.Send(JsonSerializer.Serialize(response));
        }
        else
        {
            StateService.AuthenticateUser(socket, userId);
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

public class ServerDeniesRegistration : BaseDto
{
    public string Message { get; set; }
}