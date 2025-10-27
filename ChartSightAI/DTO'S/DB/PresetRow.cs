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
    [Table("presets")]
    public sealed class PresetRow : BaseDto
    {
        [Column("created_at")] public DateTime CreatedAt { get; set; }
        [Column("updated_at")] public DateTime UpdatedAt { get; set; }

        [Column("name")] public string Name { get; set; } = "";
        [Column("description")] public string? Description { get; set; }
        [Column("market_type")] public string MarketType { get; set; } = "Crypto";
        [Column("time_frame")] public string TimeFrame { get; set; } = "Min5";
        [Column("trade_direction")] public string TradeDirection { get; set; } = "Long";


        public Preset ToDomain() => new()
        {
            Id = Id,
            Name = Name,
            Description = Description ?? "",
            MarketType = Enum.TryParse<MarketType>(MarketType, out var m) ? m : Enums.MarketType.Crypto,
            TimeFrame = Enum.TryParse<TimeFrame>(TimeFrame, out var tf) ? tf : Enums.TimeFrame.Min5,
            TradeDirection = Enum.TryParse<TradeDirection>(TradeDirection, out var td) ? td : Enums.TradeDirection.Long,
        };

        public static PresetRow FromDomain(Preset p, Guid userId) => new()
        {
            Id = p.Id,                  
            UserId = userId,
            Name = p.Name,
            Description = p.Description,
            MarketType = p.MarketType!.Value.ToString(),
            TimeFrame = p.TimeFrame!.Value.ToString(),
            TradeDirection = p.TradeDirection!.Value.ToString(),
        };
    }
}
