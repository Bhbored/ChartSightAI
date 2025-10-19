using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.MVVM.Models
{
    public class AppSettings
    {
        public string PreferredModel { get; set; } = "Gemini 1.5";
        public MarketType DefaultMarket { get; set; } = MarketType.Forex;
        public bool AutoSaveSessions { get; set; } = true;
        public string Theme { get; set; } = "Light";
    }

}
