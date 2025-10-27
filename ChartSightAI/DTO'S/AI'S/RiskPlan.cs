using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChartSightAI.DTO_S.AI_S
{
    public sealed class RiskPlan
    {
        [JsonPropertyName("entry")]
        public double Entry { get; set; }

        [JsonPropertyName("stopLoss")]
        public double StopLoss { get; set; }

        [JsonPropertyName("takeProfit1")]
        public double TakeProfit1 { get; set; }

        [JsonPropertyName("takeProfit2")]
        public double? TakeProfit2 { get; set; }   
    }
}
