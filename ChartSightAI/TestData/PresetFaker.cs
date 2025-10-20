using System.Collections.Generic;
using System.Linq;
using Bogus;
using ChartSightAI.MVVM.Models;
using ChartSightAI.Utility;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.TestData
{
    public static class PresetFaker
    {
        public static List<Preset> CreateList(int count)
        {
            var faker = BuildFaker();
            return Enumerable.Range(0, count).Select(_ => faker.Generate()).ToList();
        }

        public static Preset CreateOne() => BuildFaker().Generate();

        static Faker<Preset> BuildFaker()
        {
            var id = 1;
            return new Faker<Preset>()
                .RuleFor(p => p.Id, _ => id++)
                .RuleFor(p => p.Name, f => f.PickRandom(new[]
                {
                    "Daily Momentum","Breakout Scout","Mean Revert","EMA Crossover","Range Watch"
                }))
                .RuleFor(p => p.Description, f => f.PickRandom(new[]
                {
                    "Catch daily trends with key indicators",
                    "Scans for high-probability breakouts",
                    "Looks for pullbacks to mean",
                    "Tracks moving-average signals",
                    "Finds clean ranges and bounces"
                }))
                .RuleFor(p => p.MarketType, f => f.PickRandom<MarketType>())
                .RuleFor(p => p.TimeFrame, f => f.PickRandom<TimeFrame>())
                .RuleFor(p => p.TradeDirection, f => f.PickRandom<TradeDirection>())
                .FinishWith((f, p) => p.SetIndicatorsByMarket());
        }
    }
}
