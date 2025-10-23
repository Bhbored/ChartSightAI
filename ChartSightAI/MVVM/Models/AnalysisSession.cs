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

        public Preset? Preset { get; set; }

        public TradeDirection TradeDirection { get; set; }

        public string ImagePath => TradeDirection == TradeDirection.Long
            ? "images/long_trade.png"
            : "images/short_trade.png";
        public string? PreviewImage { get; set; }//from the device local storage
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public AiAnalysisResult? Result { get; set; }
        public bool IsRated { get; set; } = false;

    }


}
