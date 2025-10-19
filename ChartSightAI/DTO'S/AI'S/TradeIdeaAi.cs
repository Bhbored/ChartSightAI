using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.DTO_S.AI_S
{
    public class TradeIdeaAi
    {
        [JsonProperty("entry")]
        public double Entry { get; set; }

        [JsonProperty("stop_loss")]
        public double StopLoss { get; set; }

        [JsonProperty("targets")]
        public List<double>? Targets { get; set; }

        [JsonProperty("rationale")]
        public string? Rationale { get; set; }
    }
}
