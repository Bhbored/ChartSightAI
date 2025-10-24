using ChartSightAI.MVVM.Models;
using ChartSightAI.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class AnalyticsVM : ObservableObject
    {
        [ObservableProperty] private int _totalRated;
        [ObservableProperty] private int _hitCount;
        [ObservableProperty] private int _missCount;
        [ObservableProperty] private double _hitRate;
        [ObservableProperty] private string _lastFrom = "From —";
        [ObservableProperty] private string _lastTo = "To —";

        public ObservableCollection<DatePoint> TimeSeries { get; } = new();
        public ObservableCollection<NameValue> MarketBreakdown { get; } = new();
        public ObservableCollection<NameValue> DirectionBreakdown { get; } = new();
        public ObservableCollection<TFPerf> TimeframePerf { get; } = new();
        public ObservableCollection<NameValue> TopIndicators { get; } = new();

        bool _subscribed;

        public async Task InitializeAsync()
        {
            if (!_subscribed)
            {
                AnalysisSessionStore.Items.CollectionChanged += OnItemsChanged;
                _subscribed = true;
            }
            await RebuildAsync();
        }

        public void Unsubscribe()
        {
            if (_subscribed)
            {
                AnalysisSessionStore.Items.CollectionChanged -= OnItemsChanged;
                _subscribed = false;
            }
        }

        void OnItemsChanged(object? s, NotifyCollectionChangedEventArgs e)
        {
            _ = RebuildAsync();
        }

        [RelayCommand]
        public Task RefreshAsync() => RebuildAsync();

        async Task RebuildAsync()
        {
            await Task.Yield();

            var rated = AnalysisSessionStore.Items
                .Where(s => s.IsRated && s.Result != null)
                .OrderBy(s => s.CreatedAt)
                .ToList();

            TotalRated = rated.Count;
            HitCount = rated.Count(s => s.Hit);
            MissCount = TotalRated - HitCount;
            HitRate = TotalRated == 0 ? 0 : Math.Round((double)HitCount / TotalRated * 100, 1);

            if (rated.Count > 0)
            {
                LastFrom = $"From {rated.First().CreatedAt:yyyy-MM-dd}";
                LastTo = $"To {rated.Last().CreatedAt:yyyy-MM-dd}";
            }
            else
            {
                LastFrom = "From —";
                LastTo = "To —";
            }

            TimeSeries.Clear();
            foreach (var g in rated.GroupBy(s => s.CreatedAt.Date).OrderBy(g => g.Key))
            {
                var hits = g.Count(x => x.Hit);
                var misses = g.Count(x => !x.Hit);
                var total = hits + misses;
                var hr = total == 0 ? 0 : (double)hits / total * 100.0;
                TimeSeries.Add(new DatePoint { Date = g.Key, Hits = hits, Misses = misses, HitRate = hr });
            }

            MarketBreakdown.Clear();
            foreach (var g in rated.GroupBy(s => s.MarketType.ToString()))
                MarketBreakdown.Add(new NameValue { Name = g.Key, Value = g.Count() });

            DirectionBreakdown.Clear();
            foreach (var g in rated.GroupBy(s => s.TradeDirection.ToString()))
                DirectionBreakdown.Add(new NameValue { Name = g.Key, Value = g.Count() });

            TimeframePerf.Clear();
            foreach (var g in rated.GroupBy(s => s.TimeFrame.ToString()))
            {
                var h = g.Count(x => x.Hit);
                var m = g.Count() - h;
                TimeframePerf.Add(new TFPerf
                {
                    TimeFrame = g.Key,
                    Hits = h,
                    Misses = m,
                    HitRate = g.Count() == 0 ? 0 : (double)h / g.Count() * 100.0
                });
            }

            TopIndicators.Clear();
            var indicators = rated
                .Where(s => s.Result?.Indicators != null)
                .SelectMany(s => s.Result!.Indicators)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(g => new NameValue { Name = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .Take(10);
            foreach (var it in indicators) TopIndicators.Add(it);
        }
    }

    public class DatePoint
    {
        public DateTime Date { get; set; }
        public int Hits { get; set; }
        public int Misses { get; set; }
        public double HitRate { get; set; }
    }

    public class NameValue
    {
        public string Name { get; set; } = "";
        public double Value { get; set; }
    }

    public class TFPerf
    {
        public string TimeFrame { get; set; } = "";
        public int Hits { get; set; }
        public int Misses { get; set; }
        public double HitRate { get; set; }
    }
}
