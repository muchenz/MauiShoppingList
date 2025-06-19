using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
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
        App.Container.RegisterFactory<HttpClient>(c =>
        {
           
            var handler = c.Resolve<AuthHeaderHandler>();

            var baseAddress = configuration.GetSection("AppSettings")["ShoppingWebAPIBaseAddress"];
            var client = new HttpClient(handler)
            {
               
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(30)
            };


            return client;

        }, FactoryLifetime.Singleton);

        App.Container.RegisterType<UserService>();
        App.Container.RegisterType<ListItemService>();

        App.Container.RegisterFactory<IConfiguration>((c) => configuration, FactoryLifetime.Singleton);
        //App.Container.RegisterSingleton<IConfiguration, Helpers.Configuration.Configuration>();

        //var stateService = new StateService();
        App.Container.RegisterFactory<StateService>((_)=> new StateService() , FactoryLifetime.Singleton);
        App.Container.RegisterFactory<IMessenger> ((_)=> new  WeakReferenceMessenger(), FactoryLifetime.Singleton);
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

        });
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

        // var token = await _tokenService.GetAccessTokenAsync(); // lub .GetAccessToken() jeśli synchroniczne

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