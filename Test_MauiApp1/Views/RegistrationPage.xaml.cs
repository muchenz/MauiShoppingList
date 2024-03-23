using Test_MauiApp1.ViewModels;

namespace Test_MauiApp1.Views;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage(RegistrationViewModel registrationViewModel)
	{
		InitializeComponent();
        BindingContext = registrationViewModel;
        registrationViewModel.Navigation = Navigation;
    }
}