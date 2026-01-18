using Test_MauiApp1.ViewModels;

namespace Test_MauiApp1.Views;

public partial class ListAggregationPage : ContentPage
{
	public ListAggregationPage(ListAggregationViewModel listAggregationViewModel)
	{
		InitializeComponent();

        BindingContext = listAggregationViewModel;
        listAggregationViewModel.Navigation = Navigation;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        if (Parent == null)
            DisposeBindingContext();
    }

    protected void DisposeBindingContext()
    {
        if (BindingContext is IDisposable disposableBindingContext)
        {
            disposableBindingContext.Dispose();
            BindingContext = null;
        }
    }
    ~ListAggregationPage()
    {
        DisposeBindingContext();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        var context = (ListAggregationViewModel)BindingContext;

        var l = context.ListAggr;


    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ((ListAggregationViewModel)BindingContext).OnAppearingAsyncCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ((ListAggregationViewModel)BindingContext).OnDisappearingAsyncCommand.Execute(null);
    }
}