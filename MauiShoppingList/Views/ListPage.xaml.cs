using Test_MauiApp1.ViewModels;

namespace Test_MauiApp1.Views;

public partial class ListPage : ContentPage
{
	public ListPage(ListViewModel listViewModel)
	{
		InitializeComponent();

        BindingContext = listViewModel;
        listViewModel.Navigation = Navigation;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ((ListViewModel)BindingContext).OnAppearingAsyncCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ((ListViewModel)BindingContext).OnDisappearingAsyncCommand.Execute(null);
    }
}