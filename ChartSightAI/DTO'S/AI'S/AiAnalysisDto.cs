using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.DTO_S.AI_S
{
    public sealed class AiAnalysisDto
    {
        [JsonProperty("summary")] public string? Summary { get; set; }
        [JsonProperty("trend_analysis")] public string? TrendAnalysis { get; set; }
        [JsonProperty("pattern")] public string? Pattern { get; set; }
        [JsonProperty("risk")] public string? Risk { get; set; }
        [JsonProperty("explainability")] public string? Explainability { get; set; }
        [JsonProperty("indicators")] public List<string>? Indicators { get; set; }
        [JsonProperty("support_resistance")] public List<SupportResistanceDto>? SupportResistance { get; set; }
        [JsonProperty("trade_idea")] public TradeIdeaDto? TradeIdea { get; set; }
    }
}
