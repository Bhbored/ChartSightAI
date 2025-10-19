using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.DTO_S.AI_S
{

    public class AiAnalysisRequest
    {
        [JsonProperty("market_type")]
        public string? MarketType { get; set; }

        [JsonProperty("time_frame")]
        public string? TimeFrame { get; set; }

        [JsonProperty("preset")]
        public string? Preset { get; set; }

        [JsonProperty("image_base64")]
        public string? ImageBase64 { get; set; }

        [JsonProperty("additional_notes")]
        public string? AdditionalNotes { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; } = "Gemini 1.5"; // default

        [JsonProperty("response_format")]
        public string ResponseFormat { get; set; } = "json";
    }

}
