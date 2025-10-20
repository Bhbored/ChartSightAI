using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.MVVM.Models
{
    public record Option<T>(T Value, string Label);
}
