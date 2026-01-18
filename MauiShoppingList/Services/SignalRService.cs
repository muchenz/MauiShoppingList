using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Test_MauiApp1.Models;

namespace Test_MauiApp1.Services;
public class SignalRService
{
    public SignalRService(IConfiguration configuration, StateService stateService, TokenClientService tokenClientService)
    {
        _configuration = configuration;
        _stateService = stateService;
        _tokenClientService = tokenClientService;
    }
    private HubConnection? _hubConnection;
    private int _userId;
    private readonly IConfiguration _configuration;
    private readonly StateService _stateService;
    private readonly TokenClientService _tokenClientService;

    public Task OnlyStop()
    {
        return _hubConnection?.StopAsync();
    }
    public Task OnlyStart()
    {
        return _hubConnection?.StartAsync();
    }

    public async Task StartConnectionAsync()
    {
        if (string.IsNullOrEmpty(_stateService.StateInfo.Token))
        {
            return;
        }
        _userId = _stateService.StateInfo.User.UserId;

        var signalRAddress = _configuration.GetSection("AppSettings")["SignlRAddress"];
        _hubConnection = new HubConnectionBuilder().WithUrl(signalRAddress, (opts) =>
        {
            //opts.Headers.Add("Access_Token", _stateService.StateInfo.Token);

            opts.AccessTokenProvider = async () =>
            {
                await _tokenClientService.CheckAndSetNewTokens();

                return _stateService.StateInfo.Token;
            };

            opts.HttpMessageHandlerFactory = (message) =>
            {
                if (message is HttpClientHandler clientHandler)
                    // bypass SSL certificate
                    clientHandler.ServerCertificateCustomValidationCallback +=
                        (sender, certificate, chain, sslPolicyErrors) => { return true; };
                return message;
            };
        }).WithAutomaticReconnect().Build();


        await _hubConnection.StartAsync();
        _stateService.StateInfo.ClientSignalRID = _hubConnection.ConnectionId;
        await CallHuBReadyAsync();


        _hubConnection.Reconnected += (connectionId) =>
        {
            _stateService.StateInfo.ClientSignalRID = connectionId;
            return Task.CompletedTask;
        };

    }

    public async Task StopConnectionAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }
    public IDisposable RegisterInvitationAreChanedHandlers(Func<Task> func)
    {
        return _hubConnection?.On("InvitationAreChanged_" + _userId, async () =>
        {
            await func();
        });
    }
    //-----------------------------------------------

    public IDisposable RegisterDataAreChangedHandlers(Func<Task> func)
    {
        return _hubConnection?.On("DataAreChanged_" + _userId, async () =>
        {
            await func();
        });
    }

    public IDisposable RegisterListItemAreChangedHandlers(Func<SignaREnvelope, Task> func)
    {
        return _hubConnection?.On("ListItemAreChanged_" + _userId, async (string signaREnvelope) =>
        {
            var envelope = JsonSerializer.Deserialize<SignaREnvelope>(signaREnvelope);

            await func(envelope);
        });
    }



    //--------------
    event Action HuBReady;
    event Func<Task> HuBReadyAsync;

    public async Task CallHuBReadyAsync()
    {


        foreach (var handler in HuBReadyAsync?.GetInvocationList() ?? Array.Empty<Delegate>())
        {
            if (handler is Func<Task> callback)
            {
                try
                {
                    await callback();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error in signar registration: {ex.Message}");
                }

                HuBReadyAsync -= callback;
            }
        }


        foreach (var handler in HuBReady?.GetInvocationList() ?? Array.Empty<Delegate>())
        {
            if (handler is Action callback)
            {
                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error in signar registration: {ex.Message}");

                }

                HuBReady -= callback;
            }
        }
    }

    public void JoinToHub(Action func)
    {

        if (_hubConnection == null)
        {
            HuBReady += func;
        }
        else
            func();
    }

    public void JoinToHub(Func<Task> func)
    {

        if (_hubConnection == null)
        {
            HuBReadyAsync += func;
        }
        else
            func();
    }
}