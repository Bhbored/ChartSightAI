using ChartSightAI.MVVM.ViewModels;
using CommunityToolkit.Maui.Views;
using System.Windows.Input;

namespace ChartSightAI.Popups;

public partial class RatePopup : Popup
{
    private readonly HistoryVM _historyVM;
    public RatePopup(HistoryVM vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _historyVM = vm;
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        _historyVM.Hit();
        CloseAsync();
    }

    private void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        _historyVM.Miss();
        CloseAsync();
    }
}
