using MT = ChartSightAI.MVVM.Models.Enums.MarketType;
using TF = ChartSightAI.MVVM.Models.Enums.TimeFrame;
using TD = ChartSightAI.MVVM.Models.Enums.TradeDirection;

namespace ChartSightAI.Utility
{
    public static class DisplayText
    {
        public static string Market(MT t) => t switch
        {
            MT.Forex => "Forex Market",
            MT.Stocks => "Stock Market",
            MT.Crypto => "Crypto Market",
            _ => t.ToString()
        };

        public static string TimeFrameLabel(TF f) => f switch
        {
            TF.Sec10 => "10 Seconds",
            TF.Sec15 => "15 Seconds",
            TF.Sec30 => "30 Seconds",
            TF.Min1 => "1 Minute",
            TF.Min3 => "3 Minutes",
            TF.Min5 => "5 Minutes",
            TF.Min15 => "15 Minutes",
            TF.Min30 => "30 Minutes",
            TF.Hour1 => "1 Hour",
            TF.Hour2 => "2 Hours",
            TF.Hour4 => "4 Hours",
            TF.Day1 => "Daily",
            TF.Week1 => "Weekly",
            TF.Month1 => "Monthly",
            _ => f.ToString()
        };

        public static string Direction(TD d) => d switch
        {
            TD.Long => "Long Trade",
            TD.Short => "Short Trade",
            _ => d.ToString()
        };
    }
}
