using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test_MauiApp1.Models;

namespace Test_MauiApp1.Services
{
    public class StateService
    {
        public  StateInfo StateInfo { get;  } = new StateInfo();

    }


    public class StateInfo
    {
        bool IsAuthenticated { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string ClientSignalRID { get; set; }


        public  string UserName { get; set; }
        public  User User { get; set; }

    }


}

public class GidService : IGidService
{



    public string GetGid()
    {
        string _gid = string.Empty;

        if (!Preferences.Default.ContainsKey("gid"))
        {
            _gid = Guid.NewGuid().ToString();
            Preferences.Default.Set("gid", _gid);
        }
        else
        {
            _gid = Preferences.Default.Get("gid", "");
        }

        return _gid;
    }

}