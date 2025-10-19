using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.MVVM.Models
{
    public class SupportResistanceLevel
    {
        public SupportType Type { get; set; }
        public double Price { get; set; }
        public double Confidence { get; set; }  // Optional confidence score (0–100%)
    }
}
