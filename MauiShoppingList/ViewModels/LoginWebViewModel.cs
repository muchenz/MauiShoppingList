using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Resolution;
using Microsoft.Maui;
using Test_MauiApp1.Services;
using Test_MauiApp1.Views;
using Test_MauiApp1.ViewModels;

namespace Test_MauiApp1.ViewModels
{
    public class LoginWebViewModel: BaseViewModel
    {

        string facbookUrl = "";
        public LoginWebViewModel(UserService userService, LoginService loginService)
        {
            WebUrl = "https://www.facebook.com/v10.0/dialog/oauth?client_id=259675572518658"
                    + "&response_type=token"
                    + "&redirect_uri=https://192.168.0.222:5003/api/User/FacebookToken"
                    + "&state=st=state123abc,ds=123456789&scope=public_profile,email";

            _userService = userService;
            _loginService = loginService;
        }




        string _webUrl;
        private readonly UserService _userService;
        private readonly LoginService _loginService;

        public string WebUrl { get { return _webUrl; } set { SetProperty(ref _webUrl, value); } }

        public async Task ObtainedAccessTokenAsync(string accessFacebookToken)
        {
            var response = await _userService.GetTokenFromFacebookAccessToken(accessFacebookToken);

            if (response.IsError == false)
            {

                _loginService.LoginByTokenAsync(response.Data.Email, response.Data.Token);

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
