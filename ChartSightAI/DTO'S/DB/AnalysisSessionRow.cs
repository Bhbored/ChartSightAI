using ChartSightAI.MVVM.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        [Column("result_explainability")] public string? ResultExplainability { get; set; }
        [Column("result_indicators")] public string[] ResultIndicators { get; set; } = Array.Empty<string>();

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
                Explainability = ResultExplainability ?? "",
                Indicators = ResultIndicators?.ToList() ?? new List<string>()
            }
        };

        public static AnalysisSessionRow FromDomain(AnalysisSession s, Guid userId) => new()
        {
            Id = s.Id, // 0 for new inserts; DB will assign if you omit it in Upsert
            UserId = userId,
            CreatedAt = s.CreatedAt,
            MarketType = s.MarketType.ToString(),
            TimeFrame = s.TimeFrame.ToString(),
            TradeDirection = s.TradeDirection.ToString(),
            IsRated = s.IsRated,
            Hit = s.Hit,
            ResultSummary = s.Result?.Summary,
            ResultTrendAnalysis = s.Result?.TrendAnalysis,
            ResultExplainability = s.Result?.Explainability,
            ResultIndicators = s.Result?.Indicators?.ToArray() ?? Array.Empty<string>()
        };
    }
}

