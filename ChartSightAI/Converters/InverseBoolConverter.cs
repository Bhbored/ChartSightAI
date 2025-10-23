using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is bool b ? !b : true;

        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => value is bool b ? !b : false;
    }
}
