using ChartSightAI.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.Converters
{
    public class TradeDirectionToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TradeDirection d) return DisplayText.Direction(d);
            return "Any Dir";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => BindableProperty.UnsetValue;
    }
}
