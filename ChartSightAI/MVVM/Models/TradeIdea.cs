using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.MVVM.Models
{
    public class TradeIdea
    {
        public double Entry { get; set; }
        public double StopLoss { get; set; }
        public List<double> Targets { get; set; } = new();
        public string Rationale { get; set; }
    }

}
