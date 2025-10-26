using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChartSightAI.Converters
{
    public sealed class IndicatorShortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var raw = (value?.ToString() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            var main = Regex.Replace(raw, @"\s*\(.*?\)\s*", string.Empty).Trim();

            switch (main.ToUpperInvariant())
            {
                case "RSI":
                case "RELATIVE STRENGTH INDEX": return "RSI";

                case "EMA":
                case "EXPONENTIAL MOVING AVERAGE": return "EMA";

                case "SMA":
                case "SIMPLE MOVING AVERAGE": return "SMA";

                case "MACD":
                case "MOVING AVERAGE CONVERGENCE DIVERGENCE": return "MACD";

                case "VWAP":
                case "VOLUME-WEIGHTED AVERAGE PRICE": return "VWAP";

                case "BOLLINGER BANDS":
                case "BOLLINGER":
                case "BB": return "BB";

                case "STOCH":
                case "STOCHASTIC":
                case "STOCHASTIC OSCILLATOR": return "STOCH";

                case "MA":
                case "MOVING AVERAGES": return "MA";

                case "VOLUME": return "VOLUME";

                case "ADX":
                case "AVERAGE DIRECTIONAL INDEX": return "ADX";

                case "ATR":
                case "AVERAGE TRUE RANGE": return "ATR";

                case "ICHIMOKU":
                case "ICHIMOKU CLOUD": return "ICHIMOKU";

                default:
                    // Fallback: if already a short token, return it; otherwise initials.
                    var parts = main.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1) return parts[0].ToUpperInvariant();
                    var initials = string.Concat(Array.ConvertAll(parts, p => char.ToUpperInvariant(p[0])));
                    return initials;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}
