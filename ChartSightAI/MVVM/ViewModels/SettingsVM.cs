using ChartSightAI.MVVM.Models;
using ChartSightAI.MVVM.Views;
using ChartSightAI.Popups;
using ChartSightAI.Services;
using ChartSightAI.Services.Repos;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class SettingsVM : ObservableObject
    {
        private readonly AuthService _auth;
        private readonly AnalysisSessionRepo _sessionsRepo;
        private readonly PresetRepo _presetRepo;
        private readonly UserPreferenceRepo _preferencesRepo;

        public SettingsVM(AuthService auth, AnalysisSessionRepo sessionsRepo, PresetRepo presetRepo, UserPreferenceRepo preferencesRepo)
        {
            _auth = auth;
            _sessionsRepo = sessionsRepo;
            _presetRepo = presetRepo;
            _preferencesRepo = preferencesRepo;
        }

        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _email = "";
        [ObservableProperty] private bool isDeleteSheetOpen;
        [ObservableProperty] private bool isBusy;

        public async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;
                await _auth.InitializeAsync();
                Email = await _auth.GetUserEmailAsync() ?? "Unknown";
                var uid = await _auth.GetUserIdAsync();
                if (uid is null) return;
                var pref = await _preferencesRepo.GetAsync(uid.Value);
                UserName = pref?.UserName ?? "";
            }
            finally
            {
                IsBusy = false;
            }
        }
        public ICommand LogoutCommand => new Command(async () => await Shell.Current.ShowPopupAsync(new LogoutPopup(this)));
        [RelayCommand]
        private void OpenDeleteSheet() => IsDeleteSheetOpen = true;

        [RelayCommand]
        private void CloseDeleteSheet() => IsDeleteSheetOpen = false;

        [RelayCommand]
        private async Task EditUserName()
        {
            await Shell.Current.ShowPopupAsync(new EditUserNamePopup(this, UserName));
        }

        public async Task SaveUserNameAsync(string name)
        {
            var uid = await _auth.GetUserIdAsync();
            await _preferencesRepo.UpsertUserNameAsync(uid.Value, name?.Trim() ?? "");
            await Shell.Current.DisplayAlert("Saved", "Preferences updated.", "OK");
        }

        [RelayCommand]
        private async Task DeleteAllData()
        {
            var confirm = await Shell.Current.DisplayAlert("Delete data", "Delete all your sessions, presets and local files on this device? Your account stays active.", "Delete", "Cancel");
            if (!confirm) return;

            try
            {
                await _auth.InitializeAsync();
                var uid = await _auth.GetUserIdAsync();
                if (uid is null)
                {
                    await Shell.Current.DisplayAlert("Not signed in", "Please log in to delete your data.", "OK");
                    return;
                }

                var sessions = await _sessionsRepo.GetByDateRange(uid.Value, from: null, to: null, limit: 10000);
                var presets = await _presetRepo.GetAllAsync(uid.Value);

                await Task.WhenAll(
                    Task.WhenAll(sessions.Select(s => _sessionsRepo.DeleteAsync(uid.Value, s.Id))),
                    Task.WhenAll(presets.Select(p => _presetRepo.DeleteAsync(uid.Value, p.Id))),
                    _preferencesRepo.UpdateUserNameAsync(uid.Value, "")
                );

                try
                {
                    var dir = FileSystem.AppDataDirectory;
                    foreach (var file in Directory.EnumerateFiles(dir))
                    {
                        var name = Path.GetFileName(file).ToLowerInvariant();
                        if (name.StartsWith("chart_") || name.EndsWith(".png") || name.EndsWith(".jpg") || name.EndsWith(".jpeg"))
                        {
                            try { File.Delete(file); } catch { }
                        }
                    }
                }
                catch { }

                await Shell.Current.DisplayAlert("Done", "Your data was deleted.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Delete failed", ex.Message, "OK");
            }
            finally
            {
                CloseDeleteSheet();
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                IsBusy = true;
                await _auth.Logout();
                await Shell.Current.GoToAsync($"//{(nameof(LoginPage))}", true);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Logout Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
