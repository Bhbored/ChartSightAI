using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChartSightAI.MVVM.Models.Enums;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class NewPredictionVM :ObservableObject
    {
        [ObservableProperty]
        private TimeFrame _selectedTimeFrame;

        public ObservableCollection<TimeFrame> TimeFrameOptions { get; }

        public NewPredictionVM()
        {
            TimeFrameOptions = new ObservableCollection<TimeFrame>(Enum.GetValues(typeof(TimeFrame)).Cast<TimeFrame>());
            SelectedTimeFrame = TimeFrame.Day1;
        }
    }
}
