using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.MVVM.Models
{
   public class AnalysisSession
    {
        public int Id { get; set; }

        public MarketType MarketType { get; set; }
        public TimeFrame TimeFrame { get; set; }
        public string? Preset { get; set; } // Keep string for flexibility ("Recommended", "Scalping", etc.)

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsFavorite { get; set; }

        // AI-related
        public string? AiModelUsed { get; set; }
        public string? AiResponseRaw { get; set; }
        public AiAnalysisResult? ParsedResult { get; set; }
    }
}
