// MVVM/ViewModels/LoadingVM.cs
using System;
using System.Threading.Tasks;
using ChartSightAI.MVVM.Views;
using ChartSightAI.Services;
using ChartSightAI.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class LoadingVM : ObservableObject
    {
        private readonly AuthService _auth;
        [ObservableProperty] private bool _isBusy = true;

        public LoadingVM(AuthService auth) => _auth = auth;

        public async Task RunAsync()
        {
            try
            {
                IsBusy = true;
                await _auth.InitializeAsync();
                var session = await _auth.GetSession();
                if (session != null && !string.IsNullOrWhiteSpace(session.AccessToken))
                    await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
                else
                    await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
