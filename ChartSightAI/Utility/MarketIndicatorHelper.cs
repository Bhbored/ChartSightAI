using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.Utility
{
    public static class MarketIndicatorHelper
    {
        public static List<string> GetDefaultIndicators(MarketType marketType)
        {
            return marketType switch
            {
                MarketType.Forex => new List<string>
            {
                "RSI (Relative Strength Index)",
                "MACD (Moving Average Convergence Divergence)",
                "Moving Averages"
            },
                MarketType.Stocks => new List<string>
            {
                "Volume",
                "EMA (Exponential Moving Average)",
                "RSI (Relative Strength Index)"
            },
                MarketType.Crypto => new List<string>
            {
                "RSI (Relative Strength Index)",
                "MACD (Moving Average Convergence Divergence)",
                "Bollinger Bands"
            },
                _ => new List<string>()
            };
        }
    }

}
