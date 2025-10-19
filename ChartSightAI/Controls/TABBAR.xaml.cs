using System.Diagnostics;
using System.Windows.Input;

namespace ChartSightAI.Controls;

public partial class TABBAR : ContentView
{
	public TABBAR()
	{
        InitializeComponent();
        BindingContext = this;
        NavigateCommand = new Command<string>(async route =>
        {
            if (string.IsNullOrWhiteSpace(route)) return;
            if (Shell.Current is null) return;
            await Shell.Current.GoToAsync(route);
            Debug.WriteLine($"moved to {route.Substring(2)}");
        });
    }

    public static readonly BindableProperty NavigateCommandProperty =
        BindableProperty.Create(nameof(NavigateCommand), typeof(ICommand), typeof(TABBAR));
    public ICommand NavigateCommand
    {
        get => (ICommand)GetValue(NavigateCommandProperty);
        set => SetValue(NavigateCommandProperty, value);
    }
}
