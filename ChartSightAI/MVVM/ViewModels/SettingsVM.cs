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
        [ObservableProperty] private bool _isDeleteSheetOpen = false;
        #endregion

        #region Commands
        [RelayCommand]
        private async Task EditUserNameAsync()
        {
            var name = Name;
            await Shell.Current.ShowPopupAsync(new EditUserNamePopup(this, name));   
            
        }
        [RelayCommand]
        private void OpenBottomsheet()
        {
            IsDeleteSheetOpen = true;

        }
        [RelayCommand]
        private void CloseDeleteSheet()
        {
            IsDeleteSheetOpen = false;

        }
        [RelayCommand]
        private async Task DeleteAllData()
        {
            

            var confirm = await Shell.Current.DisplayAlert(
                "                   ⚠️ Delete local data",
                "❗Are you sure you want to remove all local analytics, presets, and cached AI results from this device? This cannot be undone.",
                "Delete",
                "Cancel");

            if (confirm)
            {
                // TODO: perform the actual deletion of local data here
                CloseDeleteSheet();
            }
        }
        #endregion

        #region Methods
        public Task InitializeAsync()
        {
           IsDeleteSheetOpen = false;
            return Task.CompletedTask;
        }
        #endregion
    }
}
