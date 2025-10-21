using System.Collections.ObjectModel;
using ChartSightAI.MVVM.Models;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.Utility
{
    public static class PresetStore
    {
        public static ObservableCollection<Preset> Items { get; set; } = new(){
        new ()
            {
                Id = 1,
                Name = "Recommended (Forex H1 Long)",
                Description = "Starter preset",
                MarketType = MarketType.Forex,
                TimeFrame = TimeFrame.Hour1,
                TradeDirection = TradeDirection.Long,
                Indicators = MarketIndicatorHelper.GetDefaultIndicators(MarketType.Forex)
            },
        new ()
            {
                Id = 2,
                Name = "Daily Momentum",
                Description = "Catch daily moves",
                MarketType = MarketType.Stocks,
                TimeFrame = TimeFrame.Day1,
                TradeDirection = TradeDirection.Long,
                Indicators = MarketIndicatorHelper.GetDefaultIndicators(MarketType.Stocks)
            }
        };

        public static Preset? TempPreset { get; set; }
    }
}
