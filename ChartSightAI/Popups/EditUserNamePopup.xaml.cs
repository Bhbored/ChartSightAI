using ChartSightAI.MVVM.ViewModels;
using ChartSightAI.MVVM.Views;
using CommunityToolkit.Maui.Views;

namespace ChartSightAI.Popups;

public partial class EditUserNamePopup : Popup
{
    private readonly SettingsVM _settingsVM;
    public string UserName { get; set; }

    public EditUserNamePopup(SettingsVM vm, string name)
    {
        InitializeComponent();
        BindingContext = vm;
        UserName = name;
        _settingsVM = vm;
    }
    void Cancel_Clicked(object sender, EventArgs e) => CloseAsync();

    void Save_Clicked(object sender, EventArgs e)
    {
        _settingsVM.Name = NameEntry.Text;
        CloseAsync();
    }
}
