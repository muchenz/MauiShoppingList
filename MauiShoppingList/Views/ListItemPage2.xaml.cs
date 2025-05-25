using Test_MauiApp1.ViewModels;

namespace Test_MauiApp1.Views;

public partial class ListItemPage2 : ContentPage
{
	public ListItemPage2(ListItemViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        viewModel.Navigation = Navigation;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ((ListItemViewModel)BindingContext).OnAppearingAsyncCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ((ListItemViewModel)BindingContext).OnDisappearingAsyncCommand.Execute(null);
    }

    private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
    {

    }

   

    private void TapGestureRecognizer_DoubleTapped(object sender, TappedEventArgs e)
    {
        ((ListItemViewModel)BindingContext).ItemDoubleClickedCommand.Execute(null);
    }
}