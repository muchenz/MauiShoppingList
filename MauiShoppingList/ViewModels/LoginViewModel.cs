using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;
using Microsoft.Maui;
using Test_MauiApp1.Models;
using Test_MauiApp1.Services;
using Test_MauiApp1.Views;
using Test_MauiApp1.ViewModels;
using Test_MauiApp1.Models.Response;
using System.Diagnostics;

namespace Test_MauiApp1.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;
        private readonly StateService _stateService;
        private readonly LoginService _loginService;

        public Command LoginCommand { get; set; }

        public LoginModel Model { get; set; }


        public LoginViewModel(UserService userService, IConfiguration configuration, StateService stateService, LoginService loginService, string loginError = null)
        {
            LoginError = loginError;
            _stateService = stateService;
            _loginService = loginService;
            _userService = userService;
            _configuration = configuration;
            LoginCommand = new Command(async () => await Login());

            base.InitAsyncCommand.Execute(null);

            Model = new LoginModel();
            if (Preferences.Default.ContainsKey("UserName"))
                Model.UserName = Preferences.Default.Get("UserName", "");
            if (Preferences.Default.ContainsKey("Password"))
                Model.Password = Preferences.Default.Get("Password", "");
            Model.PropertyChanged += LoginViewModel_PropertyChanged;

        }


        protected override async Task OnAppearingAsync()
        {

        }

        protected override async Task InitAsync()
        {


        }

        private void LoginViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UserName")
                Preferences.Default.Set("UserName", Model.UserName);
            if (e.PropertyName == "Password")
                Preferences.Default.Set("Password", Model.Password);

        }
        private string _loginError;

        public string LoginError
        {
            get { return _loginError; }
            set { SetProperty(ref _loginError, value); }
        }

        public async Task Login()
        {
            if (!String.IsNullOrEmpty(Model.Password) && !String.IsNullOrEmpty(Model.UserName))
            {
                LoginError = null;

                MessageAndStatusAndData<UserNameAndTokensResponse> response =
                    await _loginService.LoginAsync(Model.UserName, Model.Password);


                if (!response.IsError)
                {
                    App.Current.MainPage = new NavigationPage(App.Container.Resolve<ListAggregationPage>())
                    {
                        BarBackgroundColor = Colors.WhiteSmoke,
                        BarTextColor = Colors.Black //color of arrow in ToolbarItem
                    };

                    //await Navigation.PushAsync(App.Container.Resolve<ListAggregationPage>());
                    // Navigation.RemovePage(Navigation.NavigationStack[^2]);
                }
                else
                {

                    LoginError = response.Message;
                }
            }

        }

        public ICommand CreateAccountCommand
        {
            get
            {
                return new Command(async (list) =>
                {

                    await Navigation.PushAsync(App.Container.Resolve<RegistrationPage>());



                });

            }
        }

        public ICommand LoginFacebookCommand
        {
            get
            {
                return new Command(async (list) =>
                {


                    bool useBrowserForFacebokLogin = true;

                    if (useBrowserForFacebokLogin)
                    {
                        //------------------------ use browser ------------------------------------------------

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


                        string WebUrl = string.Format("https://www.facebook.com/v10.0/dialog/oauth?client_id={0}&response_type=code&redirect_uri={1}&state={2}&scope={3}",
                         259675572518658,
                            "https://192.168.0.222:5003/api/User",
                         $"st=state123abc,ds=123456789, di={_gid}, returnUrl=fb259675572518658://authorize", "public_profile,email");

                        Trace.WriteLine($"Facebook login id:!!!!!!!!!!!!!!!!!!!!!!!");


                        var res = await WebAuthenticator.Default.AuthenticateAsync(
                             new WebAuthenticatorOptions
                             {
                                 Url = new Uri(WebUrl),
                                 CallbackUrl = new Uri("fb259675572518658://authorize"),
                                 PrefersEphemeralWebBrowserSession = true
                             });


                        var isId = res.Properties.TryGetValue("id", out string id);

                        var response = await _userService.GetTokensFromId(id);

                        if (response.IsError == false)
                        {

                            _loginService.SetCredentials(response.Data.UserName, response.Data.Token, response.Data.RefreshToken);

                            App.Current.MainPage = new NavigationPage(App.Container.Resolve<ListAggregationPage>())
                            {
                                BarBackgroundColor = Colors.WhiteSmoke,
                                BarTextColor = Colors.Black
                            };

                            //await Shell.Current.GoToAsync("//MainPage");
                        }
                        else
                        {
                            LoginError = response.Message;

                        }


                        Trace.WriteLine($"Facebook login id: {id}");


                    }
                    else
                    {

                        //------------------------ use web view ------------------------------------------------

                        await Navigation.PushAsync(App.Container.Resolve<LoginWebPage>());
                    }


                });

            }
        }

    }
}
