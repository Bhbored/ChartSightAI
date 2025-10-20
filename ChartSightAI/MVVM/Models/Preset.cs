using ChartSightAI.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.MVVM.Models
{
    public class Preset
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string? Description { get; set; } 
        public MarketType? MarketType { get; set; }
        public TimeFrame? TimeFrame { get; set; }
        public TradeDirection? TradeDirection { get; set; }
        public List<string> Indicators { get; private set; } = new();

        public void SetIndicatorsByMarket()
        {
            if (MarketType.HasValue)
                Indicators = MarketIndicatorHelper.GetDefaultIndicators(MarketType.Value);
            else
                Indicators.Clear();
        }
    }
}
