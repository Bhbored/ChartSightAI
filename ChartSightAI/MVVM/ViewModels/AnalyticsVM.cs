using ChartSightAI.MVVM.Models;
using ChartSightAI.Services;
using ChartSightAI.Services.Repos;
using ChartSightAI.Utility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class AnalyticsVM : ObservableObject
    {
        #region Fields
        DateTime? _rangeStart = null;
        bool _subscribed;
        #endregion

        #region Properties
        [ObservableProperty] int totalRated;
        [ObservableProperty] int hitCount;
        [ObservableProperty] int missCount;
        [ObservableProperty] double hitRate;
        [ObservableProperty] string lastFrom = "From —";
        [ObservableProperty] string lastTo = "To —";
        [ObservableProperty] DateTime _chartMinDate;
        [ObservableProperty] DateTime _chartMaxDate;
        #endregion

        private readonly AuthService _auth;
        private readonly AnalysisSessionRepo _sessionsRepo;
        public AnalyticsVM(AuthService auth, AnalysisSessionRepo sessionRepo)
        {
            _auth = auth;
            _sessionsRepo = sessionRepo;
        }

        #region Collections
        [ObservableProperty] ObservableCollection<AnalysisSession> _sessions = new();
        public ObservableCollection<DonutSlice> HitRing { get; } = new();
        public ObservableCollection<DatePoint> TimeSeries { get; } = new();
        public ObservableCollection<NameValue> MarketBreakdown { get; } = new();
        public ObservableCollection<NameValue> DirectionBreakdown { get; } = new();
        public ObservableCollection<TFPerf> TimeframePerf { get; } = new();
        public ObservableCollection<DayCount> ActivityByDay { get; } = new();
        #endregion

        #region Commands
        [RelayCommand]
        public Task SetRangeAsync(string parameter)
        {
            _rangeStart = parameter switch
            {
                "7" => DateTime.Now.Date.AddDays(-7),
                "30" => DateTime.Now.Date.AddDays(-30),
                "90" => DateTime.Now.Date.AddDays(-90),
                _ => null
            };
            return RebuildAsync();
        }

        [RelayCommand]
        public async Task ExportCsvAsync()
        {
            var rated = GetRated().OrderBy(s => s.CreatedAt).ToList();
            var sb = new StringBuilder().AppendLine("Id,Date,Market,TimeFrame,Direction,Hit");
            foreach (var s in rated)
                sb.AppendLine($"{s.Id},{s.CreatedAt:yyyy-MM-dd},{s.MarketType},{s.TimeFrame},{s.TradeDirection},{(s.Hit ? 1 : 0)}");

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "Analytics CSV",
                Text = sb.ToString()
            });
        }

        [RelayCommand] public Task ShowDetailsAsync() => Task.CompletedTask;
        #endregion

        #region Methods
        public async Task InitializeAsync()
        {
            try
            {
                Sessions.Clear();

                await _auth.InitializeAsync();
                var userId = await _auth.GetUserIdAsync();
                if (userId is null)
                {
                    await Shell.Current.DisplayAlert("Not signed in", "Please log in to load your analytics.", "OK");
                    await Shell.Current.GoToAsync("//LoginPage");
                    return;
                }

                var items = await _sessionsRepo.GetByDateRange(userId.Value, from: null, to: null, limit: 400);
                foreach (var s in items)
                    Sessions.Add(s);

                if (!_subscribed)
                {
                    Sessions.CollectionChanged += OnItemsChanged;
                    _subscribed = true;
                }

                await SetRangeAsync("ALL");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        public void Unsubscribe()
        {
            if (_subscribed)
            {
                Sessions.CollectionChanged -= OnItemsChanged;
                _subscribed = false;
            }
        }

        void OnItemsChanged(object? s, NotifyCollectionChangedEventArgs e) => _ = RebuildAsync();

        async Task RebuildAsync()
        {
            await Task.Yield();

            var rated = GetRated().OrderBy(s => s.CreatedAt).ToList();

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

            HitRing.Clear();
            HitRing.Add(new DonutSlice { Name = "Hit", Value = HitRate });
            HitRing.Add(new DonutSlice { Name = "Remaining", Value = 100 - HitRate });

            TimeSeries.Clear();
            var grouped = rated.GroupBy(s => s.CreatedAt.Date).OrderBy(g => g.Key);
            double? ema = null;
            const double alpha = 0.25;
            foreach (var g in grouped)
            {
                var total = g.Count();
                var hits = g.Count(x => x.Hit);
                var rate = total == 0 ? 0 : (double)hits / total * 100.0;
                ema = ema is null ? rate : alpha * rate + (1 - alpha) * ema.Value;
                var pretty = Math.Clamp(ema.Value, 40, 90);
                TimeSeries.Add(new DatePoint { Date = g.Key, HitRate = pretty });
            }

            if (TimeSeries.Count > 0)
            {
                ChartMinDate = TimeSeries[0].Date.AddDays(-2);
                ChartMaxDate = TimeSeries[^1].Date.AddDays(1);
            }
            else
            {
                var today = DateTime.Now.Date;
                ChartMinDate = today.AddDays(-7);
                ChartMaxDate = today;
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
                var t = g.Count();
                var h = g.Count(x => x.Hit);
                var hr = t == 0 ? 0 : (double)h / t * 100.0;
                TimeframePerf.Add(new TFPerf { TimeFrame = g.Key, HitRate = hr });
            }

            ActivityByDay.Clear();
            foreach (var d in Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>())
            {
                var count = rated.Count(s => s.CreatedAt.DayOfWeek == d);
                ActivityByDay.Add(new DayCount { Day = d.ToString().Substring(0, 3), Count = count });
            }
        }

        IEnumerable<AnalysisSession> GetRated()
        {
            var q = Sessions.Where(s => s.IsRated && s.Result != null);
            if (_rangeStart != null) q = q.Where(s => s.CreatedAt.Date >= _rangeStart.Value);
            return q;
        }
        #endregion
    }

    public class DonutSlice { public string Name { get; set; } = ""; public double Value { get; set; } }
    public class DatePoint { public DateTime Date { get; set; } public double HitRate { get; set; } }
    public class NameValue { public string Name { get; set; } = ""; public double Value { get; set; } }
    public class TFPerf { public string TimeFrame { get; set; } = ""; public double HitRate { get; set; } }
    public class DayCount { public string Day { get; set; } = ""; public double Count { get; set; } }
}
