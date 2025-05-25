using Test_MauiApp1.ViewModels;

namespace Test_MauiApp1.Views;

public partial class PermissionsPage : ContentPage
{
	public PermissionsPage(PermissionsViewModel permissionsViewModel)
	{
		InitializeComponent();
        BindingContext = permissionsViewModel;
        permissionsViewModel.Navigation = Navigation;
    }
}