using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.DTO_S.AI_S
{
    public class SupportResistanceLevelAi
    {
        [JsonProperty("type")]
        public string? Type { get; set; }  // "support" or "resistance"

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }
}
