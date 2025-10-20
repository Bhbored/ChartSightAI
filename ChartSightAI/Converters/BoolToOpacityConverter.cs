using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public double TrueOpacity { get; set; } = 1.0;
        public double FalseOpacity { get; set; } = 0.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isTrue = value is bool b && b;

            double trueOp = TrueOpacity;
            double falseOp = FalseOpacity;

            if (parameter is string s && !string.IsNullOrWhiteSpace(s))
            {
                var parts = s.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 1 && double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var t))
                    trueOp = t;
                if (parts.Length >= 2 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
                    falseOp = f;
            }

            return isTrue ? trueOp : falseOp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => BindableProperty.UnsetValue;
    }
}
