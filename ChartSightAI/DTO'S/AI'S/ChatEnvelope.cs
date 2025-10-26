using ChartSightAI.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.DTO_S.AI_S
{
    internal sealed class ChatEnvelope
    {
        [JsonProperty("choices")] public List<ChatChoice>? Choices { get; set; }
    }
}
