using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class Profile : ContentPage
{
	public Profile(SettingsVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}