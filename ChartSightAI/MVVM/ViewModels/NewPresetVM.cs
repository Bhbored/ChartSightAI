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

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class NewPresetVM : ObservableObject
    {
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
            if (string.IsNullOrWhiteSpace(PresetName))
            {
                await Shell.Current.DisplayAlert("Missing name", "Please enter a preset name.", "OK");
                return;
            }

            if (EditingPreset is null)
            {
                var nextId = PresetStore.Items.Count == 0 ? 1 : PresetStore.Items.Max(p => p.Id) + 1;
                var newPreset = new Preset
                {
                    Id = nextId,
                    Name = PresetName.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    MarketType = SelectedMarketType,
                    TimeFrame = SelectedTimeFrame,
                    TradeDirection = SelectedTradeDirection,
                    Indicators = AllIndicators.ToList()
                };
                PresetStore.Items.Insert(0, newPreset);
            }
            else
            {
                var idx = PresetStore.Items.IndexOf(EditingPreset);
                if (idx >= 0)
                {
                    var updated = new Preset
                    {
                        Id = EditingPreset.Id,
                        Name = PresetName.Trim(),
                        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                        MarketType = SelectedMarketType,
                        TimeFrame = SelectedTimeFrame,
                        TradeDirection = SelectedTradeDirection,
                        Indicators = AllIndicators.ToList()
                    };
                    PresetStore.Items[idx] = updated;
                }
            }

            PresetStore.TempPreset = null;
            await Shell.Current.GoToAsync("..", true);
        }

        [RelayCommand]
        private async Task Cancel()
        {
            PresetStore.TempPreset = null;
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
        public Task InitializeAsync()
        {
            var edit = PresetStore.TempPreset;
            if (edit is null)
            {
                EditingPreset = null;
                PresetName = string.Empty;
                Description = string.Empty;

                SelectedMarketType = MT.Forex;
                SelectedTradeDirection = TD.Long;
                SelectedTimeFrame = TF.Hour1;
                SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(SelectedTimeFrame));

                AllIndicators = new ObservableCollection<string>(
                    MarketIndicatorHelper.GetDefaultIndicators(SelectedMarketType));
            }
            else
            {
                EditingPreset = edit;

                PresetName = edit.Name ?? string.Empty;
                Description = edit.Description;

                SelectedMarketType = edit.MarketType ?? MT.Forex;
                SelectedTradeDirection = edit.TradeDirection ?? TD.Long;
                SelectedTimeFrame = edit.TimeFrame ?? TF.Hour1;
                SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(SelectedTimeFrame));

                var indicators = (edit.Indicators != null && edit.Indicators.Any())
                    ? edit.Indicators
                    : MarketIndicatorHelper.GetDefaultIndicators(SelectedMarketType);
                AllIndicators = new ObservableCollection<string>(indicators);
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}
