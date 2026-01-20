using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Security.Permissions;
using Test_MauiApp1.Models;
using Test_MauiApp1.Services;
using Test_MauiApp1.ViewModels;
using Test_MauiApp1.ViewModels.Messages;
using Test_MauiApp1.Views;
using Unity;

namespace Test_MauiApp1;

public partial class App : Application
{

    public static UnityContainer Container { get; set; }
    //public static string Token { get; set; }
    //public static string SinalRId { get; set; }
    //public static string UserName { get; set; }
    //public static User User { get; set; }
    public App()
    {


        InitializeComponent();

        InitContainer();
        InitMessage();
        SetMainPage();


    }

    private void SetMainPage()
    {

        MainPage = new NavigationPage(App.Container.Resolve<LoginPage>())
        {
            BarBackgroundColor = Colors.WhiteSmoke,
            BarTextColor = Colors.Black //color of arrow in ToolbarItem
        };


        Task.Run(async () =>
        {
            var loginService = App.Container.Resolve<LoginService>();
            var isLogged = await loginService.TryToLoginAsync(); ;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (isLogged)
                    MainPage = new NavigationPage(App.Container.Resolve<ListAggregationPage>())
                    {
                        BarBackgroundColor = Colors.WhiteSmoke,
                        BarTextColor = Colors.Black //color of arrow in ToolbarItem
                    };
            });
        });
    }


    private void InitContainer()
    {
        App.Container = new UnityContainer().EnableDiagnostic();

        var configuration = new Helpers.Configuration.Configuration();

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
        App.Container.RegisterType<ListItemViewModel>();
        //App.Container.RegisterType<RegistrationPage>();
        //App.Container.RegisterType<RegistrationViewModel>();
        //App.Container.RegisterType<PermissionsPage>();
        //App.Container.RegisterType<PermissionsViewModel>();

        // App.Container.RegisterType<HttpClient>();

        App.Container.RegisterType<AuthHeaderHandler>();
        App.Container.RegisterSingleton<TokenHttpClient>();
#if ANDROID
        App.Container.RegisterSingleton<IAndroidBackHandler, AndroidBackHandler>();
#endif

        App.Container.RegisterFactory<HttpClient>(c => // some error when using AuthHeaderHandler : DelegatingHandler (setting HttpCompletionOption.ResponseHeadersRead helps)
        {
            //HttpClientHandler handler = new HttpClientHandler();
            //handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            var handler = c.Resolve<AuthHeaderHandler>();

            var baseAddress = configuration.GetSection("AppSettings")["ShoppingWebAPIBaseAddress"];
            var client = new HttpClient(handler)
            {

                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(30)
            };


            return client;

        }, FactoryLifetime.Singleton);


        //App.Container.RegisterFactory<HttpClient>(c =>
        //{
        //    var httpClientHandler = new HttpClientHandler
        //    {
        //        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        //    };

        //    var baseAddress = configuration.GetSection("AppSettings")["ShoppingWebAPIBaseAddress"];
        //    var client = new HttpClient(httpClientHandler)
        //    {
        //        BaseAddress = new Uri(baseAddress),
        //        Timeout = TimeSpan.FromSeconds(30)
        //    };

        //    return client;
        //}, FactoryLifetime.Singleton);


        App.Container.RegisterType<UserService>();
        App.Container.RegisterType<LoginService>();
        App.Container.RegisterType<ListItemService>();

        App.Container.RegisterFactory<IConfiguration>((c) => configuration, FactoryLifetime.Singleton);
        //App.Container.RegisterSingleton<IConfiguration, Helpers.Configuration.Configuration>();

        //var stateService = new StateService();
        App.Container.RegisterFactory<StateService>((c) => new StateService(c.Resolve<IGidService>()), FactoryLifetime.Singleton);
        App.Container.RegisterFactory<IMessenger>((_) => new WeakReferenceMessenger(), FactoryLifetime.Singleton);
        App.Container.RegisterSingleton<SignalRService>();
        App.Container.RegisterSingleton<TokenClientService>();
        App.Container.RegisterSingleton<IGidService, GidService>();

    }

    private void InitMessage()
    {
        var messager = Container.Resolve<IMessenger>();


        //WeakReferenceMessenger.Default.Register<DisplayAlertMessage>(this, async (r, m) =>
        messager.Register<DisplayAlertMessage>(this, async (r, m) =>
        {
            DisplayAlert message = m.Value;

            var result = true;
            if (!string.IsNullOrEmpty(message.Accept))
                result = await this.MainPage.DisplayAlert(message.Title, message.Message, message.Accept, message.Cancel);
            else
            {
                await this.MainPage.DisplayAlert(message.Title, message.Message, message.Cancel);
            }

            if (message.OnCompleted != null)
                message.OnCompleted(result);

            if (message.OnCompletedAsync != null)
                await message.OnCompletedAsync(result);

        });
    }

    protected override void OnStart()
    {
    }

    protected override async void OnSleep()
    {
        var signalR = Container.Resolve<SignalRService>();
        var stateService = Container.Resolve<StateService>();

        if (!string.IsNullOrEmpty(stateService.StateInfo.ClientSignalRID))
        {
            await signalR?.OnlyStop();
        }
    }

    protected override async void OnResume()
    {
        var signalR = Container.Resolve<SignalRService>();
        var messager = Container.Resolve<IMessenger>();
        var stateService = Container.Resolve<StateService>();

        if (!string.IsNullOrEmpty(stateService.StateInfo.Token))
        {
            messager?.Send(new RequestForNewDataMessage());
        }

        if (!string.IsNullOrEmpty(stateService.StateInfo.ClientSignalRID))
        {
            await signalR?.OnlyStart();
        }

    }
}
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly StateService _stateService;

    //private readonly ITokenService _tokenService;

    public AuthHeaderHandler(StateService stateService)
    {
        //_tokenService = tokenService;
        HttpClientHandler handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        InnerHandler = handler;
        _stateService = stateService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        if (!string.IsNullOrEmpty(_stateService.StateInfo.Token))
        {
            var token = _stateService.StateInfo.Token;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (!string.IsNullOrEmpty(_stateService.StateInfo.ClientSignalRID))
        {
            request.Headers.Add("SignalRId", _stateService.StateInfo.ClientSignalRID);
        }

        request.Headers.Add("User-Agent", "BlazorServer");

        return await base.SendAsync(request, cancellationToken);
    }
}