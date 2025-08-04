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
    private string _gid = string.Empty;

    public LoginService(StateService stateService, IUnityContainer container)
    {
        _stateService = stateService;
        _container = container;
        if (!Preferences.Default.ContainsKey("gid"))
        {
            _gid = Guid.NewGuid().ToString();
            Preferences.Default.Set("gid", _gid);
        }
        else
        {
            _gid = Preferences.Default.Get("gid", "");
        }

    }


    public async Task<bool> TryToLoginAsync()
    {

        var userService = _container.Resolve<UserService>();

        if (!Preferences.Default.ContainsKey("UserName") || !Preferences.Default.ContainsKey("Token") 
            || !Preferences.Default.ContainsKey("RefreshToken"))
        {
            return false;
        }

        var token = Preferences.Default.Get("Token", "");
        var refreshToken = Preferences.Default.Get("RefreshToken", "");
        var userName = Preferences.Default.Get("UserName", "");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }
        _stateService.StateInfo.UserName = userName;
        _stateService.StateInfo.Token = token;
        _stateService.StateInfo.RefreshToken = refreshToken;

        var isVerified = false;
        try
        {
            //isVerified =  await userService.VerifyToken(); //TODO: VerifyAcceessRefreshTokens
            isVerified =  await userService.VerifyAcceessRefreshTokens(token,refreshToken); // 
        }
        catch
        {
            isVerified = false;
        }


        if (isVerified is not true)
        {
            Preferences.Default.Remove("Token");
            Preferences.Default.Remove("RefreshToken");
            //Preferences.Default.Remove("UserName");

            return false;
        }

        return true;



    }

    public async Task<MessageAndStatusAndData<UserNameAndTokensResponse>> LoginAsync(string userName, string password)
    {
        var userService = _container.Resolve<UserService>();


        MessageAndStatusAndData<UserNameAndTokensResponse> response =
                   await userService.LoginAsync(userName, password);

        if (response.IsSuccess)
        {
            Preferences.Default.Set("UserName", response.Data.UserName);
            Preferences.Default.Set("Token", response.Data.Token);
            Preferences.Default.Set("RefreshToken", response.Data.RefreshToken);

            _stateService.StateInfo.UserName = response.Data.UserName;
            _stateService.StateInfo.Token = response.Data.Token;
            _stateService.StateInfo.RefreshToken = response.Data.RefreshToken;
        }


        return response;

    }

    public void LoginByTokenAsync(string userName, string token, string refreshToken)
    {
        Preferences.Default.Set("UserName", userName);
        Preferences.Default.Set("Token", token);
        Preferences.Default.Set("RefreshToken", refreshToken);

        _stateService.StateInfo.UserName = userName;
        _stateService.StateInfo.Token = token;
        _stateService.StateInfo.Token = refreshToken;
    }


    public async Task LogOutAsync()
    {
        Preferences.Default.Remove("UserName");
        Preferences.Default.Remove("Token");
        Preferences.Default.Remove("RefreshToken");
        Preferences.Default.Remove("Password");
        var userService = _container.Resolve<UserService>();
        await userService.LogOutAsync();

        _stateService.StateInfo.UserName = null;
        _stateService.StateInfo.Token = null;
        _stateService.StateInfo.RefreshToken = null;
    }
}
