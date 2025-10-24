using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.Converters
{
    public class TradeIdeaToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return "—";

            var t = value.GetType();
            string Get(string name)
                => t.GetRuntimeProperty(name)?.GetValue(value)?.ToString();

            var entry = Get("Entry") ?? Get("EntryPrice") ?? Get("Price");
            var sl = Get("StopLoss") ?? Get("SL");
            var tp = Get("TakeProfit") ?? Get("TP");
            var dir = Get("Direction") ?? Get("TradeDirection");
            var rrr = Get("RR") ?? Get("RRR") ?? Get("RiskReward");
            var note = Get("Note") ?? Get("Notes") ?? Get("Comment");

            string line(string label, string v) => string.IsNullOrWhiteSpace(v) ? null : $"{label}: {v}";

            var lines = new[] {
                line("Direction", dir),
                line("Entry", entry),
                line("Stop Loss", sl),
                line("Take Profit", tp),
                line("R:R", rrr),
                note
            }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            if (lines.Length > 0) return string.Join("\n", lines);

            var props = t.GetRuntimeProperties()
                         .Where(p => p.CanRead && p.GetMethod.IsPublic)
                         .Select(p => (p.Name, Val: p.GetValue(value)))
                         .Where(p => p.Val is not null)
                         .Take(6)
                         .Select(p => $"{p.Name}: {p.Val}");

            var sFallback = string.Join("\n", props);
            return string.IsNullOrWhiteSpace(sFallback) ? value.ToString() : sFallback;
        }

        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
}
