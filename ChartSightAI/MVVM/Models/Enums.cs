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
            Min1,
            Min5,
            Min15,
            Hour1,
            Day1,
            Week1
        }
        public enum SupportType
        {
            Support,
            Resistance
        }

    }
}
