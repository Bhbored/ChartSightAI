// NewPresetVM.cs
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChartSightAI.MVVM.Models;
using ChartSightAI.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MT = ChartSightAI.MVVM.Models.Enums.MarketType;
using TF = ChartSightAI.MVVM.Models.Enums.TimeFrame;
using TD = ChartSightAI.MVVM.Models.Enums.TradeDirection;
using CommunityToolkit.Maui.Extensions;
using ChartSightAI.Popups;
using ChartSightAI.Services;
using ChartSightAI.Services.Repos;
using Microsoft.Maui.Controls;
using System;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class NewPresetVM : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly PresetRepo _presetRepo;

        public NewPresetVM(AuthService authService, PresetRepo presetRepo)
        {
            _authService = authService;
            _presetRepo = presetRepo;
        }

        #region Properties
        [ObservableProperty] private string presetName = string.Empty;
        [ObservableProperty] private string? description;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsForexSelected))]
        [NotifyPropertyChangedFor(nameof(IsStocksSelected))]
        [NotifyPropertyChangedFor(nameof(IsCryptoSelected))]
        private MT selectedMarketType;
        public bool IsForexSelected => SelectedMarketType == MT.Forex;
        public bool IsStocksSelected => SelectedMarketType == MT.Stocks;
        public bool IsCryptoSelected => SelectedMarketType == MT.Crypto;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLongSelected))]
        [NotifyPropertyChangedFor(nameof(IsShortSelected))]
        private TD selectedTradeDirection;
        public bool IsLongSelected => SelectedTradeDirection == TD.Long;
        public bool IsShortSelected => SelectedTradeDirection == TD.Short;

        [ObservableProperty] private TF selectedTimeFrame;

        public ObservableCollection<Option<TF>> TimeFrameChoices { get; } =
            new(System.Enum.GetValues(typeof(TF)).Cast<TF>()
                .Select(tf => new Option<TF>(tf, DisplayText.TimeFrameLabel(tf))));
        [ObservableProperty] private Option<TF>? selectedTimeFrameOption;

        [ObservableProperty] private ObservableCollection<string> allIndicators = new();

        [ObservableProperty] private Preset? editingPreset;

        [ObservableProperty] private ObservableCollection<Preset> _presets = new();
        #endregion

        #region Commands
        [RelayCommand]
        private void SetMarketType(string value)
        {
            if (System.Enum.TryParse<MT>(value, out var mt)) SelectedMarketType = mt;
        }

        [RelayCommand]
        private void SetTradeDirection(string value)
        {
            if (System.Enum.TryParse<TD>(value, out var td)) SelectedTradeDirection = td;
        }

        [RelayCommand]
        private void SetTimeFrame(string value)
        {
            if (System.Enum.TryParse<TF>(value, out var tf))
            {
                SelectedTimeFrame = tf;
                SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(tf));
            }
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private async Task Save()
        {
            await _authService.InitializeAsync();
            var uid = await _authService.GetUserIdAsync();
            if (uid is null)
            {
                await Shell.Current.ShowPopupAsync(new InfoPopup("Please log in to save a preset."));
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            if (string.IsNullOrWhiteSpace(PresetName))
            {
                await Shell.Current.ShowPopupAsync(new InfoPopup("Missing name\nPlease enter a preset name."));
                return;
            }

            if (EditingPreset is null)
            {
                var newPreset = new Preset
                {
                    Name = PresetName.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    MarketType = SelectedMarketType,
                    TimeFrame = SelectedTimeFrame,
                    TradeDirection = SelectedTradeDirection,
                };

                await _presetRepo.InsertAsync(uid.Value, newPreset);
                if (Presets.All(p => p.Id != newPreset.Id))
                    Presets.Add(newPreset);

                await BaseVM.ShowSnackAsync(ChartSightAI.MVVM.Views.Presets.Current, "Preset added successfully");
            }
            else
            {
                var updated = new Preset
                {
                    Id = EditingPreset.Id,
                    Name = PresetName.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    MarketType = SelectedMarketType,
                    TimeFrame = SelectedTimeFrame,
                    TradeDirection = SelectedTradeDirection,
                };

                var idx = Presets.IndexOf(EditingPreset);
                if (idx >= 0) Presets[idx] = updated;

                await _presetRepo.UpdateAsync(uid.Value, updated);
                await BaseVM.ShowSnackAsync(ChartSightAI.MVVM.Views.Presets.Current, "Preset updated");
            }

            await Shell.Current.GoToAsync("..", true);
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..", true);
        }
        #endregion

        #region Change Handlers
        partial void OnSelectedMarketTypeChanged(MT value)
        {
            var defaults = MarketIndicatorHelper.GetDefaultIndicators(value);
            AllIndicators = new ObservableCollection<string>(defaults);
        }

        partial void OnSelectedTimeFrameOptionChanged(Option<TF>? value)
        {
            if (value != null) SelectedTimeFrame = value.Value;
        }
        #endregion

        #region Init
        public async Task InitializeAsync()
        {
            await _authService.InitializeAsync();
            Presets.Clear();

            var uid = await _authService.GetUserIdAsync();
            if (uid is null)
            {
                await Shell.Current.ShowPopupAsync(new InfoPopup("Please log in to manage presets."));
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            var list = await _presetRepo.GetAllAsync(uid.Value);
            foreach (var item in list) Presets.Add(item);

            if (EditingPreset is null)
            {
                PresetName = string.Empty;
                Description = string.Empty;

                SelectedMarketType = MT.Forex;
                SelectedTradeDirection = TD.Long;
                SelectedTimeFrame = TF.Hour1;
                SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(SelectedTimeFrame));

                AllIndicators = new ObservableCollection<string>(MarketIndicatorHelper.GetDefaultIndicators(SelectedMarketType));
            }
            else
            {
                PresetName = EditingPreset.Name ?? string.Empty;
                Description = EditingPreset.Description;

                SelectedMarketType = EditingPreset.MarketType ?? MT.Forex;
                SelectedTradeDirection = EditingPreset.TradeDirection ?? TD.Long;
                SelectedTimeFrame = EditingPreset.TimeFrame ?? TF.Hour1;
                SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(SelectedTimeFrame));

                var indicators = (EditingPreset.Indicators != null && EditingPreset.Indicators.Any())
                    ? EditingPreset.Indicators
                    : MarketIndicatorHelper.GetDefaultIndicators(SelectedMarketType);
                AllIndicators = new ObservableCollection<string>(indicators);
            }
        }
        #endregion
    }
}
