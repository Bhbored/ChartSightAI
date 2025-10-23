using ChartSightAI.MVVM.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.MVVM.ViewModels
{
   public partial class HomeVM : ObservableObject
    {
        public HomeVM() { }


        [RelayCommand]
        public async Task GoToSettings()
        {
            await Shell.Current.GoToAsync(nameof(Settings),true);
        }
        [RelayCommand]
        public async Task GoToHistory()
        {
            await Shell.Current.GoToAsync(nameof(History),true);
        }

    }
}
