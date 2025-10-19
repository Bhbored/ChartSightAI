using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.MVVM.Models
{
    public class Enums
    {
        public enum MarketType
        {
            Forex,
            Stocks,
            Crypto
        }

        public enum TimeFrame
        {
            Sec10,
            Sec15,
            Sec30,
            Min1,
            Min3,
            Min5,
            Min15,
            Min30,
            Hour1,
            Hour2,
            Hour4,
            Day1,
            Week1,
            Month1
        }

        public enum SupportType
        {
            Support,
            Resistance
        }
        public enum TradeDirection
        {
            Long,
            Short
        }

    }
}
