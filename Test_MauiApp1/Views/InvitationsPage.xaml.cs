using Test_MauiApp1.ViewModels;

namespace Test_MauiApp1.Views;

public partial class InvitationsPage : ContentPage
{
    public InvitationsPage(InvitationsViewModel invitationsViewModel)
    {
        InitializeComponent();
        BindingContext = invitationsViewModel;
        invitationsViewModel.Navigation = Navigation;
    }
}