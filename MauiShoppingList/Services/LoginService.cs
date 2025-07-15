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


    public async Task<MessageAndStatusAndData<UserNameAndTokenResponse>> LoginAsync(string userName, string password)
    {

        MessageAndStatusAndData<UserNameAndTokenResponse> response =
                   await _userService.LoginAsync(userName, password);

        if (response.IsSuccess)
        {
            Preferences.Default.Set("Token", response.Data.Token);
            Preferences.Default.Set("UserName", response.Data.UserName);

            _stateService.StateInfo.UserName = response.Data.Token;
            _stateService.StateInfo.Token = response.Data.Token;
        }


        return response;

    }
}
