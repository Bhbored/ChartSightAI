using ChartSightAI.MVVM.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.DTO_S.DB
{
    [Table("analysis_sessions")]
    public sealed class AnalysisSessionRow : BaseDto
    {
        [Column("created_at")] public DateTime CreatedAt { get; set; }
        [Column("updated_at")] public DateTime UpdatedAt { get; set; }

        [Column("market_type")] public string MarketType { get; set; } = "Crypto";
        [Column("time_frame")] public string TimeFrame { get; set; } = "Min5";
        [Column("trade_direction")] public string TradeDirection { get; set; } = "Long";

        [Column("is_rated")] public bool IsRated { get; set; }
        [Column("hit")] public bool Hit { get; set; }

        [Column("result_summary")] public string? ResultSummary { get; set; }
        [Column("result_trend_analysis")] public string? ResultTrendAnalysis { get; set; }
        [Column("result_pattern")] public string? ResultPattern { get; set; }
        [Column("result_risk")] public string? ResultRisk { get; set; }
        [Column("result_explainability")] public string? ResultExplainability { get; set; }
        [Column("result_indicators")] public string[] ResultIndicators { get; set; } = Array.Empty<string>();
        [Column("result_support_resistance", (Newtonsoft.Json.NullValueHandling)JsonSerializerDefaults.General)] public List<SupportResistanceLevel>? ResultSupportResistance { get; set; }
        [Column("result_trade_idea", (Newtonsoft.Json.NullValueHandling)JsonSerializerDefaults.General)] public TradeIdea? ResultTradeIdea { get; set; }


        public AnalysisSession ToDomain() => new()
        {
            Id = Id,
            CreatedAt = CreatedAt,
            MarketType = Enum.TryParse<MarketType>(MarketType, out var m) ? m : Enums.MarketType.Crypto,
            TimeFrame = Enum.TryParse<TimeFrame>(TimeFrame, out var tf) ? tf : Enums.TimeFrame.Min5,
            TradeDirection = Enum.TryParse<TradeDirection>(TradeDirection, out var td) ? td : Enums.TradeDirection.Long,
            IsRated = IsRated,
            Hit = Hit,
            Result = new AiAnalysisResult
            {
                Summary = ResultSummary ?? "",
                TrendAnalysis = ResultTrendAnalysis ?? "",
                Pattern = ResultPattern,
                Risk = ResultRisk,
                Explainability = ResultExplainability ?? "",
                Indicators = ResultIndicators?.ToList() ?? new List<string>(),
                SupportResistance = ResultSupportResistance ?? new List<SupportResistanceLevel>(),
                TradeIdea = ResultTradeIdea
            }
        };

        public static AnalysisSessionRow FromDomain(AnalysisSession s, Guid userId) => new()
        {
            Id = s.Id,
            UserId = userId,
            CreatedAt = s.CreatedAt,
            MarketType = s.MarketType.ToString(),
            TimeFrame = s.TimeFrame.ToString(),
            TradeDirection = s.TradeDirection.ToString(),
            IsRated = s.IsRated,
            Hit = s.Hit,
            ResultSummary = s.Result?.Summary,
            ResultTrendAnalysis = s.Result?.TrendAnalysis,
            ResultPattern = s.Result?.Pattern,
            ResultRisk = s.Result?.Risk,
            ResultExplainability = s.Result?.Explainability,
            ResultIndicators = s.Result?.Indicators?.ToArray() ?? Array.Empty<string>(),
            ResultSupportResistance = s.Result?.SupportResistance,
            ResultTradeIdea = s.Result?.TradeIdea
        };
    }
}

