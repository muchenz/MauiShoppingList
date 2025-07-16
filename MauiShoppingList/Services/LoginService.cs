using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Test_MauiApp1.Models;
using Test_MauiApp1.Models.Response;

namespace Test_MauiApp1.Services;
public class LoginService
{
    private readonly UserService _userService;
    private readonly StateService _stateService;

    public LoginService(UserService userService, StateService stateService)
    {
        _userService = userService;
        _stateService = stateService;
    }


    public bool TryToLogin()
    {
          

        if (Preferences.Default.ContainsKey("UserName") && Preferences.Default.ContainsKey("Token"))
        {
            _stateService.StateInfo.UserName = Preferences.Default.Get("UserName", "");
            _stateService.StateInfo.Token = Preferences.Default.Get("Token", "");

            var a = _stateService.StateInfo.Token;
            var b = _stateService.StateInfo.UserName;

            return true;           
        }
                        
        return false;
    }

    public async Task<MessageAndStatusAndData<UserNameAndTokenResponse>> LoginAsync(string userName, string password)
    {

        MessageAndStatusAndData<UserNameAndTokenResponse> response =
                   await _userService.LoginAsync(userName, password);

        if (response.IsSuccess)
        {
            Preferences.Default.Set("UserName", response.Data.UserName);
            Preferences.Default.Set("Token", response.Data.Token);

            _stateService.StateInfo.UserName = response.Data.UserName;
            _stateService.StateInfo.Token = response.Data.Token;
        }


        return response;

    }

    public  void LoginByTokenAsync(string userName, string token)
    {
        Preferences.Default.Set("UserName", userName);
        Preferences.Default.Set("Token", token);

        _stateService.StateInfo.UserName = userName;
        _stateService.StateInfo.Token = token;
    }
}
