using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;
using System.Text.Json.Serialization;
using SupportResistanceLevel = ChartSightAI.MVVM.Models.SupportResistanceLevel;
namespace ChartSightAI.DTO_S.AI_S
{
    public sealed class AiAnalysisDto
    {
        [JsonPropertyName("summary")] public string? Summary { get; set; }
        [JsonPropertyName("trendAnalysis")] public string? TrendAnalysis { get; set; }
        [JsonPropertyName("pattern")] public string? Pattern { get; set; }
        [JsonPropertyName("risk")] public string? Risk { get; set; }
        [JsonPropertyName("explainability")] public string? Explainability { get; set; }

        [JsonPropertyName("indicators")] public List<string>? Indicators { get; set; }

        [JsonPropertyName("supportResistance")] public List<SupportResistanceDto>? SupportResistance { get; set; }
        [JsonPropertyName("tradeIdea")] public TradeIdeaDto? TradeIdea { get; set; }
    }
}
