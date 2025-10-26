using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class LoadingPage : ContentPage
{
    private readonly LoadingVM _vm;
    public LoadingPage(LoadingVM vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.RunAsync();
    }
}