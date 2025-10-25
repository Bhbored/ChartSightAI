using ChartSightAI.Popups;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Preferences = ChartSightAI.MVVM.Models.Preferences;
namespace ChartSightAI.MVVM.ViewModels
{
    public partial class SettingsVM : ObservableObject
    {
        #region Properties
        [ObservableProperty] private Preferences _prefs;//wire later for backend
        [ObservableProperty] private string _name="";
        [ObservableProperty] private string _email="";
        #endregion

        #region Commands
        [RelayCommand]
        private async Task EditUserNameAsync()
        {
            var name = Name;
            await Shell.Current.ShowPopupAsync(new EditUserNamePopup(this, name));      
        }
        #endregion

        #region Methods
        public Task InitializeAsync()
        {
           
            return Task.CompletedTask;
        }
        #endregion
    }
}
