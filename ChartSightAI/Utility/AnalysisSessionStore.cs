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

        public static void SeedDummyData(int samplesPerTimeFrame = 4)
        {
            if (Items.Count > 0) return;

            var now = DateTime.Now;
            var rnd = new Random(7);
            var indicatorPool = new[] { "RSI", "MACD", "EMA", "SMA", "Bollinger Bands", "VWAP", "Stochastic" };

            int id = 1;
            var allTfs = Enum.GetValues<TimeFrame>();

            foreach (var tf in allTfs)
            {
                for (int k = 0; k < samplesPerTimeFrame; k++)
                {
                    // Spread points out in the past according to timeframe scale
                    var baseStep = StepFor(tf);
                    var factor = (k + 1) * rnd.Next(2, 12); // pushes further back
                    var step = Scale(baseStep, factor);

                    // Small jitter so timestamps don’t align perfectly
                    var jitter = TimeSpan.FromSeconds(rnd.Next(0, (int)Math.Max(1, baseStep.TotalSeconds)));
                    var createdAt = now - step + jitter;

                    var indicators = indicatorPool.OrderBy(_ => rnd.Next()).Take(rnd.Next(1, 4)).ToList();

                    Items.Add(new AnalysisSession
                    {
                        Id = id++,
                        MarketType = (MarketType)rnd.Next(0, 3), // Forex/Stocks/Crypto
                        TimeFrame = tf,
                        TradeDirection = (id % 2 == 0) ? TradeDirection.Long : TradeDirection.Short,
                        CreatedAt = createdAt,
                        IsRated = rnd.NextDouble() < 0.85,
                        Hit = rnd.NextDouble() < 0.72,
                        Result = new AiAnalysisResult
                        {
                            Summary = $"TF {tf}: {(id % 2 == 0 ? "Bullish" : "Bearish")} setup with {(indicators.Contains("EMA") ? "EMA confluence" : "momentum")} confirmation.",
                            TrendAnalysis = (id % 2 == 0)
                                ? "Higher highs and higher lows; buyers in control."
                                : "Lower lows and lower highs; sellers in control.",
                            Indicators = indicators,
                            Explainability = "Confidence driven by indicator alignment and recent structure."
                        }
                    });
                }
            }

            // Keep the collection ordered by time (oldest → newest)
            var ordered = Items.OrderBy(s => s.CreatedAt).ToList();
            Items.Clear();
            foreach (var s in ordered) Items.Add(s);
        }

        // Map each timeframe to a realistic spacing unit
        private static TimeSpan StepFor(TimeFrame tf) => tf switch
        {
            TimeFrame.Sec10 => TimeSpan.FromSeconds(10),
            TimeFrame.Sec15 => TimeSpan.FromSeconds(15),
            TimeFrame.Sec30 => TimeSpan.FromSeconds(30),
            TimeFrame.Min1 => TimeSpan.FromMinutes(1),
            TimeFrame.Min3 => TimeSpan.FromMinutes(3),
            TimeFrame.Min5 => TimeSpan.FromMinutes(5),
            TimeFrame.Min15 => TimeSpan.FromMinutes(15),
            TimeFrame.Min30 => TimeSpan.FromMinutes(30),
            TimeFrame.Hour1 => TimeSpan.FromHours(1),
            TimeFrame.Hour2 => TimeSpan.FromHours(2),
            TimeFrame.Hour4 => TimeSpan.FromHours(4),
            TimeFrame.Day1 => TimeSpan.FromDays(1),
            TimeFrame.Week1 => TimeSpan.FromDays(7),
            TimeFrame.Month1 => TimeSpan.FromDays(30),
            _ => TimeSpan.FromMinutes(5)
        };

        private static TimeSpan Scale(TimeSpan span, double factor)
            => TimeSpan.FromTicks((long)(span.Ticks * factor));
    }
}
