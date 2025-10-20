using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class Presets : ContentPage
{
	public Presets(PresetsVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PresetsVM vm)
            await vm.InitializeAsync();
    }
}