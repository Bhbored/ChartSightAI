using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.DTO_S.AI_S
{
    public class AiAnalysisResponse
    {
        [JsonProperty("summary")]
        public string? Summary { get; set; }

        [JsonProperty("trend_analysis")]
        public string? TrendAnalysis { get; set; }

        [JsonProperty("pattern")]
        public string? Pattern { get; set; }

        [JsonProperty("support_resistance")]
        public List<SupportResistanceLevelAi>? SupportResistance { get; set; }

        [JsonProperty("indicators")]
        public Dictionary<string, string>? Indicators { get; set; }

        [JsonProperty("risk")]
        public string? Risk { get; set; }

        [JsonProperty("trade_idea")]
        public TradeIdeaAi? TradeIdea { get; set; }

        [JsonProperty("explainability")]
        public List<string>? Explainability { get; set; }
    }

}
