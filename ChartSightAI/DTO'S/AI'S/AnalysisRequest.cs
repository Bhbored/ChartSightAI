using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.DTO_S.AI_S
{
    public sealed class AnalysisRequest
    {
        public MarketType MarketType { get; init; }
        public TimeFrame TimeFrame { get; init; }
        public TradeDirection TradeDirection { get; init; }
        public IList<string>? Indicators { get; init; }
        public string? ImagePath { get; init; }
        public string? PreviewImage { get; init; }
        public string? Notes { get; init; }
    }
}
