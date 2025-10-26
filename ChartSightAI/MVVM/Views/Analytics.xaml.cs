using ChartSightAI.MVVM.ViewModels;
using ChartSightAI.Utility;

namespace ChartSightAI.MVVM.Views;

public partial class Analytics : ContentPage
{
	readonly AnalyticsVM _vm;

    public Analytics(AnalyticsVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
		_vm = vm;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Unsubscribe();
    }
}