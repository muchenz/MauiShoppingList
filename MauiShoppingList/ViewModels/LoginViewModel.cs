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

namespace Test_MauiApp1.ViewModels
{
    public class LoginViewModel: BaseViewModel
    {        
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;
        private readonly StateService _stateService;
        private readonly LoginService _loginService;

        public Command LoginCommand { get; set; }

        public LoginModel Model { get; set; } 

        
        public LoginViewModel(UserService userService, IConfiguration configuration, StateService stateService, LoginService loginService,  string loginError=null)
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
                Model.UserName = Preferences.Default.Get("UserName","");
            if (Preferences.Default.ContainsKey("Password"))
                Model.Password = Preferences.Default.Get("Password","");
            Model.PropertyChanged += LoginViewModel_PropertyChanged;
        }

        protected override Task InitAsync()
        {




            return base.InitAsync();
        }

        private void LoginViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName== "UserName")
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
                
                MessageAndStatusAndData<UserNameAndTokenResponse> response = 
                    await _loginService.LoginAsync(Model.UserName, Model.Password);
                    

                if (!response.IsError)
                {
                    await Navigation.PushAsync(App.Container.Resolve<ListAggregationPage>());
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
                return new Command(async (list) => {

                    await Navigation.PushAsync(App.Container.Resolve<RegistrationPage>());



                });

            }
        }
        
        public ICommand LoginFacebookCommand
        {
            get
            {
                return new Command(async (list) => {


                    await  Navigation.PushAsync(App.Container.Resolve<LoginWebPage>());

                });

            }
        }

    }
}
