using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ChartSightAI.MVVM.Models;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.Utility
{
    public static class AnalysisSessionStore
    {
        public static ObservableCollection<AnalysisSession> Items { get; } = new();
        
        public static void SeedDummyData()
        {
            if (Items.Count > 0) return;

            var now = DateTime.Now.Date;
            var rnd = new Random(7);
            var indicatorPool = new[] { "RSI", "MACD", "EMA", "SMA", "Bollinger Bands", "VWAP", "Stochastic" };

            for (int i = 0; i < 24; i++)
            {
                var indicators = indicatorPool.OrderBy(_ => rnd.Next()).Take(rnd.Next(1, 4)).ToList();

                Items.Add(new AnalysisSession
                {
                    Id = i + 1,
                    TradeDirection = i % 2 == 0 ? TradeDirection.Long : TradeDirection.Short,
                    CreatedAt = now.AddDays(-i),
                    IsRated = true,
                    Hit = i % 4 != 0,
                    Result = new AiAnalysisResult
                    {
                        Summary = $"Session {i + 1}: {(i % 2 == 0 ? "Bullish" : "Bearish")} setup with {(indicators.Contains("EMA") ? "EMA confluence" : "momentum")} confirmation.",
                        TrendAnalysis = i % 2 == 0 ? "Higher highs and higher lows; buyers in control." : "Lower lows and lower highs; sellers in control.",
                        Indicators = indicators,
                        Explainability = "Confidence driven by indicator alignment and recent structure."
                    }
                });
            }
        }
    }
}
