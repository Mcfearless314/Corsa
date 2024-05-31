using Backend.exceptions;
using Backend.service;
using Fleck;
using lib;

namespace Backend.EventFilters;

public class AuthenticationFilter: BaseEventFilter
{
    public override Task Handle<T>(IWebSocketConnection ws, T dto)
    {
        if (!StateService.GetMetaData(ws).IsAuthenticated)
        {
            throw new AuthenticationFailureException("Connection to server lost, log in again");
        }
        else
        {
            return Task.CompletedTask;
        }
    }

  
}