using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.Converters
{
    public class MarketTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MarketType mt) return Utility.DisplayText.Market(mt);
            return "Any Market";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => BindableProperty.UnsetValue;
    }
}
