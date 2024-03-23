using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Security.Permissions;
using Test_MauiApp1.Models;
using Test_MauiApp1.Services;
using Test_MauiApp1.ViewModels;
using Test_MauiApp1.Views;
using Unity;

namespace Test_MauiApp1;

public partial class App : Application
{
   
        public static UnityContainer Container { get; set; }
        public static string Token { get; set; }
        //public static string SinalRId { get; set; }
        public static string FacebookToken { get; set; }
        public static string UserName { get; set; }
        //public static string Password { get; set; }
        public static User User { get; set; }
        public static ObservableCollection<ListAggregator> Data { get; set; }
        public App()
        {


             InitializeComponent();

            InitContainer();
            InitMessage();

            MainPage = new NavigationPage(App.Container.Resolve<LoginPage>())
            {
                BarBackgroundColor = Colors.WhiteSmoke,
                BarTextColor = Colors.Black //color of arrow in ToolbarItem
            };

            App.MMainPage = (NavigationPage)MainPage;
        }

        public static NavigationPage MMainPage { get; set; }


        private void InitContainer()
        {
        App.Container = new UnityContainer().EnableDiagnostic();


            //App.Container.RegisterType<IDataStore<Item>, MockDataStore>();

            App.Container.RegisterType<LoginPage>();
            App.Container.RegisterType<LoginViewModel>();
            //App.Container.RegisterType<LoginWebViewModel>();
            App.Container.RegisterType<ListAggregationPage>();
            App.Container.RegisterType<ListAggregationViewModel>();
            App.Container.RegisterType<ListItemPage>();
            App.Container.RegisterType<ListViewModel>();
        App.Container.RegisterType<ListPage>();
        App.Container.RegisterType<ListViewModel>();
        //App.Container.RegisterType<RegistrationPage>();
        //App.Container.RegisterType<RegistrationViewModel>();
        //App.Container.RegisterType<PermissionsPage>();
        //App.Container.RegisterType<PermissionsViewModel>();

        App.Container.RegisterType<HttpClient>();
            App.Container.RegisterType<UserService>();
            App.Container.RegisterType<ListItemService>();
            App.Container.RegisterSingleton<IConfiguration, Test_MauiApp1.Helpers.Configuration.Configuration>();


        }

        private void InitMessage()
        {

        WeakReferenceMessenger.Default.Register<DisplayAlertMessageMessage>(this, async (r, m) =>
        {
            DisplayAlertMessage message = m.Value;

            var result = true;
            if (!string.IsNullOrEmpty(message.Accept))
                result = await this.MainPage.DisplayAlert(message.Title, message.Message, message.Accept, message.Cancel);
            else
            {
                await this.MainPage.DisplayAlert(message.Title, message.Message, message.Cancel);
            }

            if (message.OnCompleted != null)
                message.OnCompleted(result);

        });


        MessagingCenter.Subscribe<Application, DisplayAlertMessage>(this, "ShowAlert", async (sender, message) =>
            {

                var result = true;
                if (!string.IsNullOrEmpty(message.Accept))
                    result = await this.MainPage.DisplayAlert(message.Title, message.Message, message.Accept, message.Cancel);
                else
                {
                    await this.MainPage.DisplayAlert(message.Title, message.Message, message.Cancel);
                }

                if (message.OnCompleted != null)
                    message.OnCompleted(result);

            }, Application.Current);

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
