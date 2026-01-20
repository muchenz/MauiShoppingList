using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Test_MauiApp1.Services;
using Test_MauiApp1.ViewModels;
using Test_MauiApp1.Views;
using Unity;
using Unity.Resolution;

namespace Test_MauiApp1.ViewModels
{
    public class LoginWebViewModel: BaseViewModel
    {

        string facbookUrl = "";
        public LoginWebViewModel(UserService userService, LoginService loginService, StateService stateService)
        {
            var gid = stateService.StateInfo.Gid;


            WebUrl = "https://www.facebook.com/v10.0/dialog/oauth?client_id=259675572518658"
                    + "&response_type=token"
                    + "&redirect_uri=https://192.168.0.222:5003/api/User/FacebookToken"
                    + $"&state=st=state123abc,ds=123456789,di={gid}&scope=public_profile,email";

            _userService = userService;
            _loginService = loginService;
            _stateService = stateService;
        }

        string _webUrl;
        private readonly UserService _userService;
        private readonly LoginService _loginService;
        private readonly StateService _stateService;

        public string WebUrl { get { return _webUrl; } set { SetProperty(ref _webUrl, value); } }

        public async Task ObtainedAccessTokenAsync(string accessFacebookToken, string state )
        {
            var response = await _userService.GetTokenFromFacebookAccessToken(accessFacebookToken, state);

            if (response.IsError == false)
            {

                _loginService.SetCredentials(response.Data.UserName, response.Data.Token, response.Data.RefreshToken);


                App.Current.MainPage = new NavigationPage(App.Container.Resolve<ListAggregationPage>())
                {
                    BarBackgroundColor = Colors.WhiteSmoke,
                    BarTextColor = Colors.Black //color of arrow in ToolbarItem
                };

                //await Navigation.PushAsync(App.Container.Resolve<ListAggregationPage>());
                //if (Navigation.NavigationStack.Count > 1)
                //{
                //    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                //}

            }
            else
            {
                ((LoginViewModel)Navigation.NavigationStack[0].BindingContext).LoginError = response.Message;
                await Navigation.PopAsync();
               
            }
            

        }

    }
}
