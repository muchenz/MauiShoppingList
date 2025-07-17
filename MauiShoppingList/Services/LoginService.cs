using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Test_MauiApp1.Models;
using Test_MauiApp1.Models.Response;
using Unity;

namespace Test_MauiApp1.Services;
public class LoginService
{
    private readonly StateService _stateService;
    private readonly IUnityContainer _container;

    public LoginService(StateService stateService, IUnityContainer container)
    {
        _stateService = stateService;
        _container = container;
    }


    public async Task<bool> TryToLoginAsync()
    {

        var userService = _container.Resolve<UserService>();

        if (!Preferences.Default.ContainsKey("UserName") || !Preferences.Default.ContainsKey("Token"))
        {
            return false;
        }

        var token = Preferences.Default.Get("Token", "");
        var userName = Preferences.Default.Get("UserName", "");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userName))
        {
            return false;
        }
        _stateService.StateInfo.UserName = userName;
        _stateService.StateInfo.Token = token;

        var isVerified = false;
        try
        {
            isVerified = await userService.VerifyToken();
        }
        catch
        {
            isVerified = false;
        }


        if (isVerified is not true)
        {
            Preferences.Default.Remove("Token");
            //Preferences.Default.Remove("UserName");

            return false;
        }

        return true;



    }

    public async Task<MessageAndStatusAndData<UserNameAndTokenResponse>> LoginAsync(string userName, string password)
    {
        var userService = _container.Resolve<UserService>();


        MessageAndStatusAndData<UserNameAndTokenResponse> response =
                   await userService.LoginAsync(userName, password);

        if (response.IsSuccess)
        {
            Preferences.Default.Set("UserName", response.Data.UserName);
            Preferences.Default.Set("Token", response.Data.Token);

            _stateService.StateInfo.UserName = response.Data.UserName;
            _stateService.StateInfo.Token = response.Data.Token;
        }


        return response;

    }

    public void LoginByTokenAsync(string userName, string token)
    {
        Preferences.Default.Set("UserName", userName);
        Preferences.Default.Set("Token", token);

        _stateService.StateInfo.UserName = userName;
        _stateService.StateInfo.Token = token;
    }

    //TODO: logout

    public void LogOut()
    {
        Preferences.Default.Remove("UserName");
        Preferences.Default.Remove("Token");
        Preferences.Default.Remove("Password");

        _stateService.StateInfo.UserName = null;
        _stateService.StateInfo.Token = null;
    }
}
