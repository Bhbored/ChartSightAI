using ChartSightAI.MVVM.Models;
using ChartSightAI.MVVM.Views;
using ChartSightAI.Popups;
using ChartSightAI.Services;
using ChartSightAI.Services.Repos;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class HistoryVM : ObservableObject
    {
        private readonly AuthService _auth;
        private readonly AnalysisSessionRepo _sessionsRepo;

        public HistoryVM(AuthService auth, AnalysisSessionRepo repo)
        {
            _auth = auth;
            _sessionsRepo = repo;
        }

        #region Properties
        [ObservableProperty] private string? choice;
        [ObservableProperty] private ObservableCollection<AnalysisSession> _sessions = new();
        #endregion

        #region Commands
        [RelayCommand]
        private async Task OpenDetails(AnalysisSession selectedSession)
        {
            if (selectedSession is null) return;
            await Shell.Current.GoToAsync(nameof(AnalysisDetails),
                new Dictionary<string, object> { { "Session", selectedSession } });
        }

        [RelayCommand]
        private async Task RateSession(AnalysisSession session)
        {
            if (session is null) return;

            await Shell.Current.ShowPopupAsync(new RatePopup(this));
            var id = await _auth.GetUserIdAsync();

            if (Choice == "Hit")
            {
                session.IsRated = true;
                session.Hit = true;
                await _sessionsRepo.UpdateAsync((Guid)id!, session);
            }
            else if (Choice == "Miss")
            {
                session.IsRated = true;
                session.Hit = false;
                await _sessionsRepo.UpdateAsync((Guid)id!, session);
            }
            else
            {
                return;
            }

            var ix = Sessions.IndexOf(session);
            if (ix >= 0) Sessions[ix] = session;

            var uid = await _auth.GetUserIdAsync();
            if (uid is null) return;
            await _sessionsRepo.InsertAsync(uid.Value, session);
        }

        [RelayCommand]
        private async Task DeleteSession(AnalysisSession session)
        {
            if (session is null) return;

            bool ok = await Shell.Current.DisplayAlert("Delete", "Remove this item?", "Delete", "Cancel");
            if (!ok) return;

            Sessions.Remove(session);

            var uid = await _auth.GetUserIdAsync();
            if (uid is null) return;
            await _sessionsRepo.DeleteAsync(uid.Value, session.Id);
        }

        [RelayCommand]
        private async Task ShareAnalysis(AnalysisSession session)
        {
            if (session is null) return;

            var sb = new StringBuilder();
            sb.AppendLine("ChartSightAI – Analysis Session");
            sb.AppendLine($"{session.MarketType} • {session.TimeFrame} • {session.TradeDirection}");
            sb.AppendLine($"Date: {session.CreatedAt:yyyy-MM-dd}");

            if (session.Result != null)
            {
                if (!string.IsNullOrWhiteSpace(session.Result.Summary))
                    sb.AppendLine().AppendLine("Summary: " + session.Result.Summary);

                if (!string.IsNullOrWhiteSpace(session.Result.TrendAnalysis))
                    sb.AppendLine("Trend: " + session.Result.TrendAnalysis);

                if (session.Result.TradeIdea != null)
                {
                    sb.AppendLine().AppendLine("Trade Idea:");
                    sb.AppendLine($"Entry: {session.Result.TradeIdea.Entry:F2}");
                    sb.AppendLine($"Stop: {session.Result.TradeIdea.StopLoss:F2}");
                    if (session.Result.TradeIdea.Targets?.Count > 0)
                        sb.AppendLine($"Targets: {string.Join(", ", session.Result.TradeIdea.Targets.Select(t => t.ToString("F2")))}");
                }
            }

            await Microsoft.Maui.ApplicationModel.DataTransfer.Share.Default.RequestAsync(
                new Microsoft.Maui.ApplicationModel.DataTransfer.ShareTextRequest
                {
                    Title = "ChartSightAI Analysis",
                    Text = sb.ToString()
                });
        }
        #endregion

        #region Methods
        public void Hit() => Choice = "Hit";
        public void Miss() => Choice = "Miss";
        #endregion

        #region Tasks
        public async Task InitializeAsync()
        {
            try
            {
                Sessions.Clear();
                var userId = await _auth.GetUserIdAsync();
                var items = await _sessionsRepo.GetByDateRange(userId.Value, from: null, to: null, limit: 200);
                foreach (var s in items)
                    Sessions.Add(s);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
        #endregion
    }
}
