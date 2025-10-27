using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChartSightAI.DTO_S.AI_S
{
    public sealed class TradeIdeaDto
    {
        [JsonPropertyName("entry")] public double Entry { get; set; }
        [JsonPropertyName("stopLoss")] public double StopLoss { get; set; }
        [JsonPropertyName("targets")] public List<double>? Targets { get; set; }
        [JsonPropertyName("rationale")] public string? Rationale { get; set; }
    }
}
