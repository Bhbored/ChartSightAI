using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class NewPreset : ContentPage
{
    public NewPreset(NewPresetVM vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if(BindingContext is NewPresetVM vm)
            await vm.InitializeAsync();
    }
}