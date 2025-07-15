using Test_MauiApp1.ViewModels;

namespace Test_MauiApp1.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel loginViewModel)
	{
		InitializeComponent();
        BindingContext = loginViewModel;
        loginViewModel.Navigation = Navigation;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((LoginViewModel)BindingContext).OnAppearingAsyncCommand.Execute(null);


        if (Preferences.Default.ContainsKey("UserName"))
            ((LoginViewModel)BindingContext).Model.UserName = Preferences.Default.Get("UserName","");
        if (Preferences.Default.ContainsKey("Password"))
            ((LoginViewModel)BindingContext).Model.Password = Preferences.Default.Get("Password","");


    }

   

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ((LoginViewModel)BindingContext).OnDisappearingAsyncCommand.Execute(null);
    }
}
