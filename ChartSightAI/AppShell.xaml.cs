using ChartSightAI.MVVM.Views;

namespace ChartSightAI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Settings), typeof(Settings));
        }
    }
}
