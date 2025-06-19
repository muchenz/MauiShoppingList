using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Test_MauiApp1.ViewModels
{
    public class RegistrationViewModel: BaseViewModel
    {
        private readonly UserService _userService;
        private readonly StateService _stateService;

        public ICommand RegistrationCommand { get; set; }

        public RegistrationModel Model { get; set; } = new RegistrationModel {  UserName = "", Password = "", PasswordConfirm="" };
      
        //public RegistrationModelError ModelError { get; set; } 

        public RegistrationViewModel(UserService userService, StateService stateService)
        {
            _userService = userService;
            _stateService = stateService;
            RegistrationCommand = new Command(async () => await Registration(), ()=>Model.IsValid);

            //ModelError = new RegistrationModelError(Model);
            
        }

        string _registrationError;
        public string RegistrationError {

            get { return _registrationError; }
            set
            {
                //_registrationError = value;
                 SetProperty(ref _registrationError, value);
                //OnPropertyChanged("RegistrationError");
            }
        }
        public async Task Registration()
        {
            MessageAndStatusAndData<string> response  = null;
            RegistrationError = "";
            if (!Model.IsValid)
            {
                RegistrationError = "Form is invalid.";
                return;
            }
            try
            {
                response = await _userService.RegisterAsync(Model);

                if (response.IsError)
                {
                    RegistrationError = response.Message;

                    return;
                }
                else
                {
                    _stateService.StateInfo.UserName = Model.UserName;

                    _stateService.StateInfo.Token = response.Data;


                    Preferences.Default.Set("UserName", Model.UserName);

                    Preferences.Default.Set("Password", Model.Password);

                    Navigation.RemovePage(Navigation.NavigationStack.Last());
                    await Navigation.PushAsync(App.Container.Resolve<ListAggregationPage>());

                    


                }
            }
            catch (Exception ex)
            {
                RegistrationError = "Some error,  try again.";
            }

            

        }

        public ICommand CreateAccountCommand
        {
            get
            {
                return new Command(async (list) => {

                    //await Navigation.PushAsync(App.Container.Resolve<ListItemPage2>());



                });

            }
        }

    }
}
