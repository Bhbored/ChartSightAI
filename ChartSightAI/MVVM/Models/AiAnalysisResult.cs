using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.MVVM.Models
{
    public class AiAnalysisResult
    {
        public string? Summary { get; set; }
        public string? TrendAnalysis { get; set; }
        public string? Pattern { get; set; }

        public List<SupportResistanceLevel> SupportResistance { get; set; } = new();

        public List<string> Indicators { get; set; } = new();

        public string? Risk { get; set; }
        public TradeIdea? TradeIdea { get; set; }

        public List<string> Explainability { get; set; } = new();
    }

}
