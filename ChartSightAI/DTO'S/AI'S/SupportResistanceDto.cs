using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.DTO_S.AI_S
{
    public sealed class SupportResistanceDto
    {
        [JsonPropertyName("type")]
        public SupportType Type { get; set; }   

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}
