using ChartSightAI.MVVM.Models; // Keep this to reference the original Enums for casting
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class NewPredictionVM : ObservableObject
    {
        // Nested Enums copied into ViewModel
        public enum MarketType
        {
            Forex,
            Stocks,
            Crypto
        }

        public enum TimeFrame
        {
            Sec10,
            Sec15,
            Sec30,
            Min1,
            Min3,
            Min5,
            Min15,
            Min30,
            Hour1,
            Hour2,
            Hour4,
            Day1,
            Week1,
            Month1
        }

        public enum TradeDirection
        {
            Long,
            Short
        }

        [ObservableProperty]
        private AnalysisSession _analysisSession;

        public ObservableCollection<TimeFrame> TimeFrameOptions { get; }
        public ObservableCollection<MarketType> MarketTypeOptions { get; }
        public ObservableCollection<TradeDirection> TradeDirectionOptions { get; }

        // Commands for each Market Type
        public ICommand SelectMarketTypeForexCommand { get; }
        public ICommand SelectMarketTypeStocksCommand { get; }
        public ICommand SelectMarketTypeCryptoCommand { get; }

        // Commands for each Trade Direction
        public ICommand SelectTradeDirectionLongCommand { get; }
        public ICommand SelectTradeDirectionShortCommand { get; }

        public ICommand AnalyzeChartCommand { get; }
        public ICommand CancelCommand { get; }

        public NewPredictionVM()
        {
            AnalysisSession = new AnalysisSession();
            TimeFrameOptions = new ObservableCollection<TimeFrame>(Enum.GetValues(typeof(TimeFrame)).Cast<TimeFrame>());
            MarketTypeOptions = new ObservableCollection<MarketType>(Enum.GetValues(typeof(MarketType)).Cast<MarketType>());
            TradeDirectionOptions = new ObservableCollection<TradeDirection>(Enum.GetValues(typeof(TradeDirection)).Cast<TradeDirection>());

            // Initialize specific commands
            SelectMarketTypeForexCommand = new RelayCommand(() => OnSelectMarketType(MarketType.Forex));
            SelectMarketTypeStocksCommand = new RelayCommand(() => OnSelectMarketType(MarketType.Stocks));
            SelectMarketTypeCryptoCommand = new RelayCommand(() => OnSelectMarketType(MarketType.Crypto));

            SelectTradeDirectionLongCommand = new RelayCommand(() => OnSelectTradeDirection(TradeDirection.Long));
            SelectTradeDirectionShortCommand = new RelayCommand(() => OnSelectTradeDirection(TradeDirection.Short));

            AnalyzeChartCommand = new AsyncRelayCommand(OnAnalyzeChartAsync);
            CancelCommand = new AsyncRelayCommand(OnCancelAsync);
        }

        private void OnSelectMarketType(MarketType marketType)
        {
            AnalysisSession.MarketType = (Enums.MarketType)marketType;
            OnPropertyChanged(nameof(AnalysisSession));
        }

        private void OnSelectTradeDirection(TradeDirection tradeDirection)
        {
            AnalysisSession.TradeDirection = (Enums.TradeDirection)tradeDirection;
            OnPropertyChanged(nameof(AnalysisSession));
        }

        private async Task OnAnalyzeChartAsync()
        {
            // Placeholder for analysis logic
            await Shell.Current.DisplayAlert("Info", "Analysis started!", "OK");
        }

        private async Task OnCancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public static string GetMarketTypeDisplayString(MarketType type)
        {
            return type switch
            {
                MarketType.Forex => "Forex Market",
                MarketType.Stocks => "Stock Market",
                MarketType.Crypto => "Crypto Market",
                _ => type.ToString()
            };
        }

        public static string GetTimeFrameDisplayString(TimeFrame frame)
        {
            return frame switch
            {
                TimeFrame.Sec10 => "10 Seconds",
                TimeFrame.Sec15 => "15 Seconds",
                TimeFrame.Sec30 => "30 Seconds",
                TimeFrame.Min1 => "1 Minute",
                TimeFrame.Min3 => "3 Minutes",
                TimeFrame.Min5 => "5 Minutes",
                TimeFrame.Min15 => "15 Minutes",
                TimeFrame.Min30 => "30 Minutes",
                TimeFrame.Hour1 => "1 Hour",
                TimeFrame.Hour2 => "2 Hours",
                TimeFrame.Hour4 => "4 Hours",
                TimeFrame.Day1 => "Daily",
                TimeFrame.Week1 => "Weekly",
                TimeFrame.Month1 => "Monthly",
                _ => frame.ToString()
            };
        }

        public static string GetTradeDirectionDisplayString(TradeDirection direction)
        {
            return direction switch
            {
                TradeDirection.Long => "Long Trade",
                TradeDirection.Short => "Short Trade",
                _ => direction.ToString()
            };
        }
    }
}