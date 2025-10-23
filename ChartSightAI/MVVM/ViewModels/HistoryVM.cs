using ChartSightAI.MVVM.Models;
using ChartSightAI.Popups;
using ChartSightAI.Utility;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class HistoryVM : ObservableObject
    {
        public ObservableCollection<AnalysisSession> Sessions
        {
            get
            {
                return sessions;
            }
            set => sessions = value;
        }


        [ObservableProperty]
        private string? choice;
        private ObservableCollection<AnalysisSession> sessions = new ObservableCollection<AnalysisSession>();

        #region Commands

        public ICommand RateSessionAsyncCommand => new Command<AnalysisSession>(async (session) => await RateSessionAsync(session));
        public ICommand DeleteSessionAsyncCommand => new Command<AnalysisSession>(async (session) => await DeleteSessionAsync(session));
        public ICommand ShareAnalysisAsyncCommand => new Command<AnalysisSession>(async (session) => await ShareAnalysisAsync(session));

        #endregion

        #region methods
        public void Hit()
        {
            Choice = "Hit";
        }
        public void Miss()
        {
            Choice = "Miss";

        }
        #endregion
        #region Tasks
        public async Task RateSessionAsync(AnalysisSession? session)
        {
            if (session is null) return;

            await Shell.Current.ShowPopupAsync(new RatePopup(this));
            if (Choice == "Hit")
            {
                session.IsRated = true;
                session.Hit = true;
            }
            else if (Choice == "Miss")
            {
                session.IsRated = true;
                session.Hit = false;
            }
            else
            {
                return;
            }

            var ix = Sessions.IndexOf(session);
            if (ix >= 0)
            {
                Sessions[ix] = session;
            }
        }
        private async Task DeleteSessionAsync(AnalysisSession? session)
        {
            if (session is null) return;
            bool ok = await Application.Current.MainPage.DisplayAlert("Delete", "Remove this item?", "Delete", "Cancel");
            if (!ok) return;

            Sessions.Remove(session);
            AnalysisSessionStore.Items.Remove(session);
        }
        private async Task ShareAnalysisAsync(AnalysisSession? session)
        {
            if (session is null) return;

            var sb = new System.Text.StringBuilder();
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
        public  Task InitializeAsync()
        {
            Sessions.Clear();
            var sessions = AnalysisSessionStore.Items; //change here
            foreach (var item in sessions)
            {
                Sessions.Add(item);
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}

