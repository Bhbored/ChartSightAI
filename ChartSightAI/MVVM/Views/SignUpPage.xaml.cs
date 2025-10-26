using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class SignUpPage : ContentPage
{
    public SignUpPage(SignUpVM vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}