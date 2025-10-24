// AnalysisDetails.xaml.cs
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using ChartSightAI.MVVM.Models;

namespace ChartSightAI.MVVM.Views
{
    [QueryProperty(nameof(Session), "Session")]
    public partial class AnalysisDetails : ContentPage
    {
        private AnalysisSession? _session;
        public AnalysisSession? Session
        {
            get => _session;
            set
            {
                _session = value;
                RecomputeConfidence();
                OnPropertyChanged(nameof(Session));
                OnPropertyChanged(nameof(HeroImage));
            }
        }

        public string? HeroImage =>
            string.IsNullOrWhiteSpace(Session?.PreviewImage) ? Session?.ImagePath : Session!.PreviewImage;

        public bool HasConfidence { get; private set; }
        public string ConfidenceText { get; private set; } = string.Empty;


        public AnalysisDetails()
        {
            InitializeComponent();          
            BindingContext = this;
        }

        private void RecomputeConfidence()
        {
            HasConfidence = false;
            ConfidenceText = string.Empty;
            var res = Session?.Result;
            if (res == null) return;
            var prop = res.GetType().GetRuntimeProperty("Confidence");
            if (prop == null) return;
            var val = prop.GetValue(res);
            if (val == null) return;
            double pct = val switch
            {
                double d => d <= 1 ? d * 100 : d,
                float f => f <= 1 ? f * 100 : f,
                int i => i,
                _ => 0
            };
            if (pct <= 0) return;
            HasConfidence = true;
            ConfidenceText = $"Confidence: {pct:0}%";
            OnPropertyChanged(nameof(HasConfidence));
            OnPropertyChanged(nameof(ConfidenceText));
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            if (Session?.Result is null) return;
            var r = Session.Result;
            var sb = new StringBuilder();
            sb.AppendLine($"ChartSightAI — {Session.MarketType} • {Session.TimeFrame} • {Session.CreatedAt:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(r.Summary)) sb.AppendLine("\nSummary:\n" + r.Summary);
            if (!string.IsNullOrWhiteSpace(r.TrendAnalysis)) sb.AppendLine("\nTrend:\n" + r.TrendAnalysis);
            if (r.Indicators?.Count > 0) sb.AppendLine("\nIndicators: " + string.Join(", ", r.Indicators));
            if (r.SupportResistance?.Count > 0) sb.AppendLine("\nS/R: " + string.Join(" | ", r.SupportResistance.Select(o => o?.ToString())));
            if (r.TradeIdea != null) sb.AppendLine("\nTrade Idea:\n" + r.TradeIdea);
            if (!string.IsNullOrWhiteSpace(r.Explainability)) sb.AppendLine("\nExplainability:\n" + r.Explainability);
            await Share.Default.RequestAsync(new ShareTextRequest { Title = "Share / Export Analysis", Text = sb.ToString() });
        }
    }
}
