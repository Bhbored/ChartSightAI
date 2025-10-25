using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class Settings : ContentPage
{
	public Settings(SettingsVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
		if(BindingContext is SettingsVM vM)
            vM.InitializeAsync();
    }

    
}