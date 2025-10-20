using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.Converters
{
    public class TimeFrameToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeFrame tf) return ChartSightAI.Utility.DisplayText.TimeFrameLabel(tf);
            return "Any TF";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => BindableProperty.UnsetValue;
    }
}
