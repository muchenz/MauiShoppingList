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


    public async Task<bool> TryToLoginAsync()
    {
          

        if (Preferences.Default.ContainsKey("UserName") && Preferences.Default.ContainsKey("Token"))
        {
            var token = Preferences.Default.Get("Token", "");
            var userName = Preferences.Default.Get("UserName", "");


            _stateService.StateInfo.UserName = userName;
            _stateService.StateInfo.Token = token;

            var isVerified = false;
            try
            {
                isVerified = await _userService.VerifyToken();
            }
            catch
            {
                isVerified = false;
            }

          
            if (isVerified is not true)
            {
                Preferences.Default.Remove("Token");
                Preferences.Default.Remove("UserName");

                return false;
            }

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

    //TODO: logout
}
