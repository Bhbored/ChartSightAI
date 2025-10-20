using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChartSightAI.MVVM.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChartSightAI.TestData;

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
        private void AddPreset()
        {
            var p = PresetFaker.CreateOne();
            Items.Insert(0, p);
            SelectedPreset = p;
        }

        [RelayCommand]
        private void DeletePreset(Preset? preset)
        {
            if (preset is null) return;
            Items.Remove(preset);
            if (SelectedPreset == preset) SelectedPreset = Items.FirstOrDefault();
        }

        [RelayCommand]
        private void EditPreset(Preset? preset)
        {
            if (preset is null) return;
            SelectedPreset = preset;
        }
        #endregion

        #region Tasks
        public Task InitializeAsync()
        {
            Items = new ObservableCollection<Preset>(PresetFaker.CreateList(10));
            SelectedPreset = Items.FirstOrDefault();
            return Task.CompletedTask;
        }
        #endregion
    }
}
