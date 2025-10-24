using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.Converters
{
    public class SupportResistanceToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return "—";

            var t = value.GetType();
            object get(string n) => t.GetRuntimeProperty(n)?.GetValue(value);

            var type = get("Type") ?? get("Kind") ?? get("Label");
            var price = get("Price") ?? get("Level") ?? get("Value");
            var conf = get("Confidence") ?? get("Weight");

            string sType = type?.ToString();
            string sPrice = price?.ToString();
            string sConf = conf is null ? null :
                conf is double d ? $"{(d <= 1 ? d * 100 : d):0}%" :
                conf is float f ? $"{(f <= 1 ? f * 100 : f):0}%" :
                conf.ToString();

            if (sType == null && sPrice == null) return value.ToString();
            if (sType != null && sPrice != null)
                return sConf == null ? $"{sType} @ {sPrice}" : $"{sType} @ {sPrice}  (Conf: {sConf})";

            return sType ?? sPrice;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
