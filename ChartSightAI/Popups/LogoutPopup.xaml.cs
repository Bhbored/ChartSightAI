using ChartSightAI.MVVM.ViewModels;
using CommunityToolkit.Maui.Views;

namespace ChartSightAI.Popups;

public partial class LogoutPopup : Popup
{
	private readonly SettingsVM _settingsVM;
	public LogoutPopup(SettingsVM vm)
	{
		InitializeComponent();
		_settingsVM = vm;
	}
    private async void Logout_Clicked(object sender, EventArgs e)
    {
        await _settingsVM.LogoutAsync();
    }

    private void Cancel_Clicked(object sender, EventArgs e)
    {
        CloseAsync();
    }
}