using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class Presets : ContentPage
{
	public Presets(PresetsVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
    public static Presets? Current {  get; set; }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PresetsVM vm)
            await vm.InitializeAsync();
    }

    private async void NewPresetFab_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("NewPreset");
    }

}