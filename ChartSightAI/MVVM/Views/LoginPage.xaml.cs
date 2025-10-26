using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginVM vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}