using System;
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
    public partial class NewPredictionVM : ObservableObject
    {
        #region Fields
        #endregion

        #region Properties
        [ObservableProperty] private AnalysisSession analysisSession = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsForexSelected))]
        [NotifyPropertyChangedFor(nameof(IsStocksSelected))]
        [NotifyPropertyChangedFor(nameof(IsCryptoSelected))]
        private MT selectedMarketType;

        public bool IsForexSelected => SelectedMarketType == MT.Forex;
        public bool IsStocksSelected => SelectedMarketType == MT.Stocks;
        public bool IsCryptoSelected => SelectedMarketType == MT.Crypto;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsTFH1))]
        [NotifyPropertyChangedFor(nameof(IsTFH4))]
        [NotifyPropertyChangedFor(nameof(IsTFD1))]
        private TF selectedTimeFrame;

        public bool IsTFH1 => SelectedTimeFrame == TF.Hour1;
        public bool IsTFH4 => SelectedTimeFrame == TF.Hour4;
        public bool IsTFD1 => SelectedTimeFrame == TF.Day1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLongSelected))]
        [NotifyPropertyChangedFor(nameof(IsShortSelected))]
        private TD selectedTradeDirection;

        public bool IsLongSelected => SelectedTradeDirection == TD.Long;
        public bool IsShortSelected => SelectedTradeDirection == TD.Short;

        public ObservableCollection<Option<TF>> TimeFrameChoices { get; } =
            new(Enum.GetValues(typeof(TF)).Cast<TF>()
                .Select(tf => new Option<TF>(tf, DisplayText.TimeFrameLabel(tf))));

        [ObservableProperty] private Option<TF>? selectedTimeFrameOption;

        public ObservableCollection<Preset> Presets { get; private set; } = new();
        [ObservableProperty] private Preset? selectedPreset;

        [ObservableProperty] private bool isTechnicalSelected;
        [ObservableProperty] private bool isPatternSelected;
        [ObservableProperty] private bool isAiSelected;
        #endregion

        #region Commands
        public IRelayCommand<string> SetMarketTypeFromStringCommand { get; }
        public IRelayCommand<string> SetTimeFrameFromStringCommand { get; }
        public IRelayCommand<string> SetTradeDirectionFromStringCommand { get; }
        public IRelayCommand<string> ToggleStrategyFocusCommand { get; }
        public IAsyncRelayCommand AnalyzeChartCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        #endregion

        #region Ctor
        public NewPredictionVM()
        {
            SetMarketTypeFromStringCommand = new RelayCommand<string>(s =>
            {
                if (Enum.TryParse<MT>(s, out var mt)) SelectedMarketType = mt;
            });

            SetTimeFrameFromStringCommand = new RelayCommand<string>(s =>
            {
                if (Enum.TryParse<TF>(s, out var tf))
                {
                    SelectedTimeFrame = tf;
                    SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(tf));
                }
            });

            SetTradeDirectionFromStringCommand = new RelayCommand<string>(s =>
            {
                if (Enum.TryParse<TD>(s, out var td)) SelectedTradeDirection = td;
            });

            ToggleStrategyFocusCommand = new RelayCommand<string>(key =>
            {
                switch (key)
                {
                    case "Technical": IsTechnicalSelected = !IsTechnicalSelected; break;
                    case "Pattern": IsPatternSelected = !IsPatternSelected; break;
                    case "AI": IsAiSelected = !IsAiSelected; break;
                }
            });

            AnalyzeChartCommand = new AsyncRelayCommand(OnAnalyzeChartAsync);
            CancelCommand = new AsyncRelayCommand(OnCancelAsync);
        }
        #endregion

        #region Partial Change Handlers
        partial void OnSelectedMarketTypeChanged(MT value)
        {
            AnalysisSession.MarketType = value;
            ReloadPresets(value);
            OnPropertyChanged(nameof(AnalysisSession));
        }

        partial void OnSelectedTimeFrameChanged(TF value)
        {
            AnalysisSession.TimeFrame = value;
            OnPropertyChanged(nameof(AnalysisSession));
        }

        partial void OnSelectedTradeDirectionChanged(TD value)
        {
            AnalysisSession.TradeDirection = value;
            OnPropertyChanged(nameof(AnalysisSession));
        }

        partial void OnSelectedTimeFrameOptionChanged(Option<TF>? value)
        {
            if (value != null) SelectedTimeFrame = value.Value;
        }

        partial void OnSelectedPresetChanged(Preset? value)
        {
            AnalysisSession.Preset = value;

            if (value?.MarketType.HasValue == true)
                SelectedMarketType = value.MarketType.Value;

            if (value?.TimeFrame.HasValue == true)
            {
                SelectedTimeFrame = value.TimeFrame.Value;
                SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(SelectedTimeFrame));
            }

            if (value?.TradeDirection.HasValue == true)
                SelectedTradeDirection = value.TradeDirection.Value;
        }
        #endregion

        #region Methods
        private Preset BuildRecommendedPreset(MT mt)
        {
            return new Preset
            {
                Id = -1,
                Name = "Recommended",
                Description = "Recommended indicators for selected market",
                MarketType = null,
                TimeFrame = null,
                TradeDirection = null,
                Indicators = MarketIndicatorHelper.GetDefaultIndicators(mt)
            };
        }

        private void ReloadPresets(MT mt)
        {
            var rec = BuildRecommendedPreset(mt);
            Presets = new ObservableCollection<Preset>(new[] { rec });
            OnPropertyChanged(nameof(Presets));
            SelectedPreset = rec;
        }
        #endregion

        #region Tasks
        public Task InitializeAsync()
        {
            SelectedMarketType = MT.Crypto;
            SelectedTimeFrame = TF.Hour1;
            SelectedTradeDirection = TD.Long;
            SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(SelectedTimeFrame));

            IsTechnicalSelected = true;
            IsPatternSelected = false;
            IsAiSelected = false;

            ReloadPresets(SelectedMarketType);

            AnalysisSession.CreatedAt = DateTime.Now;
            AnalysisSession.Preset = SelectedPreset;
            return Task.CompletedTask;
        }

        private async Task OnAnalyzeChartAsync()
        {
            AnalysisSession.CreatedAt = DateTime.Now;
            AnalysisSession.Preset = SelectedPreset;

            var mt = DisplayText.Market(SelectedMarketType);
            var tf = DisplayText.TimeFrameLabel(SelectedTimeFrame);
            var dir = DisplayText.Direction(SelectedTradeDirection);
            var presetName = SelectedPreset?.Name ?? "Recommended";

            var msg = $"Market: {mt}\nTimeframe: {tf}\nDirection: {dir}\nPreset: {presetName}";
            await Shell.Current.DisplayAlert("Info", msg, "OK");
        }

        private async Task OnCancelAsync() => InitializeAsync();
        #endregion

    }
}
