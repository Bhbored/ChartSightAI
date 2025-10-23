using System;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class BaseVM : ObservableObject
    {
        public static Task ShowSnackAsync(ContentPage page, string msg)
        {
            var tcs = new TaskCompletionSource<bool>();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);

                var options = new SnackbarOptions
                {
                    BackgroundColor = Colors.Cyan,
                    TextColor = Colors.White,
                    CornerRadius = new CornerRadius(10),
                };

                var snackbar = Snackbar.Make(
                    message: msg,
                    duration: TimeSpan.FromSeconds(2),
                    visualOptions: options,
                    anchor: page
                );

                await snackbar.Show();
                tcs.SetResult(true);
            });

            return tcs.Task;
        }
        public static Task ShowErrorSnackAsync(ContentPage page, string msg)
        {
            var tcs = new TaskCompletionSource<bool>();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);

                var options = new SnackbarOptions
                {
                    BackgroundColor = Colors.LightGray,
                    TextColor = Colors.White,
                    CornerRadius = new CornerRadius(10),
                };

                var snackbar = Snackbar.Make(
                    message: msg,
                    duration: TimeSpan.FromSeconds(2),
                    visualOptions: options,
                    anchor: page
                );

                await snackbar.Show();
                tcs.SetResult(true);
            });

            return tcs.Task;
        }
    }
}
