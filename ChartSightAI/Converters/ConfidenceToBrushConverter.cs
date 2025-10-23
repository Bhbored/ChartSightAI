using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.Converters
{
    public class ConfidenceToBrushConverter : IValueConverter
    {
        public Color LowBrush { get; set; } = Colors.LightGray;   
        public Color MidBrush { get; set; } = Colors.Orange;      
        public Color HighBrush { get; set; } = Colors.LimeGreen;  

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                if (d >= 80) return HighBrush;
                if (d >= 60) return MidBrush;
                return LowBrush;
            }
            return LowBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
