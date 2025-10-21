using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ChartSightAI.MVVM.Models;
using ChartSightAI.MVVM.Views;
using ChartSightAI.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class PresetsVM : ObservableObject
    {
        #region Properties
        [ObservableProperty] private ObservableCollection<Preset> items = new();

        [ObservableProperty] private Preset? selectedPreset;
        #endregion

        #region Commands
      

        [RelayCommand]
        private void DeletePreset(Preset? preset)
        {
            if (preset is null) return;
            items.Remove(preset);
            SelectedPreset = PresetStore.Items.FirstOrDefault();
        }

        [RelayCommand]
        private async Task EditPreset(Preset? preset)
        {
            if (preset is null) return;
            PresetStore.TempPreset = preset;
            await Shell.Current.GoToAsync("NewPreset", true);
        }

        #endregion

        #region Tasks
        public Task InitializeAsync()
        {
            var list = PresetStore.Items;
            Items.Clear();
            foreach (var item in list) { 
            Items.Add(item);
            }
            SelectedPreset = Items.FirstOrDefault();
            return Task.CompletedTask;
        }
        #endregion
    }
}
