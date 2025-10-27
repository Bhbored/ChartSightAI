using System.Text.Json;
using System.Text.Json.Serialization;
using ChartSightAI.DTO_S.AI_S; // AiAnalysisResult + converters you already added
using ChartSightAI.MVVM.Models; // SupportType converter if it lives here

static class AiJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
            new SupportTypeJsonConverter() // your enum converter
        }
    };
}
