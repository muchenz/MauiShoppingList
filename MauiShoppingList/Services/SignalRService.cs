using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MauiApp1.Services;
internal class SignalRService
{
    private HubConnection? _hubConnection;
    private int _userId;
    public async Task StartConnectionAsync()
    {
    }

    public async Task StopConnectionAsync()
    {

    }
    public IDisposable RegisterSignalRHandlers(Func<Task> func)
    {
        return _hubConnection?.On("InvitationAreChaned_" + _userId, async () =>
        {
            await func();
        });
    }
}