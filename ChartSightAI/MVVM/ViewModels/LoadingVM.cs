// MVVM/ViewModels/LoadingVM.cs
using System;
using System.Threading.Tasks;
using ChartSightAI.MVVM.Views;
using ChartSightAI.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class LoadingVM : ObservableObject
    {
        private readonly AuthService _auth;
        [ObservableProperty] private bool isBusy = true;

        public LoadingVM(AuthService auth) => _auth = auth;

        public async Task RunAsync()
        {
            try
            {
                IsBusy = true;

                var session = await _auth.EnsureSessionAsync();

                if (session != null && !string.IsNullOrWhiteSpace(session.AccessToken))
                {
                    await SafeNavigateAsync($"//{nameof(HomePage)}");
                }
                else
                {
                    await SafeNavigateAsync($"//{nameof(LoginPage)}");
                }
            }
            catch (Exception ex)
            {
                // If anything goes wrong, send to Login, and optionally inform the user
                await SafeAlertAsync("Startup error", ex.Message);
                await SafeNavigateAsync($"//{nameof(LoginPage)}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static Task SafeNavigateAsync(string route)
        {
            var shell = Shell.Current;
            if (shell == null) return Task.CompletedTask;

            if (shell.Dispatcher.IsDispatchRequired)
                return shell.Dispatcher.DispatchAsync(() => shell.GoToAsync(route));

            return shell.GoToAsync(route);
        }

        private static Task SafeAlertAsync(string title, string message)
        {
            var shell = Shell.Current;
            if (shell == null) return Task.CompletedTask;

            if (shell.Dispatcher.IsDispatchRequired)
                return shell.Dispatcher.DispatchAsync(() => shell.DisplayAlert(title, message, "OK"));

            return shell.DisplayAlert(title, message, "OK");
        }
    }
}
