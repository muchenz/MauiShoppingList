//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Unity;
//using Microsoft.Maui;
//using Test_MauiApp1.Models;
//using Test_MauiApp1.Services;
//using Test_MauiApp1.ViewModels;

//namespace Test_MauiApp1.Views
//{
//    //[XamlCompilation(XamlCompilationOptions.Compile)]
//    public partial class LoginPage2 : ContentPage
//    {

//        public LoginPage(LoginViewModel loginViewModel)
//        {
//            InitializeComponent();

//            BindingContext = loginViewModel;
//            loginViewModel.Navigation = Navigation;
//        }

//        protected override void OnAppearing()
//        {
//            base.OnAppearing();

//            //if (Application.Current.Properties.ContainsKey("UserName"))
//            //    ((LoginViewModel)BindingContext).Model.UserName = Application.Current.Properties["UserName"].ToString();
//            //if (Application.Current.Properties.ContainsKey("Password"))
//            //    ((LoginViewModel)BindingContext).Model.Password = Application.Current.Properties["Password"].ToString();
//        }
//    }
//}