using ChartSightAI.MVVM.Models;
using ChartSightAI.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.TestData
{
    public static class AiAnalysisMockData
    {
        public static AiAnalysisResult BuildForPreset(Preset? preset)
        {
            var market = preset?.MarketType ?? MarketType.Crypto;

            // starting price by market (purely mock)
            double basePrice = market switch
            {
                MarketType.Forex => 1.0850,
                MarketType.Stocks => 150.00,
                MarketType.Crypto => 42000.00,
                _ => 100.00
            };

            var indicators = (preset?.Indicators?.Any() == true)
                ? preset!.Indicators.ToList()
                : MarketIndicatorHelper.GetDefaultIndicators(market);

            // small helpers to make levels look “real”
            double pct(double p) => Math.Round(basePrice * (1.0 + p / 100.0), 2);

            var sr = new List<SupportResistanceLevel>
            {
                new() { Type = SupportType.Support,    Price = pct(-2.2), Confidence = 92 },
                new() { Type = SupportType.Support,    Price = pct(-3.8), Confidence = 86 },
                new() { Type = SupportType.Resistance, Price = pct( 4.5), Confidence = 81 },
            };

            var targets = new List<double> { pct(2.0), pct(4.0), pct(6.0) };

            return new AiAnalysisResult
            {
                Summary = "Bullish bias with healthy momentum across key indicators.",
                TrendAnalysis = "Series of higher lows; buyers defending pullbacks near first support.",
                Pattern = "Ascending structure; potential continuation if volume persists.",
                SupportResistance = sr,
                Indicators = indicators,
                Risk = "Moderate volatility; watch for fakeouts near the first resistance.",
                TradeIdea = new TradeIdea
                {
                    Entry = Math.Round(basePrice, 2),
                    StopLoss = pct(-2.1),
                    Targets = targets,
                    Rationale = "Entry near pullback support with improving momentum; confirmation on rising volume."
                },
                Explainability =
        "The suggestion is driven by price action forming higher lows with expanding up-volume; " +
        "RSI holding above its midline indicates positive momentum, and a positive MACD histogram " +
        "supports continuation while key support sits below the entry."
            };
        }
    }
}
