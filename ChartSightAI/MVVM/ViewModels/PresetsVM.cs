using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChartSightAI.MVVM.Models;
using ChartSightAI.MVVM.Views;
using ChartSightAI.Services;
using ChartSightAI.Services.Repos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class PresetsVM : ObservableObject
    {
        #region Properties
        [ObservableProperty] private ObservableCollection<Preset> items = new();
        [ObservableProperty] private Preset? selectedPreset;
        #endregion

        private readonly AuthService _auth;
        private readonly PresetRepo _repo;

        public PresetsVM(AuthService auth, PresetRepo presetRepo)
        {
            _auth = auth;
            _repo = presetRepo;
        }

        #region Commands
        [RelayCommand]
        private async Task DeletePreset(Preset? preset)
        {
            if (preset is null) return;

            try
            {
                await _auth.InitializeAsync();
                var uid = await _auth.GetUserIdAsync();
                if (uid is null)
                {
                    await Shell.Current.DisplayAlert("Not signed in", "Please log in to delete presets.", "OK");
                    await Shell.Current.GoToAsync("//LoginPage");
                    return;
                }

                await _repo.DeleteAsync(uid.Value, preset.Id);

                Items.Remove(preset);
                SelectedPreset = Items.FirstOrDefault();

                // Keeping your existing snackbar helper
                await BaseVM.ShowErrorSnackAsync(Presets.Current!, "Preset deleted successfully");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task EditPreset(Preset? preset)
        {
            if (preset is null) return;

            SelectedPreset = preset;

            // Pass the item to the edit page (requires [QueryProperty] on the target VM/Page)
            await Shell.Current.GoToAsync(nameof(NewPreset), true, new Dictionary<string, object>
            {
                { "EditingPreset", preset }
            });
        }
        #endregion

        #region Tasks
        public async Task InitializeAsync()
        {
            try
            {
                Items.Clear();

                await _auth.InitializeAsync();
                var uid = await _auth.GetUserIdAsync();
                if (uid is null)
                {
                    await Shell.Current.DisplayAlert("Not signed in", "Please log in to view presets.", "OK");
                    await Shell.Current.GoToAsync("//LoginPage");
                    return;
                }

                var list = await _repo.GetAllAsync(uid.Value);
                foreach (var item in list)
                    Items.Add(item);

                SelectedPreset = Items.FirstOrDefault();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
        #endregion
    }
}
