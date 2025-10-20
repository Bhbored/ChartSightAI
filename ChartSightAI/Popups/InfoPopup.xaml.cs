using CommunityToolkit.Maui.Views;

namespace ChartSightAI.Popups;

public partial class InfoPopup : Popup
{
    public InfoPopup(string message)
    {
        InitializeComponent();
        MessageLabel.Text = message;
    }
    private void Close_Clicked(object sender, EventArgs e) => CloseAsync();


}