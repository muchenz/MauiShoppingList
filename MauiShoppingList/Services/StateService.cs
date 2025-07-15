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
        public string ClientSignalRID { get; set; }


        public  string UserName { get; set; }
        public  User User { get; set; }

    }


}
