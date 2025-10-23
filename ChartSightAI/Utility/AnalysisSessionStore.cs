using ChartSightAI.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.Utility
{
    public static class AnalysisSessionStore
    {
        public static ObservableCollection<AnalysisSession> Items { get; } = new();
    }
}
